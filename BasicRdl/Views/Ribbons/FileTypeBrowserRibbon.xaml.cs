// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypeBrowserRibbon.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace BasicRdl.Views
{
    using BasicRdl.ViewModels;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CommonServiceLocator;

    using DevExpress.Xpf.Bars;

    /// <summary>
    /// Interaction logic for FileTypeBrowserRibbon.xaml
    /// </summary>
    public partial class FileTypeBrowserRibbon : IView, IBarItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTypeBrowserRibbon"/> class.
        /// </summary>
        public FileTypeBrowserRibbon()
        {
            this.InitializeComponent();
            var messageBus = ServiceLocator.Current.GetInstance<ICDPMessageBus>();
            this.DataContext = new FileTypeBrowserRibbonViewModel(messageBus);
        }
    }
}
