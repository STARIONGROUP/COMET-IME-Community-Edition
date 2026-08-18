// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVBrowserRibbon.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4Requirements.Views
{
    using System.ComponentModel.Composition;

    using CDP4Requirements.ViewModels;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Ribbon;

    using CDP4Dal;

    using CommonServiceLocator;

    /// <summary>
    /// Interaction logic for the V&amp;V Register ribbon group that opens the VCD browser.
    /// </summary>
    [Export(typeof(ExtendedRibbonPageGroup))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class VandVBrowserRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVBrowserRibbon"/> class.
        /// </summary>
        [ImportingConstructor]
        public VandVBrowserRibbon()
        {
            this.InitializeComponent();
            var messageBus = ServiceLocator.Current.GetInstance<ICDPMessageBus>();
            this.DataContext = new VandVBrowserRibbonViewModel(messageBus);
        }
    }
}
