# CBANE
## Cluster Based Associative Neuro-Evolution
CBANE is an approach to neuro-evolution that attempts to preserve innovation and diversity by clustering networks based on the angle between their output vectors (evaluated against some standard input set).

Rather than culling solely based on the pool wide performance of networks during the last evaluation cycle, CBANE only culls within a cluster, and if a network does not fit in to any active cluster (based on configured caps), it is temporarily archived rather than culled.

CBANE allows a select number of archived networks to "travel" to active clusters each cycle to continue their evolution. Travelling is heavily biased towards weak (low scoring) archived networks that are less likely to become cluster references in future cycles. Travelling is therefore primarily a mechanism to help keep the gene pool in each cluster diverse.

CBANE also supports & encourages periodic merging & reclustering to promote network diversity and combat global stagnation.

CBANE also encourages distributed computing and diversified training sets via the concept of superclusters.

This project represents a .NET Core implementation of CBANE, providing a library implementing the core concepts of CBANE, and a sandpit that includes a working example of the library in use.

This implementation of CBANE focuses on modifying axion weights within a network with fixed hyperparameters (fixed structure), but the CBANE model of evolution can also be applied to evolving hyperparameters. The only requirement for the CBANE model is that all networks within a supercluster have the same number and type of outputs.
