// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.cs" company="RHEA System S.A.">
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

namespace CDP4PluginPackager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Reflection;
    using System.Xml.Serialization;

    using CDP4PluginPackager.Models;
    using CDP4PluginPackager.Models.AutoGen;
    using CDP4PluginPackager.Utilities;

    using Newtonsoft.Json;

    /// <summary>
    /// App class handles plugin manifest generation and packing
    /// </summary>
    public class App
    {
        /// <summary>
        /// Field that holds the value whether Plugin is to be packed in a zip
        /// </summary>
        private readonly bool shouldPluginGetPacked;

        /// <summary>
        /// Fields that holds the working directory
        /// </summary>
        private readonly string path;

        /// <summary>
        /// Gets or Sets the path where the plugin dll is and where the manifest is
        /// </summary>
        public string OutputPath { get; private set; }

        /// <summary>
        /// Gets or sets the deserialized <see cref="CsprojectFile"/>
        /// </summary>
        public CsprojectFile Csproj { get; private set; }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of custom attribute data reflected from the target plugin
        /// </summary>
        public IEnumerable<CustomAttributeData> AssemblyInfo { get; private set; }
        
        /// <summary>
        /// The actual manifest meant to be serialized
        /// </summary>
        public Manifest Manifest { get; set; }

        /// <summary>
        /// Instantiate a new <see cref="App"/>
        /// </summary>
        /// <param name="args"></param>
        public App(string[] args = null)
        {
            if (args == null || !args.Any(Directory.Exists))
            {
                this.path = Directory.GetCurrentDirectory();
            }
            else
            {
                this.path = args.FirstOrDefault(Directory.Exists);
            }

            this.shouldPluginGetPacked = args?.Any(a => a.ToLower() == "pack") == true;
        }

        /// <summary>
        /// Wrapper calling every needed method in the right order to generate the Plugin Manifest and pack the dll and manifest <see cref="shouldPluginGetPacked"/>
        /// </summary>
        public void Start()
        {
            this.Deserialize();
            this.GetAssemblyInfo();
            this.BuildManifest();
            this.Serialize();
            this.WriteLicense();

            if (this.shouldPluginGetPacked)
            {
                Console.WriteLine("---- Packing starting ----");
                this.Pack();
                Console.WriteLine("---- Packing done ----");
            }
        }

        /// <summary>
        /// Write the license file in the output folder
        /// </summary>
        private void WriteLicense()
        {
            File.WriteAllText($"{Path.Combine(this.OutputPath, this.Manifest.Name)}.license.txt", this.GetLicense()); 
        }

        /// <summary>
        /// Set properties of the <see cref="Manifest"/>
        /// </summary>
        private void BuildManifest()
        {
            this.Manifest = new Manifest
            {
                Name = this.AssemblyInfo.QueryAssemblySpecificInfo<AssemblyTitleAttribute>(),
                Version = this.AssemblyInfo.QueryAssemblySpecificInfo<AssemblyFileVersionAttribute>(),
                ProjectGuid = this.Csproj.PropertyGroup.First(p => p.ProjectGuid != Guid.Empty).ProjectGuid,
                TargetFramework = this.Csproj.PropertyGroup.First(p => !string.IsNullOrWhiteSpace(p.TargetFrameworkVersion)).TargetFrameworkVersion,
                Author = "RHEA System S.A.",
                Website = "https://store.cdp4.org",
                Description = this.AssemblyInfo.QueryAssemblySpecificInfo<AssemblyDescriptionAttribute>(),
                ReleaseNote = this.GetReleaseNote()
            };
        }

        /// <summary>
        /// Retrieve the license and set its property
        /// </summary>
        /// <returns>license as <see cref="string"/></returns>
        public string GetLicense()
        {
            var licensePath = Directory.GetParent(this.path).EnumerateFiles().FirstOrDefault(f => f.Name == "PluginLicense.txt")?.FullName;

            if (string.IsNullOrWhiteSpace(licensePath))
            {
                Console.WriteLine("---- Warning no license file has been found ----");
                return null;
            }

            return File.ReadAllText(licensePath)
                .Replace("$PLUGIN_NAME", this.AssemblyInfo.QueryAssemblySpecificInfo<AssemblyTitleAttribute>())
                .Replace("$YEAR", DateTime.Now.Year.ToString());
        }

        /// <summary>
        /// Retrieve relative release note if any
        /// </summary>
        /// <returns>release note as <see cref="string"/></returns>
        private string GetReleaseNote()
        {
            var releaseNotePath = Directory.EnumerateFiles(this.path).FirstOrDefault(f => f.ToLower() == "releasenote.md");

            if (string.IsNullOrWhiteSpace(releaseNotePath))
            {
                Console.WriteLine("---- Warning no release note has been found ----");
                return null;
            }

            return File.ReadAllText(releaseNotePath);
        }

        /// <summary>
        /// Retrieve all the plugins references
        /// </summary>
        /// <returns>returns all pluging references as <see cref="IEnumerable{Reference}"/></returns>
        private IEnumerable<Reference> ComputeReferences()
        {
            return this.Csproj.ItemGroup.SelectMany(r => r.Reference);
        }

        /// <summary>
        /// Fetches the current IME version to set the IME version that the plugin is compatible with
        /// </summary>
        public Version GetCurrentIMEVersion()
        {
            var imePath = Path.GetFullPath(Path.Combine(this.OutputPath, @"..\..\", "CDP4IME.exe"));

            if (!File.Exists(imePath))
            {
                throw new FileNotFoundException("You should build IME first");
            }

            return Assembly.LoadFrom(imePath).GetName().Version;
        }
        
        /// <summary>
        /// Retrieve Plugin assembly info from reflection
        /// </summary>
        public void GetAssemblyInfo()
        {
            this.OutputPath = this.Csproj.PropertyGroup.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.OutputPath))?.OutputPath;
            var assemblyName = this.Csproj.PropertyGroup.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.AssemblyName))?.AssemblyName;
            var dllpath = $"{this.OutputPath}{assemblyName}.dll";
            this.AssemblyInfo = Assembly.LoadFrom(dllpath).GetCustomAttributesData();
        }
        
        /// <summary>
        /// Serialize the <see cref="Manifest"/> into Json format and write the Json file in the Plugin output directory next to the plugin binary 
        /// </summary>
        public void Serialize()
        {
            var output = JsonConvert.SerializeObject(this.Manifest);
            File.WriteAllText($"{Path.Combine(this.OutputPath, this.Manifest.Name)}.plugin.manifest",output);
        }

        /// <summary>
        /// Deserialize the target Plugin Csproj and set <see cref="Csproj"/> property
        /// </summary>
        public void Deserialize()
        {
            var csprojPath = Directory.EnumerateFiles(this.path).FirstOrDefault(f => f.EndsWith(".csproj"));

            using var stream = File.OpenText(csprojPath);
            var serializer = new XmlSerializer(typeof(CsprojectFile));
            this.Csproj = (CsprojectFile)serializer.Deserialize(stream);
        }

        /// <summary>
        /// Zip all the file in the Plugin output folder including the manifest and excluding symbols. 
        /// <remarks>Building the package in another folder is mandatory</remarks>
        /// </summary>
        private void Pack()
        {
            var zipPath = Path.Combine(this.OutputPath, $"{this.Manifest.Name}.cdp4ck");
            var temporaryZipPath = Path.Combine(this.path, $"{this.Manifest.Name}.cdp4ck");
            
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
