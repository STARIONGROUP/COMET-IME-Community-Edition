﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreDialog.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4EngineeringModel.Views.Dialogs
{
    using CDP4Common.CommonData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for DomainFileStoreDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.DomainFileStore)]
    public partial class DomainFileStoreDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreDialog"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used by MEF to instation the view. The view is instantiated to enable navigation using the <see cref="IThingDialogNavigationService"/>
        /// </remarks>
        public DomainFileStoreDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public DomainFileStoreDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
