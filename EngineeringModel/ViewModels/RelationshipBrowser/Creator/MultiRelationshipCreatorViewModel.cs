// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipCreatorViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    public class MultiRelationshipCreatorViewModel : ReactiveObject, IRelationshipCreatorViewModel, IDropTarget
    {
        /// <summary>
        /// Backing field for <see cref="AppliedCategories"/>
        /// </summary>
        private List<Category> appliedCategories;

        /// <summary>
        /// Backing field for <see cref="CanCreate"/>
        /// </summary>
        private bool canCreate;

        /// <summary>
        /// Backing field for <see cref="SelectedRelatedThing"/>
        /// </summary>
        private RelatedThingRowViewModel selectedRelatedThing;

        /// <summary>
        /// The <see cref="Iteration"/>
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The collection of <see cref="IDisposable"/>
        /// </summary>
        private readonly List<IDisposable> Subscriptions = new List<IDisposable>();

        /// <summary>
        /// The current <see cref="ModelReferenceDataLibrary"/>
        /// </summary>
        private ModelReferenceDataLibrary requiredRdl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipCreatorViewModel"/> class
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration"/></param>
        public MultiRelationshipCreatorViewModel(Iteration iteration)
        {
            this.iteration = iteration;
            this.PossibleCategories = new ReactiveList<Category>();
            this.RelatedThings = new ReactiveList<RelatedThingRowViewModel>();
            this.RelatedThings.ChangeTrackingEnabled = true;
            this.PossibleCategories = new ReactiveList<Category>();

            this.Subscriptions.Add(this.RelatedThings.CountChanged.Subscribe(x => this.CanCreate = this.RelatedThings.Count != 0));

            this.InitializeRequiredRdlSubscription();
            this.PopulateCategories();
        }

        /// <summary>
        /// Gets or sets the selected <see cref="MultiRelationshipRelatedThingRowViewModel"/>
        /// </summary>
        public RelatedThingRowViewModel SelectedRelatedThing
        {
            get { return this.selectedRelatedThing; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRelatedThing, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="Category"/> for the <see cref="MultiRelationship"/> to create
        /// </summary>
        public ReactiveList<Category> PossibleCategories { get; private set; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="MultiRelationship"/> can be created
        /// </summary>
        public bool CanCreate
        {
            get { return this.canCreate; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreate, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="Category"/>s for the <see cref="BinaryRelationship"/> to create
        /// </summary>
        public List<Category> AppliedCategories
        {
            get { return this.appliedCategories; }
            set { this.RaiseAndSetIfChanged(ref this.appliedCategories, value); }
        }

        /// <summary>
        /// Gets the rows that represents the <see cref="MultiRelationship.RelatedThing"/>
        /// </summary>
        public ReactiveList<RelatedThingRowViewModel> RelatedThings { get; private set; }

        /// <summary>
        /// Gets the type of the <see cref="Relationship"/> to create
        /// </summary>
        public string CreatorKind
        {
            get { return "Multi Relationship"; }
        }

        /// <summary>
        /// Re-initializes the view-model
        /// </summary>
        public void ReInitializeControl()
        {
            this.RelatedThings.Clear();
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///   Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Data"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            var thing = dropInfo.Payload as Thing;
            if (thing != null && !this.RelatedThings.Select(x => x.Thing).Contains(thing) && thing.TopContainer is EngineeringModel)
            {
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        /// <summary>
        /// Performs a drop.
        /// </summary>
        /// <param name="dropInfo">
        ///   Information about the drop.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var thing = dropInfo.Payload as Thing;
            if (thing == null || this.RelatedThings.Select(x => x.Thing).Contains(thing))
            {
                return;
            }

            var row = new RelatedThingRowViewModel(thing, this.RemoveRelatedThing);
            this.RelatedThings.Add(row);
        }

        /// <summary>
        /// Creates a <see cref="Relationship"/> 
        /// </summary>
        /// <returns>A new instance of <see cref="Relationship"/></returns>
        public Relationship CreateRelationshipObject()
        {
            var relationship = new MultiRelationship();
            relationship.RelatedThing.AddRange(this.RelatedThings.Select(x => x.Thing));
            relationship.Category.AddRange(this.AppliedCategories);

            return relationship;
        }

        /// <summary>
        /// Dispose of this view-model
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in this.Subscriptions)
            {
                subscription.Dispose();
            }

            foreach (var multiRelationshipRelatedThingRowViewModel in this.RelatedThings)
            {
                multiRelationshipRelatedThingRowViewModel.Dispose();
            }

            foreach (var categoryItemViewModel in this.PossibleCategories)
            {
                categoryItemViewModel.Dispose();
            }
        }

        /// <summary>
        /// Initializes the <see cref="requiredRdl"/> field
        /// </summary>
        private void InitializeRequiredRdlSubscription()
        {
            var modelsetup = (EngineeringModelSetup)this.iteration.IterationSetup.Container;
            this.requiredRdl = modelsetup.RequiredRdl.Single();

            var rdl = (ReferenceDataLibrary)this.requiredRdl;
            while (rdl != null)
            {
                var subscriber = CDPMessageBus.Current.Listen<ObjectChangedEvent>(rdl)
                    .Where(msg => msg.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.PopulateCategories());
                this.Subscriptions.Add(subscriber);
                rdl = rdl.RequiredRdl;
            }
        }

        /// <summary>
        /// Populates the possible <see cref="Category"/>
        /// </summary>
        private void PopulateCategories()
        {
            this.PossibleCategories.Clear();
            this.PossibleCategories.AddRange(this.requiredRdl.AggregatedReferenceDataLibrary.SelectMany(x => x.DefinedCategory).Where(x => x.PermissibleClass.Contains(ClassKind.MultiRelationship)).OrderBy(x => x.Name));
        }


        /// <summary>
        /// Removes the <paramref name="relatedThingRow"/> from the <see cref="RelatedThings"/> property
        /// </summary>
        /// <param name="relatedThingRow">The <see cref="RelatedThingRowViewModel"/> to remove</param>
        private void RemoveRelatedThing(RelatedThingRowViewModel relatedThingRow)
        {
            if (this.RelatedThings.Remove(relatedThingRow))
            {
                relatedThingRow.Dispose();
            }
        }
    }
}