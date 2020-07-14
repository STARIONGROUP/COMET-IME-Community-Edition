// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginUtilities.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    using CDP4Composition.Utilities;

    using Microsoft.Practices.ServiceLocation;

    using Newtonsoft.Json;

    /// <summary>
    /// Utility class that help with plugin management
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class PluginUtilities
    {
        /// <summary>
        /// The name of the plugin directory
        /// </summary>
        public const string PluginDirectoryName = "plugins";

        /// <summary>
        /// Gets the plugin <see cref="Manifest"/> present in the IME plugin folder
        /// </summary>
        /// <returns><see cref="IEnumerable{Manifest}"/> containing all founds plugin manifests</returns>
        public static IEnumerable<Manifest> GetPluginManifests()
        {
            var manifests = new List<Manifest>();

#if DEBUG
            var currentConfiguration = "Debug";
#else
            var currentConfiguration = "Release";
#endif

            var directoryInfo = PluginDirectoryExists(out var specificPluginFolderExists);

            if (directoryInfo.Exists)
            {
                foreach (var manifest in directoryInfo.EnumerateFiles("*.plugin.manifest", SearchOption.AllDirectories))
                {
                    if (!specificPluginFolderExists && (!manifest.Directory?.FullName.Contains($"{Path.DirectorySeparatorChar}{currentConfiguration}{Path.DirectorySeparatorChar}") ?? true))
                    {
                        continue;
                    }

                    manifests.Add(JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(manifest.FullName)));
                }
            }
            else
            {
                throw new FileNotFoundException();
            }

            return manifests;
        }

        /// <summary>
        /// Returns a <see cref="DirectoryInfo"/> object tha contains the root folder where to search for .manifest files
        /// </summary>
        /// <param name="specificPluginFolderExists">States if a specific plugin directory exists</param>
        /// <returns>The <see cref="DirectoryInfo"/></returns>
        public static DirectoryInfo PluginDirectoryExists(out bool specificPluginFolderExists)
        {
            var currentPath = ServiceLocator.Current.GetInstance<IAssemblyLocationLoader>().GetLocation();

            var directoryInfo = new DirectoryInfo(Path.Combine(currentPath, PluginDirectoryName));
            specificPluginFolderExists = directoryInfo.Exists;

            if (!specificPluginFolderExists)
            {
                directoryInfo = new DirectoryInfo(currentPath);
            }

            return directoryInfo;
        }
    }
}
