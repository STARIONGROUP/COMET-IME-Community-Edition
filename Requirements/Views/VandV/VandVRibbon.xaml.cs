// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVRibbon.xaml.cs" company="Starion Group S.A.">
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
    using CDP4Requirements.ViewModels;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Ribbon;

    using CDP4Dal;

    using CommonServiceLocator;

    /// <summary>
    /// Interaction logic for the V&amp;V ribbon group.
    /// </summary>
    /// <remarks>
    /// This group is declared as a child of <see cref="RequirementsRibbon"/>, so it must not be exported as an
    /// <see cref="ExtendedRibbonPageGroup"/>. Doing so makes the ribbon content builder create a second instance
    /// and the duplicate bar item names then fail to register in the ribbon name scope.
    /// </remarks>
    public partial class VandVRibbon : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVRibbon"/> class.
        /// </summary>
        public VandVRibbon()
        {
            this.InitializeComponent();
            var messageBus = ServiceLocator.Current.GetInstance<ICDPMessageBus>();
            this.DataContext = new VandVRibbonViewModel(messageBus);
        }
    }
}
