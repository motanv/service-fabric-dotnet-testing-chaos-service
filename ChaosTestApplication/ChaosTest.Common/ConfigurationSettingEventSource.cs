// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.Common
{
    using System.Diagnostics.Tracing;

    [EventSource(Name = "MyCompany-ServiceApplication-ChaosTestAppChaosCommonConfigurationSetting")]
    internal sealed class ConfigurationSettingEventSource : CommonServiceEventSource
    {
        public static ConfigurationSettingEventSource Current = new ConfigurationSettingEventSource();
    }
}