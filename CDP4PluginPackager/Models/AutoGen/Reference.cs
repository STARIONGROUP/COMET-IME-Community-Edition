﻿// -------------------------------------------------------------------------------------------------
// <copyright file="Reference.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4PluginPackager.Models.AutoGen
{
    using System.Xml.Serialization;

    /// <summary>
    /// Autogenerated class matching a "Reference" tag in a csproj file
    /// </summary>
    [XmlRoot(ElementName = "Reference", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
    public class Reference
    {
        [XmlElement(ElementName = "HintPath", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string HintPath { get; set; }

        [XmlElement(ElementName = "Private", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string Private { get; set; }

        [XmlAttribute(AttributeName = "Include")]
        public string Include { get; set; }

        [XmlElement(ElementName = "EmbedInteropTypes", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string EmbedInteropTypes { get; set; }

        [XmlElement(ElementName = "SpecificVersion", Namespace = "http://schemas.microsoft.com/developer/msbuild/2003")]
        public string SpecificVersion { get; set; }
    }
}
