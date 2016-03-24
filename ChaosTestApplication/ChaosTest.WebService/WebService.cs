// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.WebService
{
    using System.Collections.Generic;
    using System.Fabric;
    using ChaosTest.Common;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// This is a stateless web-service that acts as a mediator between the webage and the fault-inducing service.
    /// </summary>
    public class WebService : StatelessService
    {
        public WebService(StatelessServiceContext context)
            : base (context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(
                    parameters =>
                        new OwinCommunicationListener("chaostest", new Startup(), parameters, ServiceEventSource.Current))
            };
        }
    }
}