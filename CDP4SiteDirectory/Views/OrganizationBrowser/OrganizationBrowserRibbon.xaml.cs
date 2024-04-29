// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrganizationBrowserRibbon.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Ribbon;

    using CDP4Dal;

    using CDP4SiteDirectory.ViewModels;

    /// <summary>
    /// Interaction logic for LogInfoControls.xaml
    /// </summary>
    [Export(typeof(ExtendedRibbonPageGroup))]
    public partial class OrganizationBrowserRibbon : IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationBrowserRibbon" /> class
        /// </summary>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        [ImportingConstructor]
        public OrganizationBrowserRibbon(ICDPMessageBus messageBus)
        {
            this.InitializeComponent();
            this.DataContext = new OrganizationBrowserRibbonViewModel(messageBus);
        }
    }
}
