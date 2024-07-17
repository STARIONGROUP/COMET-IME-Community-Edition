// -------------------------------------------------------------------------------------------------
// <copyright file="OrExpressionDialog.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for OrExpressionDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.OrExpression)]
    public partial class OrExpressionDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrExpressionDialog"/> class.
        /// </summary>
        public OrExpressionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrExpressionDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public OrExpressionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}