// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.WebService.Controllers
{
    using System.Net.Http;
    using System.Web.Http;
    using ChaosTest.WebService.Extensions;

    /// <summary>
    /// Controller that serves up JavaScript files from the Scripts directory that are included as embedded assembly resources.
    /// You can also use the FileSystem and StaticFile middleware for OWIN to render script files,
    /// or wait for ASP.NET vNext when the full MVC stack will be available for self-hosting.
    /// </summary>
    public class ScriptsController : ApiController
    {
        /// <summary>
        /// Renders javascript files in the Scripts directory.
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        [HttpGet]
        public HttpResponseMessage Get(string name)
        {
            return this.View("ChaosTest.WebService.Scripts." + name, "application/javascript");
        }
    }
}