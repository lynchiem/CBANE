# CBANE
## Cluster Based Associative Neuro-Evolution
CBANE is an approach to neuro-evolution that attempts to preserve innovation and diversity by clustering networks based on the angle between their output vectors (evaluated against some standard input set).

Rather than culling soley based on the pool wide performance of networks during the last evaluation cycle, CBANE only culls within a cluster, and if a network does not fit in to any active cluster (based on configured caps), it is temporarily archived rather than culled.

CBANE allows a select number of archived networks to "travel" to active clusters each cycle to continue their evolution.

CBANE also supports & encourages periodic merging & reclustering to promote network diversity and combat global stagnation.

CBANE also encourages distributed computing and diversified training sets via the concept of superclusters.
