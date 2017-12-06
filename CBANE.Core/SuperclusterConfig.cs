using System;
using System.Collections.Generic;
using System.Text;

namespace CBANE.Core
{
    public class SuperclusterConfig
    {
        /// <summary>
        /// The maximum angle between two networks where they remain cluster compatible.
        /// </summary>
        public double ClusteringAngle = 20.0;

        /// <summary>
        /// The maximum number of clusters. Where possible the supercluster will
        /// attempt to maintain this number of clusters.
        /// </summary>
        public int MaxClusters = 5;

        /// <summary>
        /// The number of evolutions a network can be stagnant (no improvements) 
        /// before it is culled.
        /// </summary>
        public int MaxStagnantEvolutions = 10;

        /// <summary>
        /// The maximum number of networks the supercluster's archive can hold.
        /// </summary>
        public int MaxArchivedNetworks = 100;

        public SuperclusterConfig(int maxClusters)
        {
            this.MaxClusters = maxClusters;
        }


    }
}
