// -------------------------------------------------------------------------------------------------
// <copyright file="PanelViewModelExportAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;
    using CDP4Dal.Composition;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class PanelViewModelExportAttribute : ExportAttribute, INameMetaData
    {
        public PanelViewModelExportAttribute(string name, string description) : base(typeof(IPanelViewModel))
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets the human readable name of the exported <see cref="IPanelViewModel"/>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the human readable description of the exported <see cref="IPanelViewModel"/>
        /// </summary>
        public string Description { get; private set; }
    }
}