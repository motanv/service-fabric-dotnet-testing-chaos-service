// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.ChaosService.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    /// Default controller.
    /// </summary>
    public class DefaultController : ApiController
    {
        private readonly ChaosService service;

        public DefaultController(ChaosService service)
        {
            this.service = service;
        }

        [HttpPost]
        public Task Start()
        {
            return this.service.StartAsync();
        }

        [HttpPost]
        public Task Stop()
        {
            return this.service.StopAsync();
        }

        [HttpGet]
        public Task<Result> Results()
        {
            return this.service.GetEventsAsync();
        }
    }
}