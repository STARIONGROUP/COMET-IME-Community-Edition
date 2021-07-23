// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestModule.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
