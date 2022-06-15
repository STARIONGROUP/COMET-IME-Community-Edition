// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateServerClient.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4UpdateServerDal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;

    using CDP4UpdateServerDal.Command;
    using CDP4UpdateServerDal.Dto;
    using CDP4UpdateServerDal.Enumerators;

    /// <summary>
    /// The <see cref="UpdateServerClient"/> provides a set of Method to access the Update Server Api
    /// </summary>
    [Export(typeof(IUpdateServerClient))]
    public class UpdateServerClient : BaseClient, IUpdateServerClient
    {
        /// <summary>
        /// Holds the base Update Server Address
        /// </summary>
        public const string ImeCommunityEditionUpdateServerBaseAddress = "https://store.cdp4.org";

        /// <summary>
        /// Constant used while checking for new version
        /// </summary>
        public const string ImeKey = "IME";

        /// <summary>
        /// Initializes a new <see cref="UpdateServerClient"/> Please use object initializer to set the <see cref="BaseClient.BaseAddress"/> base server address
        /// </summary>
        [ImportingConstructor]
        public UpdateServerClient() : base()
        {
        }

        /// <summary>
        /// Initializes a new <see cref="UpdateServerClient"/> for test purpose
        /// </summary>
        /// <param name="serverBaseAddress">The base Address of the Update Server</param>
        /// <param name="messageHandler">The mocked message handler</param>
        public UpdateServerClient(Uri serverBaseAddress, HttpMessageHandler messageHandler = null) : base(serverBaseAddress, messageHandler)
        {
        }

        /// <summary>
        /// Downloads the specified ime version
        /// </summary>
        /// <param name="version">The queried version</param>
        /// <param name="platform">The platform</param>
        /// <returns>A <see cref="Stream"/></returns>
        public async Task<Stream> DownloadIme(string version, Platform platform)
        {
            return await this.QueryStream<ImeDto>(HttpVerbs.Get, $"{version}/{platform}/download");
        }

        /// <summary>
        /// Downloads the specified plugin in the specified version
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns>A <see cref="Stream"/></returns>
        public async Task<Stream> DownloadPlugin(string name, string version)
        {
            return await this.QueryStream<PluginDto>(HttpVerbs.Get, $"{name}/{version}/download");
        }

        /// <summary>
        /// Gets the latest version for all plugin
        /// </summary>
        /// <param name="imeVersion">The Current IME <see cref="Version"/></param>
        /// <param name="manifests">A <see cref="IEnumerable{T}"/> of the currently installed plugin </param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PluginDto"/> </returns>
        public async Task<IEnumerable<PluginDto>> GetLatestPlugin(IEnumerable<Manifest> manifests, Version imeVersion)
        {
            var requestBody = new ConsolidatePluginCommand()
            {
                IMEVersion = imeVersion.ToString(),
                ClientPlugins = manifests.Select(p => new ClientPluginDto()
                {
                    Version = p.Version,
                    Name = p.Name
                }).ToList()
            };

            return await this.GetLatest<List<PluginDto>>(requestBody);
        }

        /// <summary>
        /// Gets the latest version available of the IME
        /// </summary>
        /// <param name="version">The current version</param>
        /// <param name="platform">The compatible platform <see cref="Platform"/></param>
        /// <returns>An <see cref="ImeVersionDto"/></returns>
        public async Task<ImeVersionDto> GetLatestIme(Version version, Platform platform = Platform.X64)
        {
            var requestBody = new ConsolidateImeCommand()
            {
                ClientIME = new ClientImeDto()
                {
                    Platform = platform.ToString(),
                    Version = version.ToString()
                }
            };

            var response = await this.GetLatest<ImeDto>(requestBody);
            return response.Versions.FirstOrDefault();
        }

        /// <summary>
        /// Gets Latest from the API of object type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <returns>A <see cref="T"/> instance</returns>
        private async Task<T> GetLatest<T>(object content) where T : new()
        {
            if (typeof(T) == typeof(List<PluginDto>))
            {
                return await this.Query<PluginDto, T>(HttpVerbs.Post, null, this.WrapContent(content));
            }

            return await this.Query<T, T>(HttpVerbs.Post, null, this.WrapContent(content));
        }

        /// <summary>
        /// Compares current installed plugin version, current IME version installed and what versions are available on the Update Server
        /// </summary>
        /// <param name="manifests">A <see cref="List{T}"/> of the currently installed plugin </param>
        /// <param name="version">The Current IME <see cref="Version"/></param>
        /// <param name="processorArchitecture">The current <see cref="ProcessorArchitecture"/> target of the currently installed IME</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of type <code>(string ThingName, string Version)</code> containing new versions</returns>
        public async Task<IEnumerable<(string ThingName, string Version)>> CheckForUpdate(List<Manifest> manifests, Version version, ProcessorArchitecture processorArchitecture)
        {
            var result = new List<(string ThingName, string Version)>();
            var newImeVersion = await this.CompareImeVersions(version, processorArchitecture);

            if (newImeVersion != null)
            {
                result.Add((ImeKey, newImeVersion));
            }

            result.AddRange(await this.ComparePluginsVersions(version, manifests));
            return result;
        }

        /// <summary>
        /// Compares the current version with the latest one found on the update server 
        /// </summary>
        /// <param name="version">The Current IME <see cref="Version"/></param>
        /// <param name="processorArchitecture">The current <see cref="ProcessorArchitecture"/> target of the currently installed IME</param>
        /// <returns>A <see cref="Version"/> if any new</returns>
        private async Task<string> CompareImeVersions(Version version, ProcessorArchitecture processorArchitecture)
        {
            var latest = await this.GetLatestIme(version, processorArchitecture == ProcessorArchitecture.Amd64 ? Platform.X64 : Platform.X86);
            var versionSplittedUp = latest.VersionNumber.Split(new[] { '-', 'R', 'C' }, StringSplitOptions.RemoveEmptyEntries);
            var latestVersion = new Version(versionSplittedUp[0]);
            return latestVersion > version ? latest.VersionNumber : null;
        }

        /// <summary>
        /// Compares installed Plugins versions with the latest one found online
        /// </summary>
        /// <param name="imeVersion">The Current IME <see cref="Version"/></param>
        /// <param name="manifests">A <see cref="List{T}"/> of the currently installed plugin </param>
        /// <returns>A <see cref="IEnumerable{T}"/> of type <code>(string ThingName, string Version)</code> containing new versions</returns>
        private async Task<IEnumerable<(string ThingName, string Version)>> ComparePluginsVersions(Version imeVersion, List<Manifest> manifests)
        {
            var latest = (await this.GetLatestPlugin(manifests, imeVersion)).ToList();
            var result = new List<(string ThingName, string Version)>();

            foreach (var manifest in manifests)
            {
                var latestVersionFound = latest.FirstOrDefault(p => p.Name == manifest.Name)?.Versions.FirstOrDefault()?.VersionNumber;

                if (string.IsNullOrWhiteSpace(latestVersionFound))
                {
                    continue;
                }

                var newVersion = new Version(latestVersionFound);

                if (new Version(manifest.Version) < newVersion)
                {
                    result.Add((manifest.Name, latestVersionFound));
                }
            }

            return result;
        }
    }
}
