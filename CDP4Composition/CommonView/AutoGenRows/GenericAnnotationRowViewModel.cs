﻿// -------------------------------------------------------------------------------------------------
// <copyright file="GenericAnnotationRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="GenericAnnotation"/>
    /// </summary>
    public abstract partial class GenericAnnotationRowViewModel<T> : RowViewModelBase<T> where T : GenericAnnotation
    {

        /// <summary>
        /// Backing field for <see cref="LanguageCode"/>
        /// </summary>
        private string languageCode;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/>
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericAnnotationRowViewModel{T}"/> class
        /// </summary>
        /// <param name="genericAnnotation">The <see cref="GenericAnnotation"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected GenericAnnotationRowViewModel(T genericAnnotation, ISession session, IViewModelBase<Thing> containerViewModel) : base(genericAnnotation, session, containerViewModel)
        {
            this.UpdateProperties();
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
        /// Gets or sets the CreatedOn
        /// </summary>
        public DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
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
            this.LanguageCode = this.Thing.LanguageCode;
            this.Content = this.Thing.Content;
            this.CreatedOn = this.Thing.CreatedOn;
        }
    }
}
