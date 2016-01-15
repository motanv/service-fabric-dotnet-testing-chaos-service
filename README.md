---
services: service-fabric
platforms: dotnet
author: vturecek
---

# Chaos Test Service
Learn how to write a service that uses Microsoft Azure Service Fabric's built-in Chaos Test to exercise your code's fault tolerance by putting a little chaos in your cluster.

Service Fabric includes a suite of tools specifically designed to test running services. You can easily induce meaningful faults and run test scenarios to exercise and validate the numerous different states and transitions a service will experience throughout its lifetime, all in a controlled and safe manner. The chaos test induces random faults - everything from moving replicas to restarting entire nodes. 

These tools are available through a C# API and PowerShell commands, which means you invoke them from somewhere and watch them as they run. This example application shows how to use the C# API to write a service that continuously runs the chaos test - as a service - and reports results in a UI.

## Running this sample
Open the solution in Visual Studio 2015 and press F5 to run with debugging. With the application running, open a web browser and go to http://localhost:8081/chaostest to interact with the service UI. 

The chaos service can be started and stopped through the UI and displays progress. The start/stop state and progress are maintained in Reliable Collections in a stateful service, which means the test itself is fault-tolerant and will continue to run even if the test itself is moved or experiences a fault.

## More information

[Set up your development environment](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-get-started/)

[Publish your application to Azure using Visual Studio](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-publish-app-remote-cluster/)

[Service Fabric Testability Tools](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-testability-overview/)

[Service Fabric Chaos Test](https://azure.microsoft.com/en-us/documentation/articles/service-fabric-testability-scenarios/)

