// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System.Web.Http;
    using ChaosTest.ChaosService.Controllers;
    using Microsoft.Practices.Unity;
    using Unity.WebApi;

    public static class UnityConfig
    {
        public static void RegisterComponents(HttpConfiguration config, ChaosService service)
        {
            UnityContainer container = new UnityContainer();
            container.RegisterType<DefaultController>(new InjectionConstructor(service));
            config.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}