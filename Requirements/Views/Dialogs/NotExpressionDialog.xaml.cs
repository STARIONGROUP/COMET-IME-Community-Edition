// -------------------------------------------------------------------------------------------------
// <copyright file="NotExpressionDialog.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for NotExpressionDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.NotExpression)]
    public partial class NotExpressionDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotExpressionDialog"/> class.
        /// </summary>
        public NotExpressionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotExpressionDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public NotExpressionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}