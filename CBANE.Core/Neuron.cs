using System;
using System.Collections.Generic;

namespace CBANE.Core
{
    public enum ActivationTypes
    {
        Bias,
        Passthrough,
        Softstep,
        Softplus,
        ReLU,
        LeakyReLU,
        Softmax,
        TanH
    }

    public class Neuron
    {
        public double Input = 0;
        public ActivationTypes ActivationType = ActivationTypes.Passthrough;

        public Neuron(ActivationTypes activationType)
        {
            this.ActivationType = activationType;
        }

        public double GetOutput()
        {
            double output = 0;

            switch (this.ActivationType)
            {
                case ActivationTypes.Bias:
                    output = 1;
                    break;

                case ActivationTypes.TanH:
                    output = Math.Tanh(this.Input);
                    break;

                case ActivationTypes.Softstep:
                    output = NEMath.Softstep(this.Input);
                    break;

                case ActivationTypes.Softplus:
                    output = NEMath.Softplus(this.Input);
                    break;

                case ActivationTypes.ReLU:
                    output = NEMath.ReLU(this.Input);
                    break;

                case ActivationTypes.LeakyReLU:
                    output = NEMath.LeakyReLU(this.Input);
                    break;

                case ActivationTypes.Passthrough:
                default:
                    output = this.Input;
                    break;
            }
            
            return Math.Round(output, 6);
        }

    }
}
