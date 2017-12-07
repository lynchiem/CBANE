using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CBANE.Core
{
    public class Supercluster
    {
        public List<Cluster> Clusters = new List<Cluster>();
        public List<Network> NetworkArchive = new List<Network>();

        public SuperclusterConfig SuperclusterConfig;
        public ClusterConfig ClusterConfig;
        public NetworkConfig NetworkConfig;

        public Supercluster(SuperclusterConfig superclusterConfig, ClusterConfig clusterConfig, NetworkConfig networkConfig)
        {
            this.SuperclusterConfig = superclusterConfig;
            this.ClusterConfig = clusterConfig;
            this.NetworkConfig = networkConfig;
        }

        public void GenerateRandomPopulation()
        {
            this.Clusters.Clear();
            this.NetworkArchive.Clear();

            var maxNetworks = this.ClusterConfig.MaxNetworks * this.SuperclusterConfig.MaxClusters;

            while(this.NetworkArchive.Count < maxNetworks)
            {
                var network = new Network(this.NetworkConfig, NetworkOrigins.BOOTSTRAP, 0);
                network.RandomiseAxions();

                this.NetworkArchive.Add(network);
            }
        }

        /// <summary>
        /// Returns a collection of all networks, from all clusters within the supercluster.
        /// </summary>
        /// <param name="includeArchived">Flags wether archived (unclustered) networks should be included in the collection.</param>
        /// <returns></returns>
        public List<Network> GetAllNetworks(bool includeArchived = true)
        {
            var allNetworks = new List<Network>();

            if(includeArchived)
                allNetworks.AddRange(this.NetworkArchive);

            this.Clusters.ForEach(o => allNetworks.AddRange(o.Networks));

            return allNetworks;
        }

        /// <summary>
        /// Triggers an evolution cycle for all clustered networks. Does not effect archived networks.
        /// </summary>
        public void Evolve()
        {
            var maxTravellers = Math.Round(this.ClusterConfig.MaxNetworks * this.ClusterConfig.TravellerRatio, 0);
            
            foreach(var cluster in this.Clusters)
            {
                var travellerCandidates = new List<Network>();

                while(travellerCandidates.Count < maxTravellers && this.NetworkArchive.Count > 0)
                {
                    var biasedIndex = (int)Math.Round(NEMath.RandomBetween(0, this.NetworkArchive.Count - 1, 2.5), 0);

                    travellerCandidates.Add(this.NetworkArchive[biasedIndex]);
                    this.NetworkArchive.RemoveAt(biasedIndex);
                }

                cluster.Evolve(ref travellerCandidates);

                this.NetworkArchive.AddRange(travellerCandidates);
            }
        }

        /// <summary>
        /// <para>Will recluster all networks around the strongest candidates. Any networks that do not make a cluster (due to capping) will be archived.</para>
        /// <para>If the network archive overflows, the weakest candidates in the archive will be purged.</para>
        /// 
        /// <para>Clustering assumes all networks, including archived, have been evaluated using the same data set.</para>
        /// <para>If you have been using different data sets to train different clusters, reevaluate all using a desired superset before clustering.</para>
        /// </summary>
        public void Cluster(bool clusteringInputsSet = false)
        {
            var unclusteredNetworks = this.GetAllNetworks().OrderByDescending(o => o.Strength).ToList();
            
            this.Clusters.Clear();
            this.NetworkArchive.Clear();

            // If the trainer has not set inputs to use for clustering assements, we will set all the inputs
            // to 1.0 (except for the biasing input).
            if(!clusteringInputsSet)
            {
                foreach(var network in unclusteredNetworks)
                {
                    for (var i = 1; i < network.Neurons[0].Length; i++)
                        network.Neurons[0][i].Input = 1.0;
                }
            }

            // Create clusters around the strongest available candidates, until the cluster cap is reached.
            //
            // The process is to grap the strongest network (top of the list), then through all unclustered
            // networks to see if they are compatible. Once all the unclustered networks have been assessed,
            // the process repeats until the maximum number of clusters is reached.
            while(this.Clusters.Count < this.SuperclusterConfig.MaxClusters && unclusteredNetworks.Count > 0)
            {
                var reference = unclusteredNetworks[0];
                unclusteredNetworks.RemoveAt(0);

                var cluster = new Cluster(Guid.NewGuid().ToString(), this.ClusterConfig, this.NetworkConfig);
                cluster.Networks.Add(reference);

                var referenceVector = reference.Query();

                for(var i = unclusteredNetworks.Count - 1; i >= 0; i--)
                {
                    var candidate = unclusteredNetworks[i];
                    var candidateVector = candidate.Query();

                    var angle = NEMath.AngleBetweenVectors(referenceVector, candidateVector);

                    if(angle <= this.SuperclusterConfig.ClusteringAngle)
                    {
                        cluster.Networks.Add(candidate);

                        unclusteredNetworks.RemoveAt(i);
                    }
                }
                
                this.Clusters.Add(cluster);
            }

            // If there are any unclustered networks left, add them to the archive.
            this.NetworkArchive.AddRange(unclusteredNetworks);

            // Should already be sorted by strength, but let's make sure:
            this.NetworkArchive = this.NetworkArchive.OrderByDescending(o => o.Strength).ToList();

            // If there are more networks in the archive than allowed, cull the weakest.
            while(this.NetworkArchive.Count > this.SuperclusterConfig.MaxArchivedNetworks)
                this.NetworkArchive.RemoveAt(this.NetworkArchive.Count - 1);

            // If for whatever reason there were not enough clusters created to fill the quota,
            // create clusters with random networks to fill out the population.
            while(this.Clusters.Count < this.SuperclusterConfig.MaxClusters)
            {
                var cluster = new Cluster(Guid.NewGuid().ToString(), this.ClusterConfig, this.NetworkConfig);

                while(cluster.Networks.Count < this.ClusterConfig.MaxNetworks)
                {
                    var network = new Network(this.NetworkConfig.Clone(), NetworkOrigins.BOOTSTRAP, 0);
                    network.RandomiseAxions();

                    cluster.Networks.Add(network);
                }
            }
        }

        /// <summary>
        /// <para>Brings together all networks, and forces the creation of a new diversified pool of networks.</para>
        /// <para>Existing clusters will be cleared, and all existing networks (except the strongest) purged.</para>
        /// 
        /// <para>The new pool of networks will initially be archived, and will need to be assessed and then clustered.</para>
        /// 
        /// <seealso cref="Cluster"/>
        /// </summary>
        public void Merge()
        {
            var existingNetworks = this.GetAllNetworks().OrderByDescending(o => o.Strength).ToList();
            
            this.Clusters.Clear();
            this.NetworkArchive.Clear();

            if(existingNetworks.Count < 1)
                return;

            var maxNetworks = this.ClusterConfig.MaxNetworks * this.SuperclusterConfig.MaxClusters;
            var maxClones = Math.Round(maxNetworks * this.ClusterConfig.CloneRatio, 0);

            // Prime population with clones of the strongest mutation.
            // One perfect clone, two imperfect clones, and two heavily mutated clones.
            this.NetworkArchive.Add(existingNetworks[0].Clone(true));
            this.NetworkArchive.Add(existingNetworks[0].Clone());
            this.NetworkArchive.Add(existingNetworks[0].Clone());
            this.NetworkArchive.Add(existingNetworks[0].Clone());
            this.NetworkArchive.Add(existingNetworks[0].Clone());

            this.NetworkArchive[3].HeavilyMutuate(0.25);
            this.NetworkArchive[4].HeavilyMutuate(0.50);

            // Fill the clone quota, strongly biased towards stronger networks.
            while(this.NetworkArchive.Count < maxClones)
            {
                var biasedIndex = (int)Math.Round(NEMath.RandomBetween(0, existingNetworks.Count - 1, 5.0), 0);

                this.NetworkArchive.Add(existingNetworks[biasedIndex].Clone());
            }

            // Fill the remaining quota via crossover, moderately biased towards strong networks.
            while(this.NetworkArchive.Count < maxNetworks)
            {
                var biasedIndexA = (int)Math.Round(NEMath.RandomBetween(0, existingNetworks.Count - 1, 2.5), 0);
                var biasedIndexB = (int)Math.Round(NEMath.RandomBetween(0, existingNetworks.Count - 1, 2.5), 0);

                if(biasedIndexA == biasedIndexB)
                    continue;

                var parentA = existingNetworks[biasedIndexA];
                var parentB = existingNetworks[biasedIndexB];

                this.NetworkArchive.Add(parentA.Crossover(parentB));
            }
        }

    }
}
