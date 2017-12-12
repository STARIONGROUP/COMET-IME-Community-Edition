// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterizedCategoryRuleRowViewModel.cs" company="RHEA S.A.">
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
    /// Row class representing a <see cref="ParameterizedCategoryRule"/>
    /// </summary>
    public partial class ParameterizedCategoryRuleRowViewModel : RuleRowViewModel<ParameterizedCategoryRule>
    {

        /// <summary>
        /// Backing field for <see cref="Category"/>
        /// </summary>
        private Category category;

        /// <summary>
        /// Backing field for <see cref="CategoryShortName"/>
        /// </summary>
        private string categoryShortName;

        /// <summary>
        /// Backing field for <see cref="CategoryName"/>
        /// </summary>
        private string categoryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedCategoryRuleRowViewModel"/> class
        /// </summary>
        /// <param name="parameterizedCategoryRule">The <see cref="ParameterizedCategoryRule"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ParameterizedCategoryRuleRowViewModel(ParameterizedCategoryRule parameterizedCategoryRule, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterizedCategoryRule, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Category
        /// </summary>
        public Category Category
        {
            get { return this.category; }
            set { this.RaiseAndSetIfChanged(ref this.category, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Category"/>
        /// </summary>
        public string CategoryShortName
        {
            get { return this.categoryShortName; }
            set { this.RaiseAndSetIfChanged(ref this.categoryShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Category"/>
        /// </summary>
        public string CategoryName
        {
            get { return this.categoryName; }
            set { this.RaiseAndSetIfChanged(ref this.categoryName, value); }
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
            this.Category = this.Thing.Category;
        }
    }
}
