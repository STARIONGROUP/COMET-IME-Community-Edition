// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserModule.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Attributes;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.Prism.Regions;
    using Views;

    /// <summary>
    /// The <see cref="IModule"/> implementation for the <see cref="CDP4ObjectBrowser"/> Component
    /// </summary>
    [ModuleExportName(typeof(ObjectBrowserModule), "Object Browser Module")]
    public class ObjectBrowserModule : IModule
    {
        /// <summary>
        /// the <see cref="IRegionManager"/> that is used by the <see cref="ObjectBrowserModule"/> to register the regions
        /// </summary>
        private readonly IRegionManager regionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectBrowserModule"/> class.
        /// </summary>
        /// <param name="regionManager">
        /// The (MEF injected) region manager.
        /// </param>
        [ImportingConstructor]
        public ObjectBrowserModule(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        /// <summary>
        /// Initialize the Module
        /// </summary>
        public void Initialize()
        {
            this.regionManager.RegisterViewWithRegion(CDP4Composition.RegionNames.RibbonRegion, typeof(ObjectBrowserRibbonPage));
        }
    }
}
