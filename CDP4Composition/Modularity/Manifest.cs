// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Manifest.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.Modularity
{
    using System;

    /// <summary>
    /// Represents the Manifest file related to the target plugin
    /// </summary>
    public class Manifest
    {
        /// <summary>
        /// Gets or sets the project <see cref="Guid"/> sets in the target Plugin Csproj
        /// </summary>
        public Guid ProjectGuid { get; set; }

        /// <summary>
        /// Gets or sets the project Name sets in the target Plugin Csproj
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the project description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the version of the plugin
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or set the minimum IME version with which the plugin is compatible
        /// </summary>
        public string MinIMEVersion { get; set; }

        /// <summary>
        /// Gets or set the target framework of the Plugin
        /// </summary>
        public string TargetFramework { get; set; }

        /// <summary>
        /// Gets or sets the release note related to the target Plugin version
        /// </summary>
        public string ReleaseNote { get; set; }

        /// <summary>
        /// Gets or set the author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or set the website related to the target Plugin
        /// </summary>
        public string Website { get; set; }
    }
}
