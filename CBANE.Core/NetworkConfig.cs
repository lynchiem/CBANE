using System;
using System.Collections.Generic;
using System.Text;

namespace CBANE.Core
{
    public class NetworkConfig
    {
        /// <summary>
        /// Number of input rows (neurons).
        /// </summary>
        public int InputRows = 0;

        /// <summary>
        /// The activation function to use for input neurons.
        /// </summary>
        public ActivationTypes InputActivation = ActivationTypes.Passthrough;

        /// <summary>
        /// The maximum input value. Required to calculate clustering vector.
        /// </summary>
        public double MaxInputValue = 1.0;

        /// <summary>
        /// Number of output rows (neurons).
        /// </summary>
        public int OutputRows = 0;

        /// <summary>
        /// The activation function to use for output neurons.
        /// </summary>
        public ActivationTypes OutputActivation = ActivationTypes.Passthrough;

        /// <summary>
        /// The number of hidden columns (layers).
        /// </summary>
        public int HiddenColumns = 0;

        /// <summary>
        /// The number of hidden rows (neurons per layer).
        /// </summary>
        public int HiddenRows = 0;

        /// <summary>
        /// The activation function to use for hidden neurons.
        /// </summary>
        public ActivationTypes HiddenActivation = ActivationTypes.ReLU;

        /// <summary>
        /// <para>The maximum number of cycles to apply during a mutation.</para>
        /// 
        /// <para>The actual number of cycles will randomly fall between 1 and MaxMutationCycles, 
        /// but will be biased by MutationCycleBias.</para>
        /// </summary>
        public int MaxMutationCycles = 8;

        /// <summary>
        /// <para>The bias to apply when randomly selecting the number of mutation cycles.</para>
        /// 
        /// <para>MutationCycleBias will be passed as a biasing power to NEMath.Random(...).</para>
        /// <seealso cref="NEMath.Random(double)"/>
        /// </summary>
        public double MutationCycleBias = 5.0;

        /// <summary>
        /// <para>The chance for a selected axion's weight to be mutated during a mutation cycle.</para>
        /// 
        /// <para>Each mutation cycle, one axion will be selected at random and this field defines
        /// the chance that axion's weight will be mutated.</para>
        /// </summary>
        public double AxionMutationRate = 0.90;

        /// <summary>
        /// <para>The chance that a selected axion's weight will be completely replaced with a 
        /// new random weight, rather than simply being mutated.</para>
        /// 
        /// <para>The replacement rate should be less than or equal to AxionMutationRate.</para>
        /// </summary>
        public double AxionReplacementRate = 0.10;

        /// <summary>
        /// <para>When performing crossover (breeding), the CrossoverBias can be used to give
        /// the stronger network a greater chance of passing on its weight.</para>
        /// 
        /// <para>The biasing function is simply randomBetween(0.5 and 0.5 + CrossoverBias).</para>
        /// </summary>
        public double CrossoverBias = 0.25;

        public NetworkConfig(int inputRows, int outputRows, int hiddenColumns, int hiddenRows)
        {
            this.InputRows = inputRows;
            this.OutputRows = outputRows;
            this.HiddenColumns = hiddenColumns;
            this.HiddenRows = hiddenRows;
        }

        private NetworkConfig() {}

        public NetworkConfig Clone()
        {
            NetworkConfig clone = new NetworkConfig()
            {
                InputRows = this.InputRows,
                InputActivation = this.InputActivation,

                OutputRows = this.OutputRows,
                OutputActivation = this.OutputActivation,

                HiddenColumns = this.HiddenColumns,
                HiddenRows = this.HiddenRows,
                HiddenActivation = this.HiddenActivation,

                MaxMutationCycles = this.MaxMutationCycles,
                MutationCycleBias = this.MutationCycleBias,

                AxionMutationRate = this.AxionMutationRate,
                AxionReplacementRate = this.AxionReplacementRate,

                CrossoverBias = this.CrossoverBias
            };

            return clone;
        }

    }
}
