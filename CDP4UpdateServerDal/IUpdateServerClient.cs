// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IUpdateServerClient.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4UpdateServerDal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;

    using CDP4UpdateServerDal.Dto;
    using CDP4UpdateServerDal.Enumerators;

    public interface IUpdateServerClient
    {
        /// <summary>
        /// Gets or sets the Base address of the target Update Server
        /// </summary>
        Uri BaseAddress { get; set; }

        /// <summary>
        /// Downloads the specified ime version
        /// </summary>
        /// <param name="version">The queried version</param>
        /// <param name="platform">The platform</param>
        /// <returns>A <see cref="Stream"/></returns>
        Task<Stream> DownloadIme(string version, Platform platform);

        /// <summary>
        /// Downloads the specified plugin in the specified version
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns>A <see cref="Stream"/></returns>
        Task<Stream> DownloadPlugin(string name, string version);

        /// <summary>
        /// Gets the latest version for all plugin
        /// </summary>
        /// <param name="imeVersion">The Current IME <see cref="Version"/></param>
        /// <param name="manifests">A <see cref="IEnumerable{T}"/> of the currently installed plugin </param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PluginDto"/> </returns>
        Task<IEnumerable<PluginDto>> GetLatestPlugin(IEnumerable<Manifest> manifests, Version imeVersion);

        /// <summary>
        /// Gets the latest version available of the IME
        /// </summary>
        /// <param name="version">The current version</param>
        /// <param name="platform">The compatible platform <see cref="Platform"/></param>
        /// <returns>An <see cref="ImeVersionDto"/></returns>
        Task<ImeVersionDto> GetLatestIme(Version version, Platform platform = Platform.X64);

        /// <summary>
        /// Compares current installed plugin version, current IME version installed and what versions are available on the Update Server
        /// </summary>
        /// <param name="manifests">A <see cref="List{T}"/> of the currently installed plugin </param>
        /// <param name="version">The Current IME <see cref="Version"/></param>
        /// <param name="processorArchitecture">The current <see cref="ProcessorArchitecture"/> target of the currently installed IME</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of type <code>(string ThingName, string Version)</code> containing new versions</returns>
        Task<IEnumerable<(string ThingName, string Version)>> CheckForUpdate(List<Manifest> manifests, Version version, ProcessorArchitecture processorArchitecture);
    }
}
