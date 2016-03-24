// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.WebService
{
    using System;
    using System.Web.Http;
    using ChaosTest.Common;
    using ChaosTest.WebService.Controllers;
    using Microsoft.Practices.Unity;
    using Microsoft.ServiceFabric.Services.Client;
    using Unity.WebApi;

    public static class UnityConfig
    {
        public static void RegisterComponents(HttpConfiguration config)
        {
            UnityContainer container = new UnityContainer();
            
            HttpCommunicationClientFactory clientFactory = new HttpCommunicationClientFactory();

            container.RegisterType<DefaultController>(new InjectionConstructor(clientFactory));

            config.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}