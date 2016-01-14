// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.Common
{
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Owin.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;

    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly IOwinAppBuilder startup;
        private readonly ServiceInitializationParameters serviceParameters;
        private readonly string appRoot;
        private readonly CommonServiceEventSource eventSource;

        private IDisposable serverHandle;
        private string listeningAddress;

        public OwinCommunicationListener(
            string appRoot, IOwinAppBuilder startup, ServiceInitializationParameters serviceParameters, CommonServiceEventSource eventSource)
        {
            this.startup = startup;
            this.appRoot = appRoot;
            this.serviceParameters = serviceParameters;
            this.eventSource = eventSource;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            // The name of the endpoint configured in the service manifest under the Endpoints
            // section; this is the endpoint that the OWIN server will be listening on.
            EndpointResourceDescription serviceEndpoint
                = this.serviceParameters.CodePackageActivationContext.GetEndpoint(StringResource.ChaosServiceEndpoint);

            int port = serviceEndpoint.Port;

            StatefulServiceInitializationParameters statefulInitParams = this.serviceParameters as StatefulServiceInitializationParameters;

            if (statefulInitParams != null)
            {
                // Address for a stateful service needs to be unique to the replica,
                // because each node can host multiple replicas if the service has multiple partitions.
                this.listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}/{2}/{3}",
                    serviceEndpoint.Port,
                    statefulInitParams.PartitionId,
                    statefulInitParams.ReplicaId,
                    Guid.NewGuid());
            }
            else
            {
                // For a stateless service the listening address is just the app path.
                this.listeningAddress = String.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}",
                    port,
                    String.IsNullOrWhiteSpace(this.appRoot)
                        ? String.Empty
                        : this.appRoot.TrimEnd('/') + '/');
            }

            this.serverHandle = WebApp.Start(this.listeningAddress, appBuilder => this.startup.Configuration(appBuilder));

            string resultAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            this.eventSource.Message("Listening on {0}", resultAddress);

            return Task.FromResult(resultAddress);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.StopWebServer();

            return Task.FromResult(true);
        }

        public void Abort()
        {
            this.StopWebServer();
        }

        private void StopWebServer()
        {
            if (this.serverHandle != null)
            {
                try
                {
                    this.serverHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}