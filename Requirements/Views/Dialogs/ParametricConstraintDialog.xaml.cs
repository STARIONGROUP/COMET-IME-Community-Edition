// -------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for ParametricConstraintDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.ParametricConstraint)]
    public partial class ParametricConstraintDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParametricConstraintDialog"/> class.
        /// </summary>
        public ParametricConstraintDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParametricConstraintDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public ParametricConstraintDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}