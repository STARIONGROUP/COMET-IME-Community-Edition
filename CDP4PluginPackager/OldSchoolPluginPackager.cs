// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OldSchoolPluginPackager.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Reflection;

    using CDP4PluginPackager.Models.AutoGen;
    using CDP4PluginPackager.Utilities;

    /// <summary>
    /// OldSchoolPluginPackager class handles plugin manifest generation and packing for old school (non-Sdk) project files
    /// </summary>
    public class OldSchoolPluginPackager : BasePluginPackager<CsprojectFile> 
    {
        /// <summary>
        /// Instantiate a new <see cref="OldSchoolPluginPackager"/>
        /// </summary>
        /// <param name="path">the working directory</param>
        /// <param name="shouldPluginGetPacked">state if a plugin needs to be packed in a zip</param>
        /// <param name="buildConfiguration">the current build configuration (Debug/Release)</param>
        public OldSchoolPluginPackager(string path, bool shouldPluginGetPacked, string buildConfiguration = "") : base(path, shouldPluginGetPacked, buildConfiguration)
        {
        }

        /// <summary>
        /// Retrieve Plugin assembly info from reflection
        /// </summary>
        protected override void GetAssemblyInfo()
        {
            this.BuildManifest();

            var assemblyName = this.Csproj.PropertyGroup.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.AssemblyName))?.AssemblyName;
            this.OutputPath = this.Csproj.PropertyGroup.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.OutputPath))?.OutputPath;
            var dllpath = $"{this.OutputPath}{assemblyName}.dll";
            var assemblyInfo = Assembly.LoadFrom(dllpath).GetCustomAttributesData();

            this.pluginName = assemblyInfo.QueryAssemblySpecificInfo<AssemblyTitleAttribute>();

            this.Manifest.Name = this.pluginName;
            this.Manifest.Version = assemblyInfo.QueryAssemblySpecificInfo<AssemblyFileVersionAttribute>();
            this.Manifest.ProjectGuid = this.Csproj.PropertyGroup.First(p => p.ProjectGuid != Guid.Empty).ProjectGuid;
            this.Manifest.TargetFramework = this.Csproj.PropertyGroup.First(p => !string.IsNullOrWhiteSpace(p.TargetFrameworkVersion)).TargetFrameworkVersion;
            this.Manifest.Description = assemblyInfo.QueryAssemblySpecificInfo<AssemblyDescriptionAttribute>();
        }
    }
}
