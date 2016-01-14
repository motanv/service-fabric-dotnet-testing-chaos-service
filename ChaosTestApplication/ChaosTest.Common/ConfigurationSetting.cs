// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChaosTest.Common
{
    using System;
    using System.Fabric.Description;
    using System.Reflection;

    /// <summary>
    ///   Used for accessing configuration settings in a typed fashion.
    ///   Typed configuration setting whose value can be overridden in code.
    /// </summary>
    /// <typeparam name="T"> Value type </typeparam>
    /// 
    public class ConfigurationSetting<T>
    {
        /// <summary>
        /// Service Fabric config package and section name.
        /// </summary>
        private readonly string configurationSectionName;

        private readonly ConfigurationSettings configurationSettings;
        private readonly T defaultValue;
        private readonly string settingName;
        private T value;
        private bool valueSpecified;
        private CommonServiceEventSource serviceEventSource;

        public ConfigurationSetting(
            ConfigurationSettings configurationSettings,
            string configurationSectionName,
            string settingName,
            T defaultValue,
            CommonServiceEventSource serviceEventSource)
            : this(configurationSectionName, settingName, defaultValue, true)
        {
            this.configurationSettings = configurationSettings;
            this.serviceEventSource = serviceEventSource;
        }

        /// <summary>
        ///   Initializes a new instance of the ConfigurationSetting class.
        /// </summary>
        /// <param name="configurationSectionName"></param>
        /// <param name="settingName"> Name of the setting </param>
        /// <param name="defaultValue"> Default value of the setting if it does not exist in the ACS </param>
        /// <param name="enableTracing"> Whether to log the value of the setting when it is read from the ACS </param>
        private ConfigurationSetting(string configurationSectionName, string settingName, T defaultValue, bool enableTracing)
        {
            this.settingName = settingName;
            this.defaultValue = defaultValue;
            this.valueSpecified = false;
            this.DisableTracing = !enableTracing;
            this.configurationSectionName = configurationSectionName;
        }

        /// <summary>
        ///   Gets or sets a value indicating whether tracing should be used when accessing this setting.
        /// </summary>
        /// <remarks>
        ///   This property allows the tracing subsystem itself to use configuration settings, where infinite recursion might otherwise occur.
        /// </remarks>
        public bool DisableTracing { get; set; }

        public T Value
        {
            get
            {
                if (!this.valueSpecified)
                {
                    this.value = this.defaultValue;

                    string appConfigValue = this.GetConfigurationSetting(this.settingName);
                    if (appConfigValue != null)
                    {
                        if (!this.TryParse(appConfigValue, out this.value))
                        {
                            this.value = this.defaultValue;
                        }
                    }

                    if (!this.DisableTracing)
                    {
                        this.serviceEventSource.Message("Configuration value read from service settings: {0}={1}", this.settingName, this.value);
                    }

                    this.valueSpecified = true;
                }

                // This is ALWAYS the ACS value of the setting (never overwritten)
                return this.value;
            }

            // This should never be used in production code or everything will break (different instances
            // will have different views of the config value)
            set
            {
                this.valueSpecified = true;
                this.value = value;
            }
        }

        /// <summary>
        ///   Try to parse the string and return an object of the given type.
        /// </summary>
        /// <param name="value"> String to be parsed </param>
        /// <param name="type"> Type of result </param>
        /// <returns> Result of parsing the string </returns>
        public object Parse(string value, Type type)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            try
            {
                if (type == typeof(string))
                {
                    return value;
                }

                if (type == typeof(bool))
                {
                    return bool.Parse(value);
                }

                if (type == typeof(int))
                {
                    return int.Parse(value);
                }

                if (type == typeof(double))
                {
                    return double.Parse(value);
                }

                if (type == typeof(Uri))
                {
                    return new Uri(value);
                }

                if (type.IsEnum)
                {
                    return Enum.Parse(type, value, true);
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return value.Length == 0 ? null : this.Parse(value, type.GetGenericArguments()[0]);
                }

                if (type.IsArray && type.GetElementType() == typeof(byte))
                {
                    return Convert.FromBase64String(value);
                }

                MethodInfo parseMethod = type.GetMethod("Parse", new[] {typeof(string)});

                return parseMethod?.Invoke(null, new object[] {value});
            }
            catch (ArgumentException)
            {
                //CommonServiceEventSource.Current.Message("{0}: Invalid argument.", EntityName);
                this.serviceEventSource.Message("Invalid argument.");
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (TargetException)
            {
                //CommonServiceEventSource.Current.Message("{0}: Target of the Parse method is invalid.", EntityName);
                this.serviceEventSource.Message("Target of the Parse method is invalid.");
                return null;
            }
        }

        /// <summary>
        /// Parse the string and return a typed value.
        /// </summary>
        /// <typeparam name="U"> Value type </typeparam>
        /// <param name="valueString"> Value in string format </param>
        /// <param name="value"> Result of parsing </param>
        /// <returns> True if succeeds </returns>
        public bool TryParse<U>(string valueString, out U value)
        {
            object result = this.Parse(valueString, typeof(U));
            if (result == null)
            {
                value = default(U);
                return false;
            }

            value = (U) result;
            return true;
        }

        /// <summary>
        /// Get Service Fabric Settings from config.
        /// </summary>
        /// <param name="parameterName">Return settings for the parameter name</param>
        /// <returns></returns>
        private string GetConfigurationSetting(string parameterName)
        {
            try
            {
                if (this.configurationSettings?.Sections[this.configurationSectionName] != null)
                {
                    if (this.configurationSettings.Sections[this.configurationSectionName].Parameters.Contains(parameterName))
                    {
                        return this.configurationSettings.Sections[this.configurationSectionName].Parameters[parameterName]
                            .Value;
                    }
                }
            }
            catch
            {
                this.serviceEventSource.Message(
                    "Parameter {0} was not found in section {1}",
                    parameterName,
                    this.configurationSectionName);
            }

            return null;
        }
    }
}