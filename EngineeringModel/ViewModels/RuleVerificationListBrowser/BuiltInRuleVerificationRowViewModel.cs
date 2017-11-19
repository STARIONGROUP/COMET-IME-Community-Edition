// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleVerificationRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;

    /// <summary>
    /// A row representing a <see cref="BuiltInRuleVerification"/>
    /// </summary>
    public class BuiltInRuleVerificationRowViewModel : RuleVerificationRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRuleVerificationRowViewModel"/> class.
        /// </summary>
        /// <param name="builtInRuleVerification">
        /// The <see cref="BuiltInRuleVerification"/> that is represented by the current row-view-model.
        /// </param>
        /// <param name="session">
        /// The current active <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The view-model that is the container of the current row-view-model.
        /// </param>
        public BuiltInRuleVerificationRowViewModel(BuiltInRuleVerification builtInRuleVerification, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(builtInRuleVerification, session, containerViewModel)
        { 
        }
    }
}
