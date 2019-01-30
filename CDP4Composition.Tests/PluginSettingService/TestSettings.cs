// -------------------------------------------------------------------------------------------------
// <copyright file="TestSettings.cs" company="RHEA System S.A.">
//   Copyright (c) 2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.PluginSettingService
{
    using System;
    using CDP4Composition.PluginSettingService;

    /// <summary>
    /// A <see cref="PluginSettings"/> used for testing the <see cref="IPluginSettingsService"/>
    /// </summary>
    internal class TestSettings : PluginSettings
    {
        public Guid Identifier { get; set; }

        public string Description { get; set; }
    }
}