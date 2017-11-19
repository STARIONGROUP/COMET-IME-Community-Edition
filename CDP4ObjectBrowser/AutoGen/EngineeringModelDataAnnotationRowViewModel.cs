// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelDataAnnotationRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="EngineeringModelDataAnnotation"/>
    /// </summary>
    public abstract partial class EngineeringModelDataAnnotationRowViewModel<T> : GenericAnnotationRowViewModel<T>, IEngineeringModelDataAnnotationRowViewModel<T> where T :EngineeringModelDataAnnotation
    {
        /// <summary>
        /// Intermediate folder containing <see cref="RelatedThingRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel relatedThingFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="DiscussionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel discussionFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelDataAnnotationRowViewModel{T}"/> class
        /// </summary>
        /// <param name="engineeringModelDataAnnotation">The <see cref="EngineeringModelDataAnnotation"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected EngineeringModelDataAnnotationRowViewModel(T engineeringModelDataAnnotation, ISession session, IViewModelBase<Thing> containerViewModel) : base(engineeringModelDataAnnotation, session, containerViewModel)
        {
            this.relatedThingFolder = new CDP4Composition.FolderRowViewModel("Related Thing", "Related Thing", this.Session, this);
            this.ContainedRows.Add(this.relatedThingFolder);
            this.discussionFolder = new CDP4Composition.FolderRowViewModel("Discussion", "Discussion", this.Session, this);
            this.ContainedRows.Add(this.discussionFolder);
            this.UpdateProperties();
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
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.RelatedThing, this.relatedThingFolder, this.AddRelatedThingRowViewModel);
            this.ComputeRows(this.Thing.Discussion, this.discussionFolder, this.AddDiscussionRowViewModel);
        }
        /// <summary>
        /// Add an Related Thing row view model to the list of <see cref="RelatedThing"/>
        /// </summary>
        /// <param name="relatedThing">
        /// The <see cref="RelatedThing"/> that is to be added
        /// </param>
        private ModellingThingReferenceRowViewModel AddRelatedThingRowViewModel(ModellingThingReference relatedThing)
        {
            return new ModellingThingReferenceRowViewModel(relatedThing, this.Session, this);
        }
        /// <summary>
        /// Add an Discussion row view model to the list of <see cref="Discussion"/>
        /// </summary>
        /// <param name="discussion">
        /// The <see cref="Discussion"/> that is to be added
        /// </param>
        private EngineeringModelDataDiscussionItemRowViewModel AddDiscussionRowViewModel(EngineeringModelDataDiscussionItem discussion)
        {
            return new EngineeringModelDataDiscussionItemRowViewModel(discussion, this.Session, this);
        }
    }
}
