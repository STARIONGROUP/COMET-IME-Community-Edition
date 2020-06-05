// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SdkPluginPackager.cs" company="RHEA System S.A.">
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
    using System.IO;
    using System.Linq;

    using CDP4PluginPackager.Models.Sdk;

    /// <summary>
    /// SdkPluginPackager class handles plugin manifest generation and packing for Sdk type project files
    /// </summary>
    public class SdkPluginPackager : BasePluginPackager<SdkCsprojectfile> 
    {
        /// <summary>
        /// Instantiate a new <see cref="SdkPluginPackager"/>
        /// </summary>
        /// <param name="path">the working directory</param>
        /// <param name="shouldPluginGetPacked">state if a plugin needs to be packed in a zip</param>
        /// <param name="buildConfiguration">the current build configuration (Debug/Release)</param>
        /// <param name="buildTargetFramework">The target framework version (net452)</param>
        public SdkPluginPackager(string path, bool shouldPluginGetPacked, string buildConfiguration, string buildTargetFramework) : base(path, shouldPluginGetPacked, buildConfiguration, buildTargetFramework)
        {
        }

        ///// <summary>
        ///// Retrieve Plugin assembly info from reflection; The Sdk version
        ///// </summary>
        protected override void GetAssemblyInfo()
        {
            this.BuildManifest();
            
            this.OutputPath =
                System.IO.Path.Combine(
                    this.Csproj.PropertyGroup
                        .First(d => !string.IsNullOrWhiteSpace(d.OutputPath))
                        .OutputPath
                        .Replace("$(Configuration)", this.BuildConfiguration)
                        .Replace("$(TargetFramework)", this.BuildTargetFramework));

            if (!Directory.Exists(this.OutputPath))
            {
                Directory.CreateDirectory(this.OutputPath);
            }

            this.PluginName = this.Csproj.PropertyGroup.First(d => !string.IsNullOrWhiteSpace(d.AssemblyTitle))?.AssemblyTitle ?? "";

            this.Manifest.Name = this.PluginName;
            this.Manifest.Version = this.Csproj.PropertyGroup.First(d => !string.IsNullOrWhiteSpace(d.AssemblyVersion))?.AssemblyVersion ?? "";
            this.Manifest.ProjectGuid = Guid.Parse(this.Csproj.PropertyGroup.FirstOrDefault(p => p.ProjectGuid != string.Empty)?.ProjectGuid ?? "");
            this.Manifest.TargetFramework = this.Csproj.PropertyGroup.First(p => !string.IsNullOrWhiteSpace(p.TargetFramework)).TargetFramework;
            this.Manifest.Description = this.Csproj.PropertyGroup.First(d => !string.IsNullOrWhiteSpace(d.Description))?.Description ?? "";
        }
    }
}
