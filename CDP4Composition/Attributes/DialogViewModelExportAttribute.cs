// -------------------------------------------------------------------------------------------------
// <copyright file="DialogViewModelExportAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal.Composition;

    /// <summary>
    /// The purpose of the <see cref="DialogViewModelExportAttribute"/> is to decorate <see cref="IDialogViewModel"/> implementations
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class DialogViewModelExportAttribute : ExportAttribute, INameMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogViewModelExportAttribute"/> class.
        /// </summary>
        /// <param name="name">
        /// The human readable name of the <see cref="IDialogViewModel"/> implementation that is being decorated
        /// </param>
        /// <param name="description">
        /// The human readable description of the <see cref="IDialogViewModel"/> implementation that is being decorated
        /// </param>
        public DialogViewModelExportAttribute(string name, string description) : base(typeof(IDialogViewModel))
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets the human readable name of the exported <see cref="IDialogViewModel"/>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the human readable description of the exported <see cref="IDialogViewModel"/>
        /// </summary>
        public string Description { get; private set; }
    }
}