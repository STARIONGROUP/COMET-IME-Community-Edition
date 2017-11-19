﻿// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for RequirementsSpecificationDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.RequirementsSpecification)]
    public partial class RequirementsSpecificationDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationDialog"/> class.
        /// </summary>
        public RequirementsSpecificationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public RequirementsSpecificationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
