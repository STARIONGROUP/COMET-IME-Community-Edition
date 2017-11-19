// -------------------------------------------------------------------------------------------------
// <copyright file="ThingDialogViewModelExportAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System;
    using System.ComponentModel.Composition;
    using CDP4Common.CommonData;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// The purpose of the <see cref="ThingDialogViewModelExportAttribute"/> is to decorate view~-models that represent
    /// dialog models of <see cref="Thing"/>s
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class ThingDialogViewModelExportAttribute : ExportAttribute, IClassKindMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDialogViewModelExportAttribute"/> class.
        /// </summary>
        /// <param name="classKind">
        /// The <see cref="ClassKind"/> of the <see cref="Thing"/> that is represented by the decorated <see cref="IDialogViewModel"/>
        /// </param>
        public ThingDialogViewModelExportAttribute(ClassKind classKind)
            : base(typeof(IThingDialogViewModel))
        {
            this.ClassKind = classKind;
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="Thing"/> that is represented by the decorated <see cref="IDialogViewModel"/>
        /// </summary>
        public ClassKind ClassKind { get; private set; }
    }
}
