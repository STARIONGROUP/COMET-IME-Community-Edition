// -------------------------------------------------------------------------------------------------
// <copyright file="ReferencerRuleRowViewModel.cs" company="RHEA S.A.">
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
    /// Row class representing a <see cref="ReferencerRule"/>
    /// </summary>
    public partial class ReferencerRuleRowViewModel : RuleRowViewModel<ReferencerRule>
    {

        /// <summary>
        /// Backing field for <see cref="MinReferenced"/>
        /// </summary>
        private int minReferenced;

        /// <summary>
        /// Backing field for <see cref="MaxReferenced"/>
        /// </summary>
        private int maxReferenced;

        /// <summary>
        /// Backing field for <see cref="ReferencingCategory"/>
        /// </summary>
        private Category referencingCategory;

        /// <summary>
        /// Backing field for <see cref="ReferencingCategoryShortName"/>
        /// </summary>
        private string referencingCategoryShortName;

        /// <summary>
        /// Backing field for <see cref="ReferencingCategoryName"/>
        /// </summary>
        private string referencingCategoryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencerRuleRowViewModel"/> class
        /// </summary>
        /// <param name="referencerRule">The <see cref="ReferencerRule"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ReferencerRuleRowViewModel(ReferencerRule referencerRule, ISession session, IViewModelBase<Thing> containerViewModel) : base(referencerRule, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the MinReferenced
        /// </summary>
        public int MinReferenced
        {
            get { return this.minReferenced; }
            set { this.RaiseAndSetIfChanged(ref this.minReferenced, value); }
        }

        /// <summary>
        /// Gets or sets the MaxReferenced
        /// </summary>
        public int MaxReferenced
        {
            get { return this.maxReferenced; }
            set { this.RaiseAndSetIfChanged(ref this.maxReferenced, value); }
        }

        /// <summary>
        /// Gets or sets the ReferencingCategory
        /// </summary>
        public Category ReferencingCategory
        {
            get { return this.referencingCategory; }
            set { this.RaiseAndSetIfChanged(ref this.referencingCategory, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ReferencingCategory"/>
        /// </summary>
        public string ReferencingCategoryShortName
        {
            get { return this.referencingCategoryShortName; }
            set { this.RaiseAndSetIfChanged(ref this.referencingCategoryShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ReferencingCategory"/>
        /// </summary>
        public string ReferencingCategoryName
        {
            get { return this.referencingCategoryName; }
            set { this.RaiseAndSetIfChanged(ref this.referencingCategoryName, value); }
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
            this.MinReferenced = this.Thing.MinReferenced;
            this.MaxReferenced = this.Thing.MaxReferenced;
            this.ReferencingCategory = this.Thing.ReferencingCategory;
        }
    }
}
