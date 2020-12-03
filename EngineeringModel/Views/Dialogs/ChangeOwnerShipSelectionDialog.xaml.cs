// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeOwnerShipSelectionDialog.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed
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

namespace CDP4EngineeringModel.Views.Dialogs
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for ChangeOwnerShipSelectionDialog.xaml
    /// </summary>
    [DialogViewExport("ChangeOwnerShipSelectionDialog", "Ownership Selection")]
    public partial class ChangeOwnerShipSelectionDialog : DXWindow, IDialogView
    {
        // <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionFilterSelectionDialog"/> class
        /// </summary>
        public ChangeOwnerShipSelectionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionFilterSelectionDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ChangeOwnerShipSelectionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
