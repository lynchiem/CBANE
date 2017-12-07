using System;
using System.Linq;

using Newtonsoft.Json;

namespace CBANE.Core
{
    public enum NetworkOrigins
    {
        UNKNOWN,
        BOOTSTRAP,
        CLONE,
        CROSSOVER,
        TRAVELLER
    }

    public class Network
    {
        public Guid UniqueId { get; private set; }

        public NetworkOrigins Origin { get; private set; }
        public int AncestralGeneration { get; private set; }
        public int CreationGeneration { get; private set; }

        public double Strength { get; private set; }
        public int StagnantEvolutions { get; private set; }

        public Neuron[][] Neurons = null;
        public Axion[][] Axions = null;

        private NetworkConfig config = null;

        public Network(NetworkConfig networkConfig, NetworkOrigins origin, int creationGeneration = 0)
        {
            this.UniqueId = Guid.NewGuid();

            this.Origin = origin;
            this.AncestralGeneration = 0;
            this.CreationGeneration = creationGeneration;

            this.config = networkConfig;

            this.Strength = double.MinValue;
            this.StagnantEvolutions = 0;

            this.Neurons = new Neuron[this.config.HiddenColumns + 2][];
            this.Axions = new Axion[this.config.HiddenColumns + 1][];

            this.ConstructNeurons();
            this.ConstructAxions();
        }

        public string GetAxionJson()
        {
            return JsonConvert.SerializeObject(this.Axions);
        }

        public void UpdateStrength(double strength)
        {
            if (strength <= this.Strength)
                this.StagnantEvolutions += 1;
            else
                this.StagnantEvolutions = 0;

            this.Strength = strength;
        }

        private void ConstructNeurons()
        {
            // Input Layer
            var layerIndex = 0;

            this.Neurons[layerIndex] = new Neuron[this.config.InputRows + 1];
            this.Neurons[layerIndex][0] = new Neuron(ActivationTypes.Bias);

            for (var i = 1; i < this.config.InputRows + 1; i++)
                this.Neurons[layerIndex][i] = new Neuron(this.config.InputActivation);

            // Hidden Layers
            for (var i = this.config.HiddenColumns; i >= 1; i--)
            {
                layerIndex += 1;

                this.Neurons[layerIndex] = new Neuron[this.config.HiddenRows + 1];
                this.Neurons[layerIndex][0] = new Neuron(ActivationTypes.Bias);

                for (var j = 1; j < this.config.HiddenRows + 1; j++)
                    this.Neurons[layerIndex][j] = new Neuron(this.config.HiddenActivation);

            }

            // Output Layer
            layerIndex += 1;

            this.Neurons[layerIndex] = new Neuron[this.config.OutputRows];

            for (var i = 0; i < this.config.OutputRows; i++)
                this.Neurons[layerIndex][i] = new Neuron(this.config.OutputActivation);

        }

        private void ConstructAxions()
        {
            for (var i = 0; i < this.Neurons.Length - 1; i++)
            {
                this.Axions[i] = new Axion[this.Neurons[i].Length * this.Neurons[i + 1].Length];

                var axionIndex = 0;

                for (var j = 0; j < this.Neurons[i].Length; j++)
                {
                    for (var k = 0; k < this.Neurons[i + 1].Length; k++)
                    {
                        this.Axions[i][axionIndex] = new Axion(i, j, i + 1, k);

                        axionIndex += 1;
                    }
                }
            }
        }

        public void LoadAxions(Axion[][] sampleAxions)
        {
            for (var i = 0; i < this.Axions.Length; i++)
            {
                for (var j = 0; j < this.Axions[i].Length; j++)
                {
                    var axion = this.Axions[i][j];
                    var sampleAxion = sampleAxions[i][j];

                    axion.Weight = sampleAxion.Weight;
                }
            }
        }

        public void RandomiseAxions()
        {
            for (var i = 0; i < this.Axions.Length; i++)
            {
                for (var j = 0; j < this.Axions[i].Length; j++)
                {
                    var axion = this.Axions[i][j];

                    // Randomise, but bias towards zero.

                    var randomWeight = NEMath.RandomBetween(0, 1, 0.25);

                    if (NEMath.Random() < 0.5)
                        randomWeight *= -1.0;

                    axion.Weight = randomWeight;
                }
            }
        }

        public double[] GetLayerInputVector(int layerIndex)
        {
            if (this.Neurons.Length <= layerIndex)
                return null;

            return this.Neurons[layerIndex].Select(o => o.Input).ToArray();
        }

        private void ResetNeuronInputs()
        {
            // Reset inputs for all neurons, except those in the input layer (0).
            // Input neurons are externally controlled.
            for (var i = 1; i < this.Neurons.Length; i++)
            {
                for (var j = 0; j < this.Neurons[i].Length; j++)
                    this.Neurons[i][j].Input = 0.0;
            }
        }

        private double GetNeuronOutput(int layerIndex, int neuronIndex, bool forceSoftmax = false)
        {
            double value = 0.0;

            var layerInputVector = this.GetLayerInputVector(layerIndex);
            var neuron = this.Neurons[layerIndex][neuronIndex];

            if (forceSoftmax || neuron.ActivationType == ActivationTypes.Softmax)
                value = NEMath.Softmax(layerInputVector, neuronIndex);
            else
                value = neuron.GetOutput();

            return value;
        }

        public double[] Query()
        {
            this.ResetNeuronInputs();

            // Process the axions in layer order. The order of axions within a layer
            // doesn't really matter, but layer order is extremely important.
            for (var i = 0; i < this.Axions.Length; i++)
            {
                for (var j = 0; j < this.Axions[i].Length; j++)
                {
                    var axion = this.Axions[i][j];

                    var value = this.GetNeuronOutput(axion.InputLayerIndex, axion.InputNeuronIndex);

                    var neuronOut = this.Neurons[axion.OutputLayerIndex][axion.OutputNeuronIndex];
                    neuronOut.Input += Math.Round(value * axion.Weight, 6);
                }
            }

            // Gather output values from neurons in the output layer, so they
            // can be returned as a simple vector.
            var outputVector = new double[this.config.OutputRows];

            var outputLayerIndex = this.Neurons.Length - 1;

            for (var i = 0; i < this.config.OutputRows; i++)
            {
                outputVector[i] = this.GetNeuronOutput(outputLayerIndex, i);
            }

            return outputVector;
        }

        public void HeavilyMutuate(double perAxionChance)
        {
            for (var i = 0; i < this.Axions.Length; i++)
            {
                for (var j = 0; j < this.Axions[i].Length; j++)
                {
                    var axion = this.Axions[i][j];

                    if (NEMath.RNG.NextDouble() < perAxionChance)
                        axion.Weight = NEMath.Clamp(axion.Weight + NEMath.RandomBetween(-0.5, 0.5), -1.0, 1.0); ;
                }
            }
        }

        public void Mutate()
        {
            var cycles = Math.Round(NEMath.RandomBetween(1, this.config.MaxMutationCycles, this.config.MutationCycleBias), 0);

            while (cycles > 0)
            {
                var chance = NEMath.Random();

                if (chance < this.config.AxionMutationRate)
                {
                    var layer = this.Axions[NEMath.RNG.Next(0, this.Axions.Length)];
                    var axion = layer[NEMath.RNG.Next(0, layer.Length)];

                    if (chance < this.config.AxionReplacementRate)
                    {
                        axion.Weight = NEMath.Clamp(NEMath.RandomBetween(-1, 1), -1.0, 1.0);
                    }
                    else
                    {
                        axion.Weight = NEMath.Clamp(axion.Weight + NEMath.RandomBetween(-0.1, 0.1), -1.0, 1.0);
                    }
                }

                cycles -= 1;
            }
        }

        public Network Clone(bool perfect = false)
        {
            Network clone = new Network(this.config.Clone(), NetworkOrigins.CLONE, this.CreationGeneration + 1)
            {
                AncestralGeneration = this.AncestralGeneration
            };

            for (var i = 0; i < this.Axions.Length; i++)
            {
                for (var j = 0; j < this.Axions[i].Length; j++)
                {
                    var selfAxion = this.Axions[i][j];
                    var cloneAxion = clone.Axions[i][j];

                    cloneAxion.Weight = selfAxion.Weight;
                }
            }

            if (!perfect)
                clone.Mutate();

            return clone;
        }

        public Network Crossover(Network partner, bool ignoreStrength = false)
        {
            var maxCreationGeneration = (this.CreationGeneration > partner.CreationGeneration) ? this.CreationGeneration : partner.CreationGeneration;

            var offspring = new Network(this.config.Clone(), NetworkOrigins.CROSSOVER, maxCreationGeneration + 1)
            {
                AncestralGeneration = this.AncestralGeneration
            };

            for (var i = 0; i < this.Axions.Length; i++)
            {
                for (var j = 0; j < this.Axions[i].Length; j++)
                {
                    var selfAxion = this.Axions[i][j];
                    var partnerAxion = partner.Axions[i][j];
                    var offspringAxion = offspring.Axions[i][j];

                    var strongestWeight = (this.Strength > partner.Strength) ? selfAxion.Weight : partnerAxion.Weight;
                    var weakestWeight = (this.Strength < partner.Strength) ? selfAxion.Weight : partnerAxion.Weight;

                    if (ignoreStrength)
                    {
                        strongestWeight = selfAxion.Weight;
                        weakestWeight = partnerAxion.Weight;
                    }

                    var chance = (NEMath.Random() * this.config.CrossoverBias + 0.50);
                    chance = (ignoreStrength) ? 0.5 : chance;

                    offspringAxion.Weight = (NEMath.Random() < chance) ? strongestWeight : weakestWeight;
                }
            }

            return offspring;
        }

    }
}
