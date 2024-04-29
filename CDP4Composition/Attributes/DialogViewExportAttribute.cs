﻿// -------------------------------------------------------------------------------------------------
// <copyright file="DialogViewExportAttribute.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal.Composition;

    /// <summary>
    /// The purpose of the <see cref="DialogViewExportAttribute"/> is to decorate <see cref="IDialogView"/> implementations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class DialogViewExportAttribute : ExportAttribute, INameMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogViewExportAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The human readable name of the <see cref="IDialogView"/> implementation that is being decorated
        /// </param>
        /// <param name="description">
        /// The human readable description of the <see cref="IDialogView"/> implementation that is being decorated
        /// </param>
        public DialogViewExportAttribute(string name, string description)
            : base(typeof(IDialogView))
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets the human readable name of the exported <see cref="IDialogView"/>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the human readable description of the exported <see cref="IDialogView"/>
        /// </summary>
        public string Description { get; private set; }
    }
}