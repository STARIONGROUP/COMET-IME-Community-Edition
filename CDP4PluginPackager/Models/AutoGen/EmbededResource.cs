﻿// -------------------------------------------------------------------------------------------------
// <copyright file="EmbeddedResource.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// <summary>
//   This file has been generated using a csproj file with this tool to convert Xml structure into Csharp: https://xmltocsharp.azurewebsites.net/
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4PluginPackager.Models.AutoGen
{
    using System.Xml.Serialization;

    /// <summary>
    /// Autogenerated class matching a "EmbeddedResource" tag in a csproj file
    /// </summary>
    [XmlRoot(ElementName = "EmbeddedResource", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class EmbeddedResource
    {
        [XmlElement(ElementName = "SubType", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string SubType { get; set; }

        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }
    }
}
