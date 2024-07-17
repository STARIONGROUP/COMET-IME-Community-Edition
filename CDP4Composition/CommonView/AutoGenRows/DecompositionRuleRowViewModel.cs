// -------------------------------------------------------------------------------------------------
// <copyright file="DecompositionRuleRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
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
    /// Row class representing a <see cref="DecompositionRule"/>
    /// </summary>
    public partial class DecompositionRuleRowViewModel : RuleRowViewModel<DecompositionRule>
    {

        /// <summary>
        /// Backing field for <see cref="MinContained"/>
        /// </summary>
        private int minContained;

        /// <summary>
        /// Backing field for <see cref="MaxContained"/>
        /// </summary>
        private int maxContained;

        /// <summary>
        /// Backing field for <see cref="ContainingCategory"/>
        /// </summary>
        private Category containingCategory;

        /// <summary>
        /// Backing field for <see cref="ContainingCategoryShortName"/>
        /// </summary>
        private string containingCategoryShortName;

        /// <summary>
        /// Backing field for <see cref="ContainingCategoryName"/>
        /// </summary>
        private string containingCategoryName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecompositionRuleRowViewModel"/> class
        /// </summary>
        /// <param name="decompositionRule">The <see cref="DecompositionRule"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public DecompositionRuleRowViewModel(DecompositionRule decompositionRule, ISession session, IViewModelBase<Thing> containerViewModel) : base(decompositionRule, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the MinContained
        /// </summary>
        public int MinContained
        {
            get { return this.minContained; }
            set { this.RaiseAndSetIfChanged(ref this.minContained, value); }
        }

        /// <summary>
        /// Gets or sets the MaxContained
        /// </summary>
        public int MaxContained
        {
            get { return this.maxContained; }
            set { this.RaiseAndSetIfChanged(ref this.maxContained, value); }
        }

        /// <summary>
        /// Gets or sets the ContainingCategory
        /// </summary>
        public Category ContainingCategory
        {
            get { return this.containingCategory; }
            set { this.RaiseAndSetIfChanged(ref this.containingCategory, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ContainingCategory"/>
        /// </summary>
        public string ContainingCategoryShortName
        {
            get { return this.containingCategoryShortName; }
            set { this.RaiseAndSetIfChanged(ref this.containingCategoryShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ContainingCategory"/>
        /// </summary>
        public string ContainingCategoryName
        {
            get { return this.containingCategoryName; }
            set { this.RaiseAndSetIfChanged(ref this.containingCategoryName, value); }
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
            this.MinContained = this.Thing.MinContained;
            if(this.Thing.MaxContained.HasValue)
            {
                this.MaxContained = this.Thing.MaxContained.Value;
            }
			if (this.Thing.ContainingCategory != null)
			{
				this.ContainingCategoryShortName = this.Thing.ContainingCategory.ShortName;
				this.ContainingCategoryName = this.Thing.ContainingCategory.Name;
			}			
            this.ContainingCategory = this.Thing.ContainingCategory;
        }
    }
}
