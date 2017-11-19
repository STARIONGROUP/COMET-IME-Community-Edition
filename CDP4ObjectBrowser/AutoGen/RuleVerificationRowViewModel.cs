// -------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    /// Row class representing a <see cref="RuleVerification"/>
    /// </summary>
    public abstract partial class RuleVerificationRowViewModel<T> : ObjectBrowserRowViewModel<T>, IRuleVerificationRowViewModel<T> where T :RuleVerification
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ViolationRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel violationFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationRowViewModel{T}"/> class
        /// </summary>
        /// <param name="ruleVerification">The <see cref="RuleVerification"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected RuleVerificationRowViewModel(T ruleVerification, ISession session, IViewModelBase<Thing> containerViewModel) : base(ruleVerification, session, containerViewModel)
        {
            this.violationFolder = new CDP4Composition.FolderRowViewModel("Violation", "Violation", this.Session, this);
            this.ContainedRows.Add(this.violationFolder);
            this.UpdateProperties();
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
            this.ComputeRows(this.Thing.Violation, this.violationFolder, this.AddViolationRowViewModel);
        }
        /// <summary>
        /// Add an Violation row view model to the list of <see cref="Violation"/>
        /// </summary>
        /// <param name="violation">
        /// The <see cref="Violation"/> that is to be added
        /// </param>
        private RuleViolationRowViewModel AddViolationRowViewModel(RuleViolation violation)
        {
            return new RuleViolationRowViewModel(violation, this.Session, this);
        }
    }
}
