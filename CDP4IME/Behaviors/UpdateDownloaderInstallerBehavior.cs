// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginInstallerBehavior.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.Behaviors
{
    using System.ComponentModel;

    using COMET.ViewModels;
    using COMET.Views;

    using DevExpress.Mvvm.UI.Interactivity;

    /// <summary>
    /// Behavior of the e<see cref="UpdateDownloaderInstaller"/> used to handle the interactions between <see cref="UpdateDownloaderInstallerViewModel"/> and the view
    /// </summary>
    public class UpdateDownloaderInstallerBehavior : Behavior<UpdateDownloaderInstaller>, IUpdateDownloaderInstallerBehavior
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
        /// Occurs when the user request the <see cref="UpdateDownloaderInstaller"/> view to close
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        private void OnClosing(object sender, CancelEventArgs e)
        {
            (this.AssociatedObject.DataContext as IUpdateDownloaderInstallerViewModel)?.CancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Fires whenever the Data contex changes
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments <see cref="System.Windows.DependencyPropertyChangedEventArgs"/></param>
        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IUpdateDownloaderInstallerViewModel viewModel)
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
