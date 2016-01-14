This ChaosSample application demonstrates the probabilistic fault inducing to a Service Fabric cluster; 
e.g., node restart. It includes two services: a stateless web-service that also serves a webpage showing 
the recent faults and a stateful service that keeps inducing faults and failovers to the cluster 
using the ChaosTestScenario from Microsoft.ServiceFabric.Testability under the hood.

For a local dev-cluster, the default address for the webpage is localhost:8503/ChaosTest/

Upon publishing to an Azure Service Fabric cluster, the address for the webpage would be: 
<Cluster Public IP Address>:8503/ChaosTest/