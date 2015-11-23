﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using MultiTenantSurveyApp.Common.Configuration;

namespace MultiTenantSurveyApp.WebApi.Configuration
{
    /// <summary>
    /// This class holds various configuration values used by the MultiTenantSurveyApp.WebApi project.
    /// </summary>
    public class ConfigurationOptions
    {
        public ConfigurationOptions()
        {
            Data = new DatabaseOptions();
            Redis = new RedisOptions();
            KeyVault = new KeyVaultOptions();
            AzureAd = new AzureAdOptions();
        }
        public DatabaseOptions Data { get; set; }
        public AzureAdOptions AzureAd { get; set; }
        public RedisOptions Redis { get; set; }
        public KeyVaultOptions KeyVault { get; set; }
    }
}
