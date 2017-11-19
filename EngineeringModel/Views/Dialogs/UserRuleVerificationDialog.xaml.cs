// ------------------------------------------------------------------------------------------------
// <copyright file="UserRuleVerificationDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for <see cref="UserRuleVerificationDialog"/> XAML
    /// </summary>
    [ThingDialogViewExport(ClassKind.UserRuleVerification)]
    public partial class UserRuleVerificationDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRuleVerificationDialog"/> class.
        /// </summary>
        public UserRuleVerificationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRuleVerificationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public UserRuleVerificationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
