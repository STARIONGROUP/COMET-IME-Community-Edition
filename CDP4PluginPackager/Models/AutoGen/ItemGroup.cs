﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ItemGroup.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// <summary>
//   This file has been generated using a csproj file with this tool to convert Xml structure into Csharp: https://xmltocsharp.azurewebsites.net/
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4PluginPackager.Models.AutoGen
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// <summary>
    /// Autogenerated class matching a "ItemGroup" tag in a csproj file
    /// </summary>
    [XmlRoot(ElementName = "ItemGroup", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class ItemGroup
    {
        [XmlElement(ElementName = "Reference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public List<Reference> Reference { get; set; }

        [XmlElement(ElementName = "Compile", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public List<Compile> Compile { get; set; }

        [XmlElement(ElementName = "ProjectReference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public ProjectReference ProjectReference { get; set; }

        [XmlElement(ElementName = "Page", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public List<Page> Page { get; set; }

        [XmlElement(ElementName = "EmbeddedResource", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public EmbeddedResource EmbeddedResource { get; set; }

        [XmlElement(ElementName = "None", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public None None { get; set; }
    }
}