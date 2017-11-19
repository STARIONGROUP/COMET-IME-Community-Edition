﻿// ------------------------------------------------------------------------------------------------
// <copyright file="DerivedQuantityKindDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for <see cref="DerivedQuantityKindDialog"/> XAML
    /// </summary>
    [ThingDialogViewExport(ClassKind.DerivedQuantityKind)]
    public partial class DerivedQuantityKindDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedQuantityKindDialog"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used by MEF to instation the view. The view is instantiated to enable navigation using the <see cref="IThingDialogNavigationService"/>
        /// </remarks>
        public DerivedQuantityKindDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedQuantityKindDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public DerivedQuantityKindDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}