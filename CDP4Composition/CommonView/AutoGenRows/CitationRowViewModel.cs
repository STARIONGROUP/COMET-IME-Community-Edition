// -------------------------------------------------------------------------------------------------
// <copyright file="CitationRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="Citation"/>
    /// </summary>
    public partial class CitationRowViewModel : RowViewModelBase<Citation>
    {

        /// <summary>
        /// Backing field for <see cref="Location"/>
        /// </summary>
        private string location;

        /// <summary>
        /// Backing field for <see cref="IsAdaptation"/>
        /// </summary>
        private bool isAdaptation;

        /// <summary>
        /// Backing field for <see cref="Remark"/>
        /// </summary>
        private string remark;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Source"/>
        /// </summary>
        private ReferenceSource source;

        /// <summary>
        /// Backing field for <see cref="SourceShortName"/>
        /// </summary>
        private string sourceShortName;

        /// <summary>
        /// Backing field for <see cref="SourceName"/>
        /// </summary>
        private string sourceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CitationRowViewModel"/> class
        /// </summary>
        /// <param name="citation">The <see cref="Citation"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public CitationRowViewModel(Citation citation, ISession session, IViewModelBase<Thing> containerViewModel) : base(citation, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Location
        /// </summary>
        public string Location
        {
            get { return this.location; }
            set { this.RaiseAndSetIfChanged(ref this.location, value); }
        }

        /// <summary>
        /// Gets or sets the IsAdaptation
        /// </summary>
        public bool IsAdaptation
        {
            get { return this.isAdaptation; }
            set { this.RaiseAndSetIfChanged(ref this.isAdaptation, value); }
        }

        /// <summary>
        /// Gets or sets the Remark
        /// </summary>
        public string Remark
        {
            get { return this.remark; }
            set { this.RaiseAndSetIfChanged(ref this.remark, value); }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        public string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the Source
        /// </summary>
        public ReferenceSource Source
        {
            get { return this.source; }
            set { this.RaiseAndSetIfChanged(ref this.source, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Source"/>
        /// </summary>
        public string SourceShortName
        {
            get { return this.sourceShortName; }
            set { this.RaiseAndSetIfChanged(ref this.sourceShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Source"/>
        /// </summary>
        public string SourceName
        {
            get { return this.sourceName; }
            set { this.RaiseAndSetIfChanged(ref this.sourceName, value); }
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
            this.Location = this.Thing.Location;
            this.IsAdaptation = this.Thing.IsAdaptation;
            this.Remark = this.Thing.Remark;
            this.ShortName = this.Thing.ShortName;
			if (this.Thing.Source != null)
			{
				this.SourceShortName = this.Thing.Source.ShortName;
				this.SourceName = this.Thing.Source.Name;
			}			
            this.Source = this.Thing.Source;
        }
    }
}
