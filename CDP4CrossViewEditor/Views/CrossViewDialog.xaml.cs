// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewDialog.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4CrossViewEditor.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4CrossViewEditor.ViewModels;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for <see cref="CrossViewDialog"/> XAML
    /// </summary>
    [DialogViewExport("CrossViewDialog", "The Dialog to select a CrossView Editor parameters")]
    public partial class CrossViewDialog : DXWindow, IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CrossViewDialog"/> class.
        /// </summary>
        public CrossViewDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossViewDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public CrossViewDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }

        /// <summary>
        /// Initialize data context for both user controls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DXWindow_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!(this.DataContext is CrossViewDialogViewModel viewModel))
            {
                return;
            }

            this.ElementThingSelector.DataContext = viewModel.ElementSelectorViewModel;
            this.ParameterThingSelector.DataContext = viewModel.ParameterSelectorViewModel;
        }
    }
}
