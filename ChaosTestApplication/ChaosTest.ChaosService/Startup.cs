// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System.Web.Http;
    using ChaosTest.Common;
    using Owin;

    /// <summary>
    /// OWIN configuration
    /// </summary>
    public class Startup : IOwinAppBuilder
    {
        private readonly ChaosService service;

        public Startup(ChaosService service)
        {
            this.service = service;
        }

        /// <summary>
        /// Hooks up the Web API pipeline with the OWIN pipeline,
        /// using the method UseWebApi
        /// </summary>
        /// <param name="appBuilder"></param>
        /// 
        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            FormatterConfig.ConfigureFormatters(config.Formatters);
            UnityConfig.RegisterComponents(config, this.service);

            config.MapHttpAttributeRoutes();

            appBuilder.UseWebApi(config);
        }
    }
}