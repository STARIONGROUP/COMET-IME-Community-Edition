// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
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
    /// Row class representing a <see cref="ReferenceSource"/>
    /// </summary>
    public partial class ReferenceSourceRowViewModel : DefinedThingRowViewModel<ReferenceSource>
    {

        /// <summary>
        /// Backing field for <see cref="VersionIdentifier"/>
        /// </summary>
        private string versionIdentifier;

        /// <summary>
        /// Backing field for <see cref="VersionDate"/>
        /// </summary>
        private DateTime versionDate;

        /// <summary>
        /// Backing field for <see cref="Author"/>
        /// </summary>
        private string author;

        /// <summary>
        /// Backing field for <see cref="PublicationYear"/>
        /// </summary>
        private int publicationYear;

        /// <summary>
        /// Backing field for <see cref="Language"/>
        /// </summary>
        private string language;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="Publisher"/>
        /// </summary>
        private Organization publisher;

        /// <summary>
        /// Backing field for <see cref="PublishedIn"/>
        /// </summary>
        private ReferenceSource publishedIn;

        /// <summary>
        /// Backing field for <see cref="PublishedInShortName"/>
        /// </summary>
        private string publishedInShortName;

        /// <summary>
        /// Backing field for <see cref="PublishedInName"/>
        /// </summary>
        private string publishedInName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSourceRowViewModel"/> class
        /// </summary>
        /// <param name="referenceSource">The <see cref="ReferenceSource"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ReferenceSourceRowViewModel(ReferenceSource referenceSource, ISession session, IViewModelBase<Thing> containerViewModel) : base(referenceSource, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the VersionIdentifier
        /// </summary>
        public string VersionIdentifier
        {
            get { return this.versionIdentifier; }
            set { this.RaiseAndSetIfChanged(ref this.versionIdentifier, value); }
        }

        /// <summary>
        /// Gets or sets the VersionDate
        /// </summary>
        public DateTime VersionDate
        {
            get { return this.versionDate; }
            set { this.RaiseAndSetIfChanged(ref this.versionDate, value); }
        }

        /// <summary>
        /// Gets or sets the Author
        /// </summary>
        public string Author
        {
            get { return this.author; }
            set { this.RaiseAndSetIfChanged(ref this.author, value); }
        }

        /// <summary>
        /// Gets or sets the PublicationYear
        /// </summary>
        public int PublicationYear
        {
            get { return this.publicationYear; }
            set { this.RaiseAndSetIfChanged(ref this.publicationYear, value); }
        }

        /// <summary>
        /// Gets or sets the Language
        /// </summary>
        public string Language
        {
            get { return this.language; }
            set { this.RaiseAndSetIfChanged(ref this.language, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets or sets the Publisher
        /// </summary>
        public Organization Publisher
        {
            get { return this.publisher; }
            set { this.RaiseAndSetIfChanged(ref this.publisher, value); }
        }

        /// <summary>
        /// Gets or sets the PublishedIn
        /// </summary>
        public ReferenceSource PublishedIn
        {
            get { return this.publishedIn; }
            set { this.RaiseAndSetIfChanged(ref this.publishedIn, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="PublishedIn"/>
        /// </summary>
        public string PublishedInShortName
        {
            get { return this.publishedInShortName; }
            set { this.RaiseAndSetIfChanged(ref this.publishedInShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="PublishedIn"/>
        /// </summary>
        public string PublishedInName
        {
            get { return this.publishedInName; }
            set { this.RaiseAndSetIfChanged(ref this.publishedInName, value); }
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
            this.VersionIdentifier = this.Thing.VersionIdentifier;
            if(this.Thing.VersionDate.HasValue)
            {
                this.VersionDate = this.Thing.VersionDate.Value;
            }
            this.Author = this.Thing.Author;
            if(this.Thing.PublicationYear.HasValue)
            {
                this.PublicationYear = this.Thing.PublicationYear.Value;
            }
            this.Language = this.Thing.Language;
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.Publisher = this.Thing.Publisher;
			if (this.Thing.PublishedIn != null)
			{
				this.PublishedInShortName = this.Thing.PublishedIn.ShortName;
				this.PublishedInName = this.Thing.PublishedIn.Name;
			}			
            this.PublishedIn = this.Thing.PublishedIn;
        }
    }
}
