// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoadingControl.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for LoadingControl.xaml
    /// </summary>
    public partial class LoadingControl : UserControl
    {
        /// <summary>
        /// The dependency property that allows setting the cancel <see cref="ICommand"/>
        /// </summary>
        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(LoadingControl), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// The dependency property that allows setting the visibility of the Cancel button.
        /// </summary>
        public static readonly DependencyProperty IsCancelVisibleProperty = DependencyProperty.Register("IsCancelVisible", typeof(bool), typeof(LoadingControl), new FrameworkPropertyMetadata(false, OnCancelVisibleChanged));

        public LoadingControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cancel button is visible
        /// </summary>
        /// <value>
        ///   true if cancel button is visible; otherwise, false.
        /// </value>
        public bool IsCancelVisible
        {
            get => (bool)this.GetValue(IsCancelVisibleProperty);
            set => this.SetValue(IsCancelVisibleProperty, value);
        }

        /// <summary>
        /// Gets or sets the cancel <see cref="ICommand"/>
        /// </summary>
        public ICommand CancelCommand
        {
            get => (ICommand)this.GetValue(CancelCommandProperty);
            set => this.SetValue(CancelCommandProperty, value);
        }

        /// <summary>
        /// Called when start in preview mode changed.
        /// </summary>
        /// <param name="caller">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnCancelVisibleChanged(DependencyObject caller, DependencyPropertyChangedEventArgs e)
        {
            if (caller is LoadingControl control)
            {
                control.CancelGrid.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
                control.NormalImage.Visibility = (bool)e.NewValue ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
