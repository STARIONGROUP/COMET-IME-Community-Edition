// ------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleVerificationDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for <see cref="BuiltInRuleVerificationDialog"/> XAML
    /// </summary>
    [ThingDialogViewExport(ClassKind.BuiltInRuleVerification)]
    public partial class BuiltInRuleVerificationDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRuleVerificationDialog"/> class.
        /// </summary>
        public BuiltInRuleVerificationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRuleVerificationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public BuiltInRuleVerificationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
