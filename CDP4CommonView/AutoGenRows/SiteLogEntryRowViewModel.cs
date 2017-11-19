// -------------------------------------------------------------------------------------------------
// <copyright file="SiteLogEntryRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="SiteLogEntry"/>
    /// </summary>
    public partial class SiteLogEntryRowViewModel : RowViewModelBase<SiteLogEntry>
    {

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/>
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="LanguageCode"/>
        /// </summary>
        private string languageCode;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Backing field for <see cref="Level"/>
        /// </summary>
        private LogLevelKind level;

        /// <summary>
        /// Backing field for <see cref="Author"/>
        /// </summary>
        private Person author;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteLogEntryRowViewModel"/> class
        /// </summary>
        /// <param name="siteLogEntry">The <see cref="SiteLogEntry"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public SiteLogEntryRowViewModel(SiteLogEntry siteLogEntry, ISession session, IViewModelBase<Thing> containerViewModel) : base(siteLogEntry, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets or sets the LanguageCode
        /// </summary>
        public string LanguageCode
        {
            get { return this.languageCode; }
            set { this.RaiseAndSetIfChanged(ref this.languageCode, value); }
        }

        /// <summary>
        /// Gets or sets the Content
        /// </summary>
        public string Content
        {
            get { return this.content; }
            set { this.RaiseAndSetIfChanged(ref this.content, value); }
        }

        /// <summary>
        /// Gets or sets the Level
        /// </summary>
        public LogLevelKind Level
        {
            get { return this.level; }
            set { this.RaiseAndSetIfChanged(ref this.level, value); }
        }

        /// <summary>
        /// Gets or sets the Author
        /// </summary>
        public Person Author
        {
            get { return this.author; }
            set { this.RaiseAndSetIfChanged(ref this.author, value); }
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
            this.CreatedOn = this.Thing.CreatedOn;
            this.LanguageCode = this.Thing.LanguageCode;
            this.Content = this.Thing.Content;
            this.Level = this.Thing.Level;
            this.Author = this.Thing.Author;
        }
    }
}
