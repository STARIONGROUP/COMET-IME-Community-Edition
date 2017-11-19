// -------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for RelationalExpressionDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.RelationalExpression)]
    public partial class RelationalExpressionDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParametricConstraintDialog"/> class.
        /// </summary>
        public RelationalExpressionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalExpressionDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public RelationalExpressionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}