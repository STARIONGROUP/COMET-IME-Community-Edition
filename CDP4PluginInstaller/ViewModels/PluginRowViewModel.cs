// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-Plugin Installer Community Edition. 
//    The CDP4-Plugin Installer Community Edition is the RHEA Plugin Installer for the CDP4-IME Community Edition.
//
//    The CDP4-Plugin Installer Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-Plugin Installer Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace CDP4PluginInstaller.Views
{
    using System.IO;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="PluginRow"/> holding its properties and interaction logic
    /// </summary>
    public class PluginRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for the property <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or Sets the <see cref="name"/> name of the represented object
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Gets or Sets the <see cref="description"/> name of the represented object
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="Author"/>
        /// </summary>
        private string author;

        /// <summary>
        /// Gets or Sets the <see cref="author"/> name of the represented object
        /// </summary>
        public string Author
        {
            get => this.author;
            set => this.RaiseAndSetIfChanged(ref this.author, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="InstallationProgress"/>
        /// </summary>
        private double installationProgress;

        /// <summary>
        /// Gets or Sets the <see cref="installationProgress"/> name of the represented object
        /// </summary>
        public double InstallationProgress
        {
            get => this.installationProgress;
            set => this.RaiseAndSetIfChanged(ref this.installationProgress, value);
        }

        /// <summary>
        /// Backing field for the property <see cref="IsImeVersionCompatible"/>
        /// </summary>
        private bool isImeVersionCompatible;

        /// <summary>
        /// Gets or Sets the <see cref="isImeVersionCompatible"/> name of the represented object
        /// </summary>
        public bool IsImeVersionCompatible
        {
            get => this.isImeVersionCompatible;
            set => this.RaiseAndSetIfChanged(ref this.isImeVersionCompatible, value);
        }

        /// <summary>
        /// Gets or sets the path were the cdp4ck to be installed sits 
        /// </summary>
        public string UpdatePath { get; set; }

        /// <summary>
        /// Gets or sets the path where the old version of the plugin will be temporaly kept in case anything goes wrong with the installation
        /// </summary>
        public string TemporaryPath { get; set; }

        /// <summary>
        /// Gets or sets the path where the updated plugin should be installed
        /// </summary>
        public string InstallationPath { get; set; }
    }
}
