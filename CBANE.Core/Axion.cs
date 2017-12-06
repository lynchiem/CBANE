using System;

namespace CBANE.Core
{
    public class Axion
    {
        public int InputLayerIndex { get; private set; }
        public int InputNeuronIndex { get; private set; }
        public int OutputLayerIndex { get; private set; }
        public int OutputNeuronIndex { get; private set; }

        public double Weight = 0;

        public Axion(int inputLayerIndex, int inputNeuronIndex, int outputLayerIndex, int outputNeuronIndex)
        {
            this.InputLayerIndex = inputLayerIndex;
            this.InputNeuronIndex = inputNeuronIndex;
            this.OutputLayerIndex = outputLayerIndex;
            this.OutputNeuronIndex = outputNeuronIndex;
        }
    }
}
