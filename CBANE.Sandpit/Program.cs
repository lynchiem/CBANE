using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

using CBANE.Core;
using static CBANE.Core.NEMath;

namespace CBANE.Sandpit
{
    public enum ProgramStatus
    {
        UNKNOWN,
        IDLE,
        TRAINING,
        TESTING,
        OUTPUT,
        STOPPING,
        EXITING
    }

    class Program
    { 
        static bool running = true;

        static ExampleTrainer Trainer;

        static ProgramStatus outputStatus = ProgramStatus.UNKNOWN;

        static void Main(string[] args)
        {
            Console.WriteLine("CBANE SANDPIT");
            Console.WriteLine("-------------------------");
            Console.WriteLine("");

            var superclusterConfig = new SuperclusterConfig(5);
            var clusterConfig = new ClusterConfig(50, 0.20, 0.05);
            var networkConfig = new NetworkConfig(3, 2, 3, 12);

            networkConfig.InputActivation = ActivationTypes.Passthrough;
            networkConfig.OutputActivation = ActivationTypes.Softmax;
            networkConfig.HiddenActivation = ActivationTypes.LeakyReLU;

            var supercluster = new Supercluster(superclusterConfig, clusterConfig, networkConfig);

            supercluster.GenerateRandomPopulation();

            Trainer = new ExampleTrainer(supercluster, 25, 50, 15);

            outputStatus = ProgramStatus.IDLE;

            var outputTask = Task.Run(() => ManageOutput());

            Task trainingTask = null;

            while(running)
            {
                var keyInfo = Console.ReadKey(true);

                if(outputStatus == ProgramStatus.IDLE && keyInfo.Key == ConsoleKey.T)
                {
                    outputStatus = ProgramStatus.TRAINING;
                    trainingTask = Task.Run(() => PerformTrainingRun());
                }
                else if(outputStatus == ProgramStatus.IDLE && keyInfo.Key == ConsoleKey.O)
                {
                    outputStatus = ProgramStatus.OUTPUT;
                    
                    OutputBestWeights();

                    outputStatus = ProgramStatus.IDLE;
                }
                else if(outputStatus == ProgramStatus.TRAINING && keyInfo.Key == ConsoleKey.S)
                {
                    outputStatus = ProgramStatus.STOPPING;

                    Trainer.StopTraining();

                    if(trainingTask != null && !trainingTask.IsCompleted)
                        trainingTask.Wait();

                    outputStatus = ProgramStatus.IDLE;
                }
                else if(keyInfo.Key == ConsoleKey.X)
                {
                    outputStatus = ProgramStatus.EXITING;

                    Trainer.StopTraining();

                    if(trainingTask != null && !trainingTask.IsCompleted)
                        trainingTask.Wait();
                    
                    running = false;
                    outputTask.Wait();
                }
            }
        }

        static void PerformTrainingRun()
        {
            Trainer.Train(500);

            if(outputStatus != ProgramStatus.EXITING)
            {
                outputStatus = ProgramStatus.TESTING;

                Trainer.Evaluate(EvaluationMode.TESTING, true);

                outputStatus = ProgramStatus.IDLE;
            }
        }

        static void OutputPredictions(Network network)
        {
            List<string> outputLines = new List<string>();

            foreach(var record in Trainer.TestingDataset)
            {
                network.Neurons[0][0].Input = 1.0; // Bias
                network.Neurons[0][1].Input = record.Age;
                network.Neurons[0][2].Input = record.SpendCategoryA;
                network.Neurons[0][3].Input = record.SpendCategoryB;

                var results = network.Query();
                var predictAction = (results[0] > 0.5);

                outputLines.Add($"{record.Age.ToString()}, {record.SpendCategoryA.ToString()}, {record.SpendCategoryB.ToString()}, {record.PerformedAction.ToString()}, {results[0].ToString()}, {predictAction.ToString()}");
            
                File.WriteAllLines($"data/outputs/{network.UniqueId.ToString()}-predictions.csv", outputLines.ToArray());
            }
        }

        static void OutputBestWeights()
        {
            Trainer.Evaluate(EvaluationMode.TESTING, true);
            Trainer.Supercluster.Cluster();

            foreach(var cluster in Trainer.Supercluster.Clusters)
            {
                if(cluster.Networks.Count == 0)
                    continue;

                var bestNetwork = cluster.Networks.OrderByDescending(o => o.Strength).First();

                File.WriteAllText($"data/outputs/{bestNetwork.UniqueId.ToString()}.json", bestNetwork.GetAxionJson());

                OutputPredictions(bestNetwork);
            }
        }

        static void ManageOutput()
        {
            var previousStatus = ProgramStatus.UNKNOWN;
            var lastAutoUpdate = DateTime.Now.AddHours(-1);

            ulong lastTrainingCycle = 0;

            while(running)
            {
                var statusMessage = outputStatus.ToString().PadRight(8, '.');

                var now = DateTime.Now;

                if(outputStatus == ProgramStatus.TRAINING && previousStatus != ProgramStatus.TRAINING)
                {
                    Console.WriteLine($"[{statusMessage}] Starting 500 cycle training run...");
                    Console.WriteLine($"[{statusMessage}] Press [S] to stop.");
                }

                if(outputStatus == ProgramStatus.TRAINING && (now - lastAutoUpdate).TotalMilliseconds > 1000 && Trainer.Cycle > lastTrainingCycle)
                {
                    string trainingScore = (Trainer.BestTrainingScore == double.MinValue) ? "N/A" : $"{Trainer.BestTrainingScore / Trainer.MaxTrainingScore * 100:0.000000}%";
                    string trainingScoreMessage = $"[{statusMessage}] Best Score: {trainingScore}, Cycles: {Trainer.Cycle}";

                    Console.WriteLine(trainingScoreMessage);

                    lastAutoUpdate = DateTime.Now;
                    lastTrainingCycle = Trainer.Cycle;
                }

                if(outputStatus == ProgramStatus.STOPPING && previousStatus != ProgramStatus.STOPPING)
                {
                    string testingMessage = ProgramStatus.TRAINING.ToString().PadRight(8, '.');

                    Console.WriteLine($"[{testingMessage}] Stopping training run...");
                }

                if(outputStatus == ProgramStatus.TESTING && previousStatus != ProgramStatus.TESTING)
                {
                    Console.WriteLine($"[{statusMessage}] Testing networks...");
                }

                if(outputStatus != ProgramStatus.TESTING && previousStatus == ProgramStatus.TESTING)
                {
                    string testingMessage = ProgramStatus.TESTING.ToString().PadRight(8, '.');

                    string testingScore = (Trainer.BestTestingScore == double.MinValue) ? "N/A" : $"{Trainer.BestTestingScore / Trainer.MaxTestingScore * 100:0.000000}%";
                    string testingScoreMessage = $"[{testingMessage}] Best Score: {testingScore}, Cycles: {Trainer.Cycle}";

                    Console.WriteLine(testingScoreMessage);
                }

                if(outputStatus == ProgramStatus.OUTPUT && previousStatus != ProgramStatus.OUTPUT)
                {
                    Console.WriteLine($"[{statusMessage}] Outputing best weights...");
                }

                if(outputStatus == ProgramStatus.IDLE && previousStatus != ProgramStatus.IDLE)
                {
                    Console.WriteLine($"[{statusMessage}] Press [T] to train or [O] to output best weights.");
                    Console.WriteLine($"[{statusMessage}] Press [X] to exit.");
                }

                if(outputStatus == ProgramStatus.EXITING && previousStatus != ProgramStatus.EXITING)
                {
                    Console.WriteLine($"[{statusMessage}] Waiting on background tasks to stop...");
                }

                previousStatus = outputStatus;
                Thread.Sleep(250);
            }
        }

    }
}
