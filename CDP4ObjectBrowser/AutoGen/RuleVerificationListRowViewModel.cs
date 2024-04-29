// -------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="RuleVerificationList"/>
    /// </summary>
    public partial class RuleVerificationListRowViewModel : DefinedThingRowViewModel<RuleVerificationList>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="RuleVerificationRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel ruleVerificationFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListRowViewModel"/> class
        /// </summary>
        /// <param name="ruleVerificationList">The <see cref="RuleVerificationList"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public RuleVerificationListRowViewModel(RuleVerificationList ruleVerificationList, ISession session, IViewModelBase<Thing> containerViewModel) : base(ruleVerificationList, session, containerViewModel)
        {
            this.ruleVerificationFolder = new CDP4Composition.FolderRowViewModel("Rule Verification", "Rule Verification", this.Session, this);
            this.ContainedRows.Add(this.ruleVerificationFolder);
            this.UpdateProperties();
            this.UpdateColumnValues();
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
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.RuleVerification, this.ruleVerificationFolder, this.AddRuleVerificationRowViewModel);
        }
        /// <summary>
        /// Add an Rule Verification row view model to the list of <see cref="RuleVerification"/>
        /// </summary>
        /// <param name="ruleVerification">
        /// The <see cref="RuleVerification"/> that is to be added
        /// </param>
        private IRuleVerificationRowViewModel<RuleVerification> AddRuleVerificationRowViewModel(RuleVerification ruleVerification)
        {
        var userRuleVerification = ruleVerification as UserRuleVerification;
        if (userRuleVerification != null)
        {
            return new UserRuleVerificationRowViewModel(userRuleVerification, this.Session, this);
        }
        var builtInRuleVerification = ruleVerification as BuiltInRuleVerification;
        if (builtInRuleVerification != null)
        {
            return new BuiltInRuleVerificationRowViewModel(builtInRuleVerification, this.Session, this);
        }
        throw new Exception("No RuleVerification to return");
        }
    }
}
