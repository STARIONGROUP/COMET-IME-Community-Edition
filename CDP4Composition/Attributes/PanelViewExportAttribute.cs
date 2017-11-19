// -------------------------------------------------------------------------------------------------
// <copyright file="PanelViewExportAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;

    /// <summary>
    /// The purpose of the <see cref="PanelViewExportAttribute"/> is to decorate <see cref="IPanelView"/> implementations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class PanelViewExportAttribute : ExportAttribute, IRegionMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PanelViewExportAttribute"/> class.
        /// </summary>
        /// <param name="region">
        /// The region in which the <see cref="IPanelView"/> shall be displayed
        /// </param>
        public PanelViewExportAttribute(string region)
            : base(typeof(IPanelView))
        {
            this.Region = region;
        }

        /// <summary>
        /// Gets the region of the Panel
        /// </summary>
        public string Region { get; private set; }
    }
}