// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.WebService
{
    using System.Diagnostics.Tracing;
    using ChaosTest.Common;

    [EventSource(Name = "MyCompany-ServiceApplication-ChaosTestAppChaosWebService")]
    internal sealed class ServiceEventSource
    {
        public static CommonServiceEventSource Current = CommonServiceEventSource.Instance;
    }
}