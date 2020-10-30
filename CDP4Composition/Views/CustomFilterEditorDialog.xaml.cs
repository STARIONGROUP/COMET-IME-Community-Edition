// -------------------------------------------------------------------------------------------------
// <copyright file="CustomFilterEditorDialog.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smieckowski
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
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using System.Windows;

    using DevExpress.Xpf.Core.FilteringUI;

    /// <summary>
    /// Interaction logic for ConfirmationDialog.xaml
    /// </summary>
    public partial class CustomFilterEditorDialog
    {
        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref=""/> setter method.
        /// </summary>
        private static readonly DependencyProperty FilteringContextProperty = DependencyProperty.Register("FilteringContext", typeof(FilteringUIContext), typeof(CustomFilterEditorDialog));

        /// <summary>
        /// The <see cref="FilteringUIContext"/> this <see cref="CustomFilterEditorDialog"/> is associated with
        /// </summary>
        public FilteringUIContext FilteringContext
        {
            get => this.GetValue(FilteringContextProperty) as FilteringUIContext;
            set => this.SetValue(FilteringContextProperty, value);
        }

        public CustomFilterEditorDialog(FilteringUIContext filteringContext)
        {
            this.InitializeComponent();
            this.FilteringContext = filteringContext;
        }

        /// <summary>
        /// Executes when a user clicks the Ok button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/></param>
        private void OkClicked(object sender, RoutedEventArgs e)
        {
            this.FilterEditor.ApplyFilter();
            this.Close();
        }

        /// <summary>
        /// Executes when a user clicks the Cancel button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/></param>
        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Executes when a user clicks the Apply button
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/></param>
        private void ApplyClicked(object sender, RoutedEventArgs e)
        {
            this.FilterEditor.ApplyFilter();
        }
    }
}
