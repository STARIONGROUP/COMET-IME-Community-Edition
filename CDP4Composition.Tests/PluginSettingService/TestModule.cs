// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestModule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.PluginSettingService
{
    using CDP4Composition.Attributes;
    using CDP4Composition.PluginSettingService;
    using Microsoft.Practices.Prism.Modularity;
    
    /// <summary>
    /// an <see cref="IModule"/> implementation for the purpose of testing the <see cref="PluginSettingsService"/>
    /// </summary>
    [ModuleExportName(typeof(TestModule), "Test Module")]
    internal class TestModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TestModule"/>
        /// </summary>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read/write the <see cref="PluginSettings"/>
        /// </param>
        internal TestModule(IPluginSettingsService pluginSettingsService)
        {
            this.PluginSettingsService = pluginSettingsService;
        }

        /// <summary>
        /// Gets or sets the <see cref="IPluginSettingsService"/> used to read/write the <see cref="PluginSettings"/>
        /// </summary>
        internal IPluginSettingsService PluginSettingsService { get; set; }

        public void Initialize()
        {
        }
    }
}