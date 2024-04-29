// -------------------------------------------------------------------------------------------------
// <copyright file="ThingDialogViewExportAttribute.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;
    using CDP4Common.CommonData;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// The purpose of the <see cref="ThingDialogViewExportAttribute"/> is to decorate views that represent
    /// dialog views of <see cref="Thing"/>s
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class ThingDialogViewExportAttribute : ExportAttribute, IClassKindMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDialogViewExportAttribute"/> class.
        /// </summary>
        /// <param name="classKind">
        /// The <see cref="ClassKind"/> of the <see cref="Thing"/> that is represented by the decorated <see cref="IDialogView"/>
        /// </param>
        public ThingDialogViewExportAttribute(ClassKind classKind)
            : base(typeof(IThingDialogView))
        {
            this.ClassKind = classKind;
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="Thing"/> that is represented by the decorated <see cref="IDialogView"/>
        /// </summary>
        public ClassKind ClassKind { get; private set; }
    }
}
