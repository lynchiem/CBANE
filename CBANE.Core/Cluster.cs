using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CBANE.Core
{
    public class Cluster
    {
        public string ClusterName { get; private set; }

        public List<Network> Networks = new List<Network>();

        private ClusterConfig clusterConfig;
        private NetworkConfig networkConfig;

        public Cluster(string clusterName, ClusterConfig clusterConfig, NetworkConfig networkConfig)
        {
            this.ClusterName = clusterName;

            this.clusterConfig = clusterConfig;
            this.networkConfig = networkConfig;
        }

        public void Evolve(ref List<Network> travellerCandidates)
        {
            // Sort networks by strength.
            this.Networks = this.Networks.OrderByDescending(o => o.Strength).ToList();

            // Cull weakest half of networks.
            if(this.Networks.Count > 1)
            {
                var half = (int)Math.Round((double)this.Networks.Count / 2, 0);
                this.Networks = this.Networks.Take(half).ToList();
            }
            
            // Fill opened space with new networks.
            var newNetworks = new List<Network>();

            if(this.Networks.Count < this.clusterConfig.MaxNetworks)
            {
                var clones = this.Networks.Where(o => o.Origin == NetworkOrigins.CLONE).Count();
                var travellers = this.Networks.Where(o => o.Origin == NetworkOrigins.TRAVELLER).Count();
                var others = this.Networks.Where(o => o.Origin != NetworkOrigins.CLONE && o.Origin != NetworkOrigins.TRAVELLER).Count();

                var maxClones = Math.Round(this.clusterConfig.MaxNetworks * this.clusterConfig.CloneRatio, 0);
                var maxTravellers = Math.Round(this.clusterConfig.MaxNetworks * this.clusterConfig.TravellerRatio, 0);
                var maxOthers = this.clusterConfig.MaxNetworks - maxClones - maxTravellers;

                var deltaClones = NEMath.Clamp(maxClones - clones, 0, maxClones);
                var deltaTravellers = NEMath.Clamp(maxTravellers - travellers, 0, maxClones);
                var deltaOthers = NEMath.Clamp(maxOthers - others, 0, maxOthers);

                while(deltaClones > 0)
                {
                    var biasedIndex = (int)Math.Round(NEMath.RandomBetween(0, this.Networks.Count - 1, 5.0), 0);

                    newNetworks.Add(this.Networks[biasedIndex].Clone());

                    deltaClones -= 1;
                }

                while(deltaTravellers > 0 && travellerCandidates.Count > 0)
                {
                    newNetworks.Add(travellerCandidates[0]);
                    travellerCandidates.RemoveAt(0);
                }
                
                while(deltaOthers > 0 && this.Networks.Count > 1)
                {
                    var biasedIndexA = (int)Math.Round(NEMath.RandomBetween(0, this.Networks.Count - 1, 2.5), 0);
                    var biasedIndexB = (int)Math.Round(NEMath.RandomBetween(0, this.Networks.Count - 1, 2.5), 0);

                    if(biasedIndexA == biasedIndexB)
                        continue;

                    var parentA = this.Networks[biasedIndexA];
                    var parentB = this.Networks[biasedIndexB];

                    newNetworks.Add(parentA.Crossover(parentB));
                    
                    deltaOthers -= 1;
                }
            }

            // Mutate existing networks (except strongest)
            for(var i = 1; i < this.Networks.Count; i++)
            {
                if(NEMath.Random() < this.clusterConfig.HeavyMutationRate)
                    this.Networks[i].HeavilyMutuate(0.25);
                else
                    this.Networks[i].Mutate();
            }

            // Add new networks to cluster.
            this.Networks.AddRange(newNetworks);
        }

    }
}
