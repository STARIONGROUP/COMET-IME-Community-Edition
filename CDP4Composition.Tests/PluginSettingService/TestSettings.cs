// -------------------------------------------------------------------------------------------------
// <copyright file="TestSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.PluginSettingService
{
    using System;
    using System.Collections.Generic;

    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services.PluginSettingService;

    using Newtonsoft.Json;

    /// <summary>
    /// A <see cref="PluginSettings"/> used for testing the <see cref="IPluginSettingsService"/>
    /// </summary>
    internal class TestSettings : PluginSettings
    {
        public Guid Identifier { get; set; }

        public string Description { get; set; }
    }

    internal class TestConfiguration : IPluginSavedConfiguration
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Name { get; set; }

        public string Description { get; set; }

        public string OwnTestConfigurationProperty { get; set; }
    }
}