using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using CBANE.Core;

namespace CBANE.Sandpit
{
    public enum EvaluationMode
    {
        TRAINING,
        TESTING
    }
    public class ExampleTrainer
    {
        public Supercluster Supercluster { get; private set; }
        public ulong Cycle { get; private set; }

        public double BestTestingScore { get; private set; }
        public double MaxTestingScore { get; private set; }

        public double BestTrainingScore { get; private set; }
        public double MaxTrainingScore { get; private set; }

        public List<NormalisedExampleRecord> TestingDataset;
        public List<NormalisedExampleRecord> TrainingDataset;

        private bool trainingEnable = false;
        private uint clusterRate;
        private uint mergingRate;
        private int maxThreads;

        private int maxAge;
        private double maxSpendCategoryA;
        private double maxSpendCategoryB;

        /// <summary>
        /// Construct a trainer to train a supercluster using the sandpit's example data.
        /// </summary>
        /// <param name="supercluster">Supercluster to train.</param>
        /// <param name="clusterRate">Number of subcycles between clustering.</param>
        /// <param name="mergingRate">Number of subcylces between merging.</param>
        /// <param name="maxThreads">Max number of concurrent threads to spawn during training.</param>
        public ExampleTrainer(Supercluster supercluster, uint clusterRate, uint mergingRate, int maxThreads)
        {
            this.Supercluster = supercluster;
            this.maxThreads = (maxThreads < 1)? 1 : maxThreads;

            this.Cycle = 0;

            this.clusterRate = clusterRate;
            this.mergingRate = mergingRate;

            // Load datasets.
            var testingDataset = this.LoadDatasetFromCSV("data/eg-testing-set.csv");
            var positiveTrainingDataset = this.LoadDatasetFromCSV("data/eg-training-set-positive.csv");
            var negativeTrainingDataset = this.LoadDatasetFromCSV("data/eg-training-set-negative.csv");

            var trainingDataset = positiveTrainingDataset.Concat(negativeTrainingDataset).ToList();

            // Find max values for normalisation.
            var completeDataset = trainingDataset.Concat(testingDataset);

            this.maxAge = completeDataset.Max(o => o.Age);
            this.maxSpendCategoryA = completeDataset.Max(o => o.SpendCategoryA);
            this.maxSpendCategoryB = completeDataset.Max(o => o.SpendCategoryA);

            // Normalise datasets.
            this.TestingDataset = this.NormaliseDataset(testingDataset);
            this.TrainingDataset = this.NormaliseDataset(trainingDataset);

            this.BestTestingScore = double.MinValue;
            this.MaxTestingScore = this.TestingDataset.Count;

            this.BestTrainingScore = double.MinValue;
            this.MaxTrainingScore = this.TrainingDataset.Count;
        }

        private List<ExampleRecord> LoadDatasetFromCSV(string filePath)
        {
            var records = new List<ExampleRecord>();

            var lines = File.ReadAllLines(filePath);
            var values = (
                from line in lines
                select (line.Split(',')).ToArray()
            ).ToArray();

            for(var i = 1; i < values.Length; i++)
            {
                int age = 0;
                double spendCategoryA = 0;
                double spendCategoryB = 0;
                int performedAction = 0;

                int.TryParse(values[i][0], out age);
                double.TryParse(values[i][1], out spendCategoryA);
                double.TryParse(values[i][2], out spendCategoryB);
                int.TryParse(values[i][3], out performedAction);

                records.Add(new ExampleRecord(age, spendCategoryA, spendCategoryB, performedAction == 1));
            }

            return records;
        }

        private List<NormalisedExampleRecord> NormaliseDataset(List<ExampleRecord> dataset)
        {
            var normalisedRecords = new List<NormalisedExampleRecord>();

            foreach(var record in dataset)
                normalisedRecords.Add(new NormalisedExampleRecord(record, this.maxAge, this.maxSpendCategoryA, this.maxSpendCategoryB));

            return normalisedRecords.OrderBy(o => NEMath.Random()).ToList();
        }

        public void StopTraining()
        {
            this.trainingEnable = false;
        }

        public void Train(ulong cycles)
        {
            this.trainingEnable = true; 
            
            ulong lastCycle = this.Cycle + cycles;
            ulong lastCluster = 0;
            ulong lastMerge = 0;

            this.Evaluate(EvaluationMode.TRAINING, true);

            while(this.trainingEnable && this.Cycle < lastCycle)
            {
                var merge = (this.Cycle == lastMerge + this.mergingRate);
                var cluster = (this.Cycle == 0 || this.Cycle == lastCluster + this.clusterRate);

                var includeArchived = (merge || cluster);

                this.Supercluster.Evolve();
                this.Evaluate(EvaluationMode.TRAINING, includeArchived);

                if(merge)
                {
                    this.Supercluster.Merge();
                    this.Evaluate(EvaluationMode.TRAINING, true);

                    lastMerge = this.Cycle;
                }

                if(merge || cluster)
                {
                    this.Supercluster.Cluster();

                    lastCluster = this.Cycle;
                }

                this.Cycle += 1;
            }
        }

        public void Evaluate(EvaluationMode evaluationMode, bool includeArchived = false)
        {
            var dataset = (evaluationMode == EvaluationMode.TESTING) ? this.TestingDataset : this.TrainingDataset;

            for(var i = 0; i < this.Supercluster.Clusters.Count; i++)
            {
                var cluster = this.Supercluster.Clusters[i];

                this.EvaluateNetworks(cluster.Networks, dataset);
            }

            if(includeArchived)
                this.EvaluateNetworks(this.Supercluster.NetworkArchive, dataset);

            var allNetworks = this.Supercluster.GetAllNetworks(true);
            var bestScore = allNetworks.Max(o => o.Strength);

            if(evaluationMode == EvaluationMode.TRAINING && bestScore > this.BestTrainingScore)
                this.BestTrainingScore = bestScore;
            else if(evaluationMode == EvaluationMode.TESTING && bestScore > this.BestTestingScore)
                this.BestTestingScore = bestScore;
        }
        
        private void EvaluateNetworks(List<Network> networks, List<NormalisedExampleRecord> dataset)
        {
            var evaluated = 0;

            while (evaluated < networks.Count)
            {
                var backgroundTasks = new List<Task>();

                foreach (var network in networks.Skip(evaluated).Take(this.maxThreads))
                {
                    backgroundTasks.Add(Task.Run(() => EvaluateNetwork(network, dataset)));
                }

                Task.WaitAll(backgroundTasks.ToArray());
                Thread.Sleep(50);

                evaluated += maxThreads;
            }
        }

        private void EvaluateNetwork(Network network, List<NormalisedExampleRecord> dataset)
        {
            var strength = 0.0;

            foreach(var record in dataset)
            {
                network.Neurons[0][0].Input = 1.0; // Bias
                network.Neurons[0][1].Input = record.Age;
                network.Neurons[0][2].Input = record.SpendCategoryA;
                network.Neurons[0][3].Input = record.SpendCategoryB;

                var results = network.Query();

                if(record.PerformedAction)
                {
                    strength += results[0];
                }
                else
                {
                    strength += results[1];
                }
            }

            network.UpdateStrength(strength);
        }

    }
}