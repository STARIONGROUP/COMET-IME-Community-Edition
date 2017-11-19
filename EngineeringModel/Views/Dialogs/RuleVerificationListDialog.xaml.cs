// ------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for RuleVerificationListDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.RuleVerificationList)]
    public partial class RuleVerificationListDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListDialog"/> class.
        /// </summary>
        public RuleVerificationListDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public RuleVerificationListDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
