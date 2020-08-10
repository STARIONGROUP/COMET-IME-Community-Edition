// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.cs" company="RHEA System S.A.">
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

namespace CDP4UpdateServerDal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    using CDP4UpdateServerDal.Dto;
    using CDP4UpdateServerDal.Enumerators;

    /// <summary>
    /// The <see cref="Client"/> provides a set of Method to access the Update Server Api
    /// </summary>
    public class Client : BaseClient
    {
        /// <summary>
        /// Initializes a new <see cref="Client"/>
        /// </summary>
        /// <param name="serverBaseAddress">The base Address of the Update Server</param>
        public Client(Uri serverBaseAddress) : base(serverBaseAddress)
        {
        }

        /// <summary>
        /// Downloads the specified ime version
        /// </summary>
        /// <param name="version">The queried version</param>
        /// <param name="platform">The platform</param>
        /// <returns>A <see cref="Stream"/></returns>
        public async Task<Stream> DownloadIme(Version version, Platform platform)
        {
            return await this.QueryStream<IMEDto>(HttpVerbs.Get, $"{version}/{platform}");
        }

        /// <summary>
        /// Downloads the specified plugin in the specified version
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns>A <see cref="Stream"/></returns>
        public async Task<Stream> DownloadPlugin(string name, Version version)
        {
            return await this.QueryStream<PluginDto>(HttpVerbs.Get, $"{name}/{version}");
        }

        /// <summary>
        /// Gets the latest version for all plugin
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PluginDto"/> </returns>
        public async Task<IEnumerable<PluginDto>> GetLatestPlugin()
        {
            return await this.GetLatest<List<PluginDto>>();
        }

        /// <summary>
        /// Gets the latest version available of the IME
        /// </summary>
        /// <param name="platform">The compatible platform <see cref="Platform"/></param>
        /// <param name="includingReleaseCandidate">An assert whether this can return a Release Candidate</param>
        /// <returns>An <see cref="IMEVersionDto"/></returns>
        public async Task<IMEVersionDto> GetLatestIme(Platform platform = Platform.X64, bool includingReleaseCandidate = false)
        {
            return (await this.GetLatest<IMEDto>()).Versions?
                .OrderByDescending(v => v.ReleaseDate)
                .FirstOrDefault(v => v.Platforms.Any(p => p.Platform == platform) &&
                (includingReleaseCandidate || !v.IsPreRelease));
        }

        /// <summary>
        /// Gets Latest from the API of object type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <returns>A <see cref="T"/> instance</returns>
        private async Task<T> GetLatest<T>() where T : new()
        {
                return await this.Query<T, T>(HttpVerbs.Get);
        }
    }
}
