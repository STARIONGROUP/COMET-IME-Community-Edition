// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipRuleRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="BinaryRelationshipRule"/>
    /// </summary>
    public partial class BinaryRelationshipRuleRowViewModel : RuleRowViewModel<BinaryRelationshipRule>
    {

        /// <summary>
        /// Backing field for <see cref="ForwardRelationshipName"/>
        /// </summary>
        private string forwardRelationshipName;

        /// <summary>
        /// Backing field for <see cref="InverseRelationshipName"/>
        /// </summary>
        private string inverseRelationshipName;

        /// <summary>
        /// Backing field for <see cref="RelationshipCategory"/>
        /// </summary>
        private Category relationshipCategory;

        /// <summary>
        /// Backing field for <see cref="RelationshipCategoryShortName"/>
        /// </summary>
        private string relationshipCategoryShortName;

        /// <summary>
        /// Backing field for <see cref="RelationshipCategoryName"/>
        /// </summary>
        private string relationshipCategoryName;

        /// <summary>
        /// Backing field for <see cref="SourceCategory"/>
        /// </summary>
        private Category sourceCategory;

        /// <summary>
        /// Backing field for <see cref="SourceCategoryShortName"/>
        /// </summary>
        private string sourceCategoryShortName;

        /// <summary>
        /// Backing field for <see cref="SourceCategoryName"/>
        /// </summary>
        private string sourceCategoryName;

        /// <summary>
        /// Backing field for <see cref="TargetCategory"/>
        /// </summary>
        private Category targetCategory;

        /// <summary>
        /// Backing field for <see cref="TargetCategoryShortName"/>
        /// </summary>
        private string targetCategoryShortName;

        /// <summary>
        /// Backing field for <see cref="TargetCategoryName"/>
        /// </summary>
        private string targetCategoryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipRuleRowViewModel"/> class
        /// </summary>
        /// <param name="binaryRelationshipRule">The <see cref="BinaryRelationshipRule"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public BinaryRelationshipRuleRowViewModel(BinaryRelationshipRule binaryRelationshipRule, ISession session, IViewModelBase<Thing> containerViewModel) : base(binaryRelationshipRule, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the ForwardRelationshipName
        /// </summary>
        public string ForwardRelationshipName
        {
            get { return this.forwardRelationshipName; }
            set { this.RaiseAndSetIfChanged(ref this.forwardRelationshipName, value); }
        }

        /// <summary>
        /// Gets or sets the InverseRelationshipName
        /// </summary>
        public string InverseRelationshipName
        {
            get { return this.inverseRelationshipName; }
            set { this.RaiseAndSetIfChanged(ref this.inverseRelationshipName, value); }
        }

        /// <summary>
        /// Gets or sets the RelationshipCategory
        /// </summary>
        public Category RelationshipCategory
        {
            get { return this.relationshipCategory; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipCategory, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="RelationshipCategory"/>
        /// </summary>
        public string RelationshipCategoryShortName
        {
            get { return this.relationshipCategoryShortName; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipCategoryShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="RelationshipCategory"/>
        /// </summary>
        public string RelationshipCategoryName
        {
            get { return this.relationshipCategoryName; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipCategoryName, value); }
        }

        /// <summary>
        /// Gets or sets the SourceCategory
        /// </summary>
        public Category SourceCategory
        {
            get { return this.sourceCategory; }
            set { this.RaiseAndSetIfChanged(ref this.sourceCategory, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="SourceCategory"/>
        /// </summary>
        public string SourceCategoryShortName
        {
            get { return this.sourceCategoryShortName; }
            set { this.RaiseAndSetIfChanged(ref this.sourceCategoryShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="SourceCategory"/>
        /// </summary>
        public string SourceCategoryName
        {
            get { return this.sourceCategoryName; }
            set { this.RaiseAndSetIfChanged(ref this.sourceCategoryName, value); }
        }

        /// <summary>
        /// Gets or sets the TargetCategory
        /// </summary>
        public Category TargetCategory
        {
            get { return this.targetCategory; }
            set { this.RaiseAndSetIfChanged(ref this.targetCategory, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="TargetCategory"/>
        /// </summary>
        public string TargetCategoryShortName
        {
            get { return this.targetCategoryShortName; }
            set { this.RaiseAndSetIfChanged(ref this.targetCategoryShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="TargetCategory"/>
        /// </summary>
        public string TargetCategoryName
        {
            get { return this.targetCategoryName; }
            set { this.RaiseAndSetIfChanged(ref this.targetCategoryName, value); }
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
            this.ForwardRelationshipName = this.Thing.ForwardRelationshipName;
            this.InverseRelationshipName = this.Thing.InverseRelationshipName;
            this.RelationshipCategory = this.Thing.RelationshipCategory;
            this.SourceCategory = this.Thing.SourceCategory;
            this.TargetCategory = this.Thing.TargetCategory;
        }
    }
}
