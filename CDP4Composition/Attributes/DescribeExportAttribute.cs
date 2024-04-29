// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DescribeExportAttribute.cs" company="Starion Group S.A.">
//   Copyright (c) 2017 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;

    /// <summary>
    /// The purpose of the <see cref="DescribeExportAttribute"/> is to decorate implementations of the CDP4 components to give human readable information about them.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class DescribeExportAttribute : ExportAttribute, IDescribeMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DescribeExportAttribute"/> class.
        /// </summary>
        /// <param name="name">The human readable name of the component implementation that is being decorated.</param>
        /// <param name="description">The human readable description of the component implementation that is being decorated.</param>
        /// <param name="type">The type of the component implementation that is being decorated.</param>
        public DescribeExportAttribute(string name, string description, Type type) : base (type)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets a human readable name for a CDP4 component.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a human readable description for a CDP4 component.
        /// </summary>
        public string Description { get; private set; }
    }
}