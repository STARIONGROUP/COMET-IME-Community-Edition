// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleExportNameAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
        public ModuleExportNameAttribute(Type type, string name) : base(type)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the human readable name of the exported <see cref="IModule"/>
        /// </summary>
        public string Name { get; private set; }
    }
}
