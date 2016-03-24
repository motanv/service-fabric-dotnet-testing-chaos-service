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
    [RoutePrefix("api")]
    public class DefaultController : ApiController
    {
        private const string ChaosTestServiceName = "ChaosTestService";
        private readonly ICommunicationClientFactory<HttpCommunicationClient> clientFactory;


        public DefaultController(ICommunicationClientFactory<HttpCommunicationClient> clientFactory)
        {
            this.clientFactory = clientFactory;
        }
        
        [HttpPost]
        [Route("Start")]
        public Task Start()
        {
            Uri serviceUri = new ServiceUriBuilder(ChaosTestServiceName).ToUri();

            ServicePartitionClient<HttpCommunicationClient> servicePartitionClient = new ServicePartitionClient<HttpCommunicationClient>(
                this.clientFactory,
                serviceUri);

            return servicePartitionClient.InvokeWithRetryAsync(
                client =>
                {
                    return client.HttpClient.PostAsync(new Uri(client.Url, "api/Start"), new StringContent(String.Empty));
                });
        }

        [HttpPost]
        [Route("Stop")]
        public Task Stop()
        {
            Uri serviceUri = new ServiceUriBuilder(ChaosTestServiceName).ToUri();

            ServicePartitionClient<HttpCommunicationClient> servicePartitionClient = new ServicePartitionClient<HttpCommunicationClient>(
                this.clientFactory,
                serviceUri);

            return servicePartitionClient.InvokeWithRetryAsync(
                client =>
                {
                    return client.HttpClient.PostAsync(new Uri(client.Url, "api/Stop"), new StringContent(String.Empty));
                });
        }

        [HttpGet]
        [Route("Results")]
        public Task<string> Results()
        {
            Uri serviceUri = new ServiceUriBuilder(ChaosTestServiceName).ToUri();

            ServicePartitionClient<HttpCommunicationClient> servicePartitionClient = new ServicePartitionClient<HttpCommunicationClient>(
                this.clientFactory,
                serviceUri);

            return servicePartitionClient.InvokeWithRetryAsync(
                client =>
                {
                    return client.HttpClient.GetStringAsync(new Uri(client.Url, "api/Results"));
                });
        }
    }
}