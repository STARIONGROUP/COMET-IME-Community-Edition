// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleExportNameAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;
    using Microsoft.Practices.Prism.MefExtensions.Modularity;
    using Microsoft.Practices.Prism.Modularity;

    /// <summary>
    /// The purpose of the <see cref="ModuleExportAttribute"/> is to decorate <see cref="IModule"/> implementations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class ModuleExportNameAttribute : ModuleExportAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleExportNameAttribute"/> class.
        /// </summary>
        /// <param name="type">
        /// The <see cref="Type"/> of the module to be exported.
        /// </param>
        /// <param name="name">
        /// The human readable name of the <see cref="IModule"/> implementation that is being decorated
        /// </param>
        /// <param name="isMandatory">
        ///Specifies if the plugin should be mandatory for the application
        /// </param>
        public ModuleExportNameAttribute(Type type, string name, bool isMandatory = false) : base(type)
        {
            this.Name = name;
            this.IsMandatory = isMandatory;
        }

        /// <summary>
        /// Gets the human readable name of the exported <see cref="IModule"/>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the plugin as manadatory or not of the <see cref="IModule"/>
        /// </summary>
        public bool IsMandatory { get; set; }
    }
}
