using System;
using System.Collections.Generic;
using System.Text;

namespace CBANE.Core
{
    public class ClusterConfig
    {
        /// <summary>
        /// The maximum number of networks the cluster should contain.
        /// </summary>
        public int MaxNetworks = 25;

        /// <summary>
        /// The ideal ratio of clone networks within the cluster.
        /// </summary>
        public double CloneRatio = 0.20;

        /// <summary>
        /// The ideal ratio of traveller networks within the cluster. 
        /// </summary>
        public double TravellerRatio = 0.05;

        /// <summary>
        /// The chance for heavy mutation to occur during the standard mutation/evolution cycle.
        /// </summary>
        public double HeavyMutationRate = 0.10;

        public ClusterConfig(int maxNetworks, double cloneRatio, double travellerRatio)
        {
            this.MaxNetworks = maxNetworks;
            this.CloneRatio = cloneRatio;
            this.TravellerRatio = travellerRatio;
        }

        private ClusterConfig() { }

        public ClusterConfig Clone()
        {
            var clone = new ClusterConfig()
            {
                MaxNetworks = this.MaxNetworks,
                CloneRatio = this.CloneRatio,
                TravellerRatio = this.TravellerRatio
            };

            return clone;
        }

    }
}
