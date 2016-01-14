// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.WebService.Controllers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using ChaosTest.Common;
    using ChaosTest.WebService.Extensions;
    using Microsoft.ServiceFabric.Services.Communication.Client;

    /// <summary>
    /// Default controller.
    /// </summary>
    public class DefaultController : ApiController
    {
        private const string ChaosTestServiceName = "ChaosTestService";
        private readonly ICommunicationClientFactory<HttpCommunicationClient> clientFactory;


        public DefaultController(ICommunicationClientFactory<HttpCommunicationClient> clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        [HttpGet]
        public HttpResponseMessage Index()
        {
            return this.View("ChaosTest.WebService.Views.Default.Index.html", "text/html");
        }

        [HttpPost]
        public Task Start()
        {
            Uri serviceUri = new ServiceUriBuilder(ChaosTestServiceName).ToUri();

            ServicePartitionClient<HttpCommunicationClient> servicePartitionClient = new ServicePartitionClient<HttpCommunicationClient>(
                this.clientFactory,
                serviceUri);

            return servicePartitionClient.InvokeWithRetryAsync(
                client =>
                {
                    HttpClient httpClient = new HttpClient();
                    return httpClient.PostAsync(client.BaseAddress + "/Start", new StringContent(String.Empty));
                });
        }

        [HttpPost]
        public Task Stop()
        {
            Uri serviceUri = new ServiceUriBuilder(ChaosTestServiceName).ToUri();

            ServicePartitionClient<HttpCommunicationClient> servicePartitionClient = new ServicePartitionClient<HttpCommunicationClient>(
                this.clientFactory,
                serviceUri);

            return servicePartitionClient.InvokeWithRetryAsync(
                client =>
                {
                    HttpClient httpClient = new HttpClient();
                    return httpClient.PostAsync(client.BaseAddress + "/Stop", new StringContent(String.Empty));
                });
        }

        [HttpGet]
        public Task<string> Results()
        {
            Uri serviceUri = new ServiceUriBuilder(ChaosTestServiceName).ToUri();

            ServicePartitionClient<HttpCommunicationClient> servicePartitionClient = new ServicePartitionClient<HttpCommunicationClient>(
                this.clientFactory,
                serviceUri);

            return servicePartitionClient.InvokeWithRetryAsync(
                client =>
                {
                    HttpClient httpClient = new HttpClient();
                    return httpClient.GetStringAsync(client.BaseAddress + "/Results");
                });
        }
    }
}