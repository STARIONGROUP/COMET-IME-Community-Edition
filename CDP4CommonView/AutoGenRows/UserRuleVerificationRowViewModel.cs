// -------------------------------------------------------------------------------------------------
// <copyright file="UserRuleVerificationRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="UserRuleVerification"/>
    /// </summary>
    public partial class UserRuleVerificationRowViewModel : RuleVerificationRowViewModel<UserRuleVerification>
    {

        /// <summary>
        /// Backing field for <see cref="Rule"/>
        /// </summary>
        private Rule rule;

        /// <summary>
        /// Backing field for <see cref="RuleShortName"/>
        /// </summary>
        private string ruleShortName;

        /// <summary>
        /// Backing field for <see cref="RuleName"/>
        /// </summary>
        private string ruleName;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRuleVerificationRowViewModel"/> class
        /// </summary>
        /// <param name="userRuleVerification">The <see cref="UserRuleVerification"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public UserRuleVerificationRowViewModel(UserRuleVerification userRuleVerification, ISession session, IViewModelBase<Thing> containerViewModel) : base(userRuleVerification, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Rule
        /// </summary>
        public Rule Rule
        {
            get { return this.rule; }
            set { this.RaiseAndSetIfChanged(ref this.rule, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Rule"/>
        /// </summary>
        public string RuleShortName
        {
            get { return this.ruleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ruleShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Rule"/>
        /// </summary>
        public string RuleName
        {
            get { return this.ruleName; }
            set { this.RaiseAndSetIfChanged(ref this.ruleName, value); }
        }

	
        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            this.Rule = this.Thing.Rule;
        }
    }
}
