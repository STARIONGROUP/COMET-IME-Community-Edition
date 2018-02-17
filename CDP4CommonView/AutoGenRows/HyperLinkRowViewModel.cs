// -------------------------------------------------------------------------------------------------
// <copyright file="HyperLinkRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="HyperLink"/>
    /// </summary>
    public partial class HyperLinkRowViewModel : RowViewModelBase<HyperLink>
    {

        /// <summary>
        /// Backing field for <see cref="Uri"/>
        /// </summary>
        private string uri;

        /// <summary>
        /// Backing field for <see cref="LanguageCode"/>
        /// </summary>
        private string languageCode;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyperLinkRowViewModel"/> class
        /// </summary>
        /// <param name="hyperLink">The <see cref="HyperLink"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public HyperLinkRowViewModel(HyperLink hyperLink, ISession session, IViewModelBase<Thing> containerViewModel) : base(hyperLink, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Uri
        /// </summary>
        public string Uri
        {
            get { return this.uri; }
            set { this.RaiseAndSetIfChanged(ref this.uri, value); }
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
            this.Uri = this.Thing.Uri;
            this.LanguageCode = this.Thing.LanguageCode;
            this.Content = this.Thing.Content;
        }
    }
}
