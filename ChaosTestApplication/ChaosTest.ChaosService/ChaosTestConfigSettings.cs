// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.ChaosService
{
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using ChaosTest.Common;

    /// <summary>
    /// This class populates values of various parameters from the Settings.xml under Config
    /// </summary>
    internal class ChaosTestConfigSettings
    {
        private const string ConfigPackageName = "Config";
        private const string ChaosTestConfigurationSectionName = "ChaosTestService";

        private static readonly ConfigurationSettings Configuration =
            FabricRuntime.GetActivationContext().GetConfigurationPackageObject(ConfigPackageName).Settings;

        public static TimeSpan MaxClusterStabilizationTimeout = new ConfigurationSetting<TimeSpan>(
            Configuration,
            ChaosTestConfigurationSectionName,
            "MaxClusterStabilizationTimeout",
            TimeSpan.FromSeconds(300),
            ServiceEventSource.Current).Value;

        public static uint MaxConcurrentFaults = new ConfigurationSetting<uint>(
            Configuration,
            ChaosTestConfigurationSectionName,
            "MaxConcurrentFaults",
            2,
            ServiceEventSource.Current).Value;

        public static bool EnableMoveReplicaFaults = new ConfigurationSetting<bool>(
            Configuration,
            ChaosTestConfigurationSectionName,
            "EnableMoveReplicaFaults",
            true,
            ServiceEventSource.Current).Value;

        public static TimeSpan OperationTimeout = new ConfigurationSetting<TimeSpan>(
            Configuration,
            ChaosTestConfigurationSectionName,
            "OperationTimeout",
            TimeSpan.FromSeconds(300),
            ServiceEventSource.Current).Value;

        public static TimeSpan WaitTimeBetweenFaults = new ConfigurationSetting<TimeSpan>(
            Configuration,
            ChaosTestConfigurationSectionName,
            "WaitTimeBetweenFaults",
            TimeSpan.FromSeconds(300),
            ServiceEventSource.Current).Value;

        public static TimeSpan WaitTimeBetweenIterations = new ConfigurationSetting<TimeSpan>(
            Configuration,
            ChaosTestConfigurationSectionName,
            "WaitTimeBetweenIterations",
            TimeSpan.FromSeconds(300),
            ServiceEventSource.Current).Value;
    }
}