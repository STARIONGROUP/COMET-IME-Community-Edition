// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4ScriptingModule.cs" company="RHEA System S.A.">
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

namespace CDP4Scripting
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;

    using Microsoft.Practices.Prism.Modularity;

    /// <summary>
    ///  The <see cref="IModule"/> implementation for the <see cref="CDP4ScriptingModule"/> Component
    /// </summary>
    [ModuleExportName(typeof(CDP4ScriptingModule), "Scripting Module")]
    public class CDP4ScriptingModule : IModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CDP4ScriptingModule"/> class.
        /// </summary>
        /// <param name="panelNavigationService">
        /// The (MEF injected) instance of <see cref="IPanelNavigationService"/>
        /// </param>
        [ImportingConstructor]
        public CDP4ScriptingModule(IPanelNavigationService panelNavigationService)
        {
            this.PanelNavigationService = panelNavigationService;
        }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> that is used by the <see cref="CDP4ScriptingModule"/> to support panel navigation
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }
        
        /// <summary>
        /// Initialize the module with the static views
        /// </summary>
        public void Initialize()
        {
        }
    }
}
