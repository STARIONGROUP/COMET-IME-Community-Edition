// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginUpdateInstallerBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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


namespace CDP4Composition.Behaviors
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Documents;

    using CDP4Composition.ViewModels;
    using CDP4Composition.Views;

    using DevExpress.Mvvm.UI.Interactivity;

    using ReactiveUI;

    /// <summary>
    /// Behavior of the e<see cref="PluginInstaller"/> used to handle the interactions between <see cref="PluginInstallerViewModel"/> and the view
    /// </summary>
    public class PluginUpdateInstallerBehavior : Behavior<PluginInstaller>, IPluginUpdateInstallerBehavior
    {
        /// <summary>
        /// Register event handlers
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.DataContextChanged += this.OnDataContextChanged;
            this.AssociatedObject.Closing += this.OnClosing;
        }
        
        /// <summary>
        /// Unregister event handlers
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.DataContextChanged -= this.OnDataContextChanged;
            this.AssociatedObject.Closing -= this.OnClosing;
        }

        /// <summary>
        /// Occurs when the user request the <see cref="PluginInstaller"/> view to close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosing(object sender, CancelEventArgs e)
        {
            (this.AssociatedObject.DataContext as IPluginInstallerViewModel)?.CancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Fires whenever the Data contex changes
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments <see cref="System.Windows.DependencyPropertyChangedEventArgs"/></param>
        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IPluginInstallerViewModel viewModel)
            {
                viewModel.Behavior = this;
            }
        }

        /// <summary>
        /// Closes the Window
        /// </summary>
        public void Close()
        {
            this.AssociatedObject?.Close();
        }
    }
}
