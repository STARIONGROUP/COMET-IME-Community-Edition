﻿// -------------------------------------------------------------------------------------------------
// <copyright file="IterationRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="Iteration"/>
    /// </summary>
    public partial class IterationRowViewModel : RowViewModelBase<Iteration>
    {

        /// <summary>
        /// Backing field for <see cref="SourceIterationIid"/>
        /// </summary>
        private Guid sourceIterationIid;

        /// <summary>
        /// Backing field for <see cref="IterationSetup"/>
        /// </summary>
        private IterationSetup iterationSetup;

        /// <summary>
        /// Backing field for <see cref="TopElement"/>
        /// </summary>
        private ElementDefinition topElement;

        /// <summary>
        /// Backing field for <see cref="TopElementShortName"/>
        /// </summary>
        private string topElementShortName;

        /// <summary>
        /// Backing field for <see cref="TopElementName"/>
        /// </summary>
        private string topElementName;

        /// <summary>
        /// Backing field for <see cref="DefaultOption"/>
        /// </summary>
        private Option defaultOption;

        /// <summary>
        /// Backing field for <see cref="DefaultOptionShortName"/>
        /// </summary>
        private string defaultOptionShortName;

        /// <summary>
        /// Backing field for <see cref="DefaultOptionName"/>
        /// </summary>
        private string defaultOptionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationRowViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public IterationRowViewModel(Iteration iteration, ISession session, IViewModelBase<Thing> containerViewModel) : base(iteration, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the SourceIterationIid
        /// </summary>
        public Guid SourceIterationIid
        {
            get { return this.sourceIterationIid; }
            set { this.RaiseAndSetIfChanged(ref this.sourceIterationIid, value); }
        }

        /// <summary>
        /// Gets or sets the IterationSetup
        /// </summary>
        public IterationSetup IterationSetup
        {
            get { return this.iterationSetup; }
            set { this.RaiseAndSetIfChanged(ref this.iterationSetup, value); }
        }

        /// <summary>
        /// Gets or sets the TopElement
        /// </summary>
        public ElementDefinition TopElement
        {
            get { return this.topElement; }
            set { this.RaiseAndSetIfChanged(ref this.topElement, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="TopElement"/>
        /// </summary>
        public string TopElementShortName
        {
            get { return this.topElementShortName; }
            set { this.RaiseAndSetIfChanged(ref this.topElementShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="TopElement"/>
        /// </summary>
        public string TopElementName
        {
            get { return this.topElementName; }
            set { this.RaiseAndSetIfChanged(ref this.topElementName, value); }
        }

        /// <summary>
        /// Gets or sets the DefaultOption
        /// </summary>
        public Option DefaultOption
        {
            get { return this.defaultOption; }
            set { this.RaiseAndSetIfChanged(ref this.defaultOption, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DefaultOption"/>
        /// </summary>
        public string DefaultOptionShortName
        {
            get { return this.defaultOptionShortName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultOptionShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DefaultOption"/>
        /// </summary>
        public string DefaultOptionName
        {
            get { return this.defaultOptionName; }
            set { this.RaiseAndSetIfChanged(ref this.defaultOptionName, value); }
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
            if(this.Thing.SourceIterationIid.HasValue)
            {
                this.SourceIterationIid = this.Thing.SourceIterationIid.Value;
            }
            this.IterationSetup = this.Thing.IterationSetup;
			if (this.Thing.TopElement != null)
			{
				this.TopElementShortName = this.Thing.TopElement.ShortName;
				this.TopElementName = this.Thing.TopElement.Name;
			}			
            this.TopElement = this.Thing.TopElement;
			if (this.Thing.DefaultOption != null)
			{
				this.DefaultOptionShortName = this.Thing.DefaultOption.ShortName;
				this.DefaultOptionName = this.Thing.DefaultOption.Name;
			}			
            this.DefaultOption = this.Thing.DefaultOption;
        }
    }
}
