// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.WebService
{
    using System.Fabric;
    using System.Fabric.Description;
    using ChaosTest.Common;

    internal class ChaosTestWebServiceConfigSettings
    {
        // WinFab-based config
        private const string ConfigPackageName = "Config";
        private const string ChaosTestConfigurationSectionName = "ChaosTestWebService";

        private static readonly ConfigurationSettings ConfigurationSettings =
            FabricRuntime.GetActivationContext().GetConfigurationPackageObject(ConfigPackageName).Settings;

        public static string ChaosServiceName = new ConfigurationSetting<string>(
            ConfigurationSettings,
            ChaosTestConfigurationSectionName,
            "ChaosServiceName",
            "ChaosTestService",
            ServiceEventSource.Current).Value;
    }
}