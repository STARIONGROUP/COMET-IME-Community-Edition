// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
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

namespace CDP4PluginPackager
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;

    using CDP4PluginPackager.Models;

    using Newtonsoft.Json;

    /// <summary>
    /// BasePluginPackager class handles plugin manifest generation and packing
    /// </summary>
    public abstract class BasePluginPackager<T> where T : class
    {
        /// <summary>
        /// Field that holds the value whether Plugin is to be packed in a zip
        /// </summary>
        protected readonly bool ShouldPluginGetPacked;

        /// <summary>
        /// Fields that holds the working directory
        /// </summary>
        protected readonly string Path;

        /// <summary>
        /// Contains the current build configuration (Debug/Release)
        /// </summary>
        protected string BuildConfiguration;

        /// <summary>
        /// The target framework version (net8)
        /// </summary>
        protected string BuildTargetFramework;

        /// <summary>
        /// The build platform (AnyCpu/x64)
        /// </summary>
        protected readonly string BuildPlatform;

        /// <summary>
        /// Contains the plugin name
        /// </summary>
        protected string PluginName;

        /// <summary>
        /// Gets or Sets the path where the plugin dll is and where the manifest is
        /// </summary>
        public string OutputPath { get; protected set; }

        /// <summary>
        /// Gets or sets the deserialized project file
        /// </summary>
        public T Csproj { get; private set; }

        /// <summary>
        /// The actual manifest meant to be serialized
        /// </summary>
        public Manifest Manifest { get; set; }

        /// <summary>
        /// Default constructor for <see cref="BasePluginPackager{T}"/>
        /// </summary>
        /// <param name="path">the working directory</param>
        /// <param name="shouldPluginGetPacked">state if a plugin needs to be packed in a zip</param>
        /// <param name="buildConfiguration">the current build configuration (Debug/Release)</param>
        /// <param name="buildTargetFramework">The target framework version (net8)</param>
        /// <param name="buildPlatform">The build platform (AnyCpu/x64)</param>
        protected BasePluginPackager(string path, bool shouldPluginGetPacked, string buildConfiguration = "", string buildTargetFramework = "", string buildPlatform = "")
        {
            this.Path = path;
            this.ShouldPluginGetPacked = shouldPluginGetPacked;
            this.BuildConfiguration = buildConfiguration;
            this.BuildTargetFramework = buildTargetFramework;
            this.BuildPlatform = buildPlatform;
        }

        /// <summary>
        /// Wrapper calling every needed method in the right order to generate the Plugin Manifest and pack the dll and manifest <see cref="ShouldPluginGetPacked"/>
        /// </summary>
        public void Start()
        { 
            this.Deserialize();
            this.Serialize();

            if (this.ShouldPluginGetPacked)
            {
                Console.WriteLine("---- Packing starting ----");
                this.Pack();
                Console.WriteLine("---- Packing done ----");
            }
        }

        /// <summary>
        /// Set properties of the <see cref="Manifest"/>
        /// </summary>
        protected void BuildManifest()
        {
            this.Manifest = new Manifest
            {
                Author = "Starion Group S.A.",
                Website = "https://store.cdp4.org",
                ReleaseNote = this.GetReleaseNote()
            };
        }

        /// <summary>
        /// Retrieve relative release note if any
        /// </summary>
        /// <returns>release note as <see cref="string"/></returns>
        protected string GetReleaseNote()
        {
            var releaseNotePath = Directory.EnumerateFiles(this.Path).FirstOrDefault(f => f.ToLower() == "releasenote.md");

            if (string.IsNullOrWhiteSpace(releaseNotePath))
            {
                Console.WriteLine("---- Warning no release note has been found ----");
                return null;
            }

            return File.ReadAllText(releaseNotePath);
        }

        /// <summary>
        /// Fetches the current IME version to set the IME version that the plugin is compatible with
        /// </summary>
        protected Version GetCurrentIMEVersion()
        {
            var imePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(this.OutputPath, @"..\..\", "CDP4IME.exe"));

            if (!File.Exists(imePath))
            {
                throw new FileNotFoundException("You should build IME first");
            }

            return Assembly.LoadFrom(imePath).GetName().Version;
        }

        /// <summary>
        /// Retrieve Plugin assembly info from reflection
        /// </summary>
        protected abstract void GetAssemblyInfo();

        /// <summary>
        /// Serialize the <see cref="Manifest"/> into Json format and write the Json file in the Plugin output directory next to the plugin binary 
        /// </summary>
        protected void Serialize()
        {
            var output = JsonConvert.SerializeObject(this.Manifest);
            File.WriteAllText($"{System.IO.Path.Combine(this.OutputPath, this.Manifest.Name)}.plugin.manifest", output);
        }

        /// <summary>
        /// Deserialize the target Plugin Csproj and set <see cref="Csproj"/> property
        /// </summary>
        protected void Deserialize()
        {
            var csprojPath = Directory.EnumerateFiles(this.Path).FirstOrDefault(f => f.EndsWith(".csproj"));

            using (var stream = File.OpenText(csprojPath))
            {
                var serializer = new XmlSerializer(typeof(T));
                this.Csproj = (T) serializer.Deserialize(stream);
                this.GetAssemblyInfo();
            }
        }

        /// <summary>
        /// Zip all the file in the Plugin output folder including the manifest and excluding symbols. 
        /// <remarks>Building the package in another folder is mandatory</remarks>
        /// </summary>
        protected void Pack()
        {
            var zipPath = System.IO.Path.Combine(this.OutputPath, $"{this.Manifest.Name}.cdp4ck");
            var temporaryZipPath = System.IO.Path.Combine(this.Path, $"{this.Manifest.Name}.cdp4ck");

            CleanUpOldPackages(zipPath, temporaryZipPath);

            ZipFile.CreateFromDirectory(this.OutputPath, temporaryZipPath, CompressionLevel.Optimal, false);

            using (var zip = ZipFile.Open(temporaryZipPath, ZipArchiveMode.Update))
            {
                var selection = zip.Entries.Where(b => b.FullName.EndsWith(".pdb")).Select(x => x.Name).ToList();

                foreach (var zipEntry in selection)
                {
                    zip.GetEntry(zipEntry)?.Delete();
                }
            }

            File.Move(temporaryZipPath, zipPath);
        }

        /// <summary>
        /// Delete old package in the temporary folder and the output folder
        /// </summary>
        /// <param name="zipPath">the path where the zip is moved after creation</param>
        /// <param name="temporaryZipPath">the path where the zip is created</param>
        private static void CleanUpOldPackages(string zipPath, string temporaryZipPath)
        {
            if (File.Exists(temporaryZipPath))
            {
                File.Delete(temporaryZipPath);
            }

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
        }
    }
}
