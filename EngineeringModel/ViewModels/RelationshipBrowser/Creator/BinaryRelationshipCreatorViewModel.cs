// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipCreatorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The view-model that gives the capability to create a <see cref="BinaryRelationship" /> through means of drag and drop
    /// </summary>
    public class BinaryRelationshipCreatorViewModel : ReactiveObject, IRelationshipCreatorViewModel
    {
        /// <summary>
        /// Logger instance used to log using ILog Facade
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Iteration" />
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The collection of subscriptions to dispose of
        /// </summary>
        private readonly List<IDisposable> Subscriptions = new List<IDisposable>();

        /// <summary>
        /// Backing field for <see cref="AppliedCategories" />
        /// </summary>
        private List<Category> appliedCategories = new List<Category>();

        /// <summary>
        /// Backing field for <see cref="CanCreate" />
        /// </summary>
        private bool canCreate;

        /// <summary>
        /// Backing field for <see cref="Name" />
        /// </summary>
        private string name;

        /// <summary>
        /// The current <see cref="ModelReferenceDataLibrary" />
        /// </summary>
        private ModelReferenceDataLibrary requiredRdl;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipCreatorViewModel" /> class
        /// </summary>
        /// <param name="iteration">The current <see cref="Iteration" /></param>
        /// <param name="session">The <see cref="ISession" /> that was used to retrieve data from the datastore</param>
        public BinaryRelationshipCreatorViewModel(Iteration iteration, ISession session)
        {
            this.Session = session;
            this.iteration = iteration;
            this.PossibleCategories = new ReactiveList<Category>();
            this.SourceViewModel = new RelatedThingViewModel(session.CDPMessageBus);
            this.TargetViewModel = new RelatedThingViewModel(session.CDPMessageBus);
            var relatedThingChangedSubscriber = this.WhenAnyValue(x => x.TargetViewModel.RelatedThing, y => y.SourceViewModel.RelatedThing).Subscribe(x => this.CanCreate = this.SourceViewModel.RelatedThing != null && this.TargetViewModel.RelatedThing != null);
            this.Subscriptions.Add(relatedThingChangedSubscriber);

            this.InitializeRequiredRdlSubscription();

            this.PopulateCategories();
        }

        /// <summary>
        /// The view-model for the user control to select the source of a <see cref="BinaryRelationship" /> to create
        /// </summary>
        public RelatedThingViewModel SourceViewModel { get; private set; }

        /// <summary>
        /// The view-model for the user control to select the target of a <see cref="BinaryRelationship" /> to create
        /// </summary>
        public RelatedThingViewModel TargetViewModel { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="Category" /> for the <see cref="BinaryRelationship" /> to create
        /// </summary>
        public ReactiveList<Category> PossibleCategories { get; private set; }

        /// <summary>
        /// Gets the <see cref="ISession" />
        /// </summary>
        public ISession Session { get; }

        /// <summary>
        /// Gets or sets the Name for the <see cref="BinaryRelationship" /> to create
        /// </summary>
        [CDPVersion("1.2.0")]
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Category" />s for the <see cref="BinaryRelationship" /> to create
        /// </summary>
        public List<Category> AppliedCategories
        {
            get => this.appliedCategories;
            set => this.RaiseAndSetIfChanged(ref this.appliedCategories, value);
        }

        /// <summary>
        /// Gets a value indicating whether a <see cref="BinaryRelationship" /> can be created
        /// </summary>
        public bool CanCreate
        {
            get => this.canCreate;
            private set => this.RaiseAndSetIfChanged(ref this.canCreate, value);
        }

        /// <summary>
        /// Gets the type of the <see cref="Relationship" /> to create
        /// </summary>
        public string CreatorKind => "Binary Relationship";

        /// <summary>
        /// Re-initializes the view-model
        /// </summary>
        public void ReInitializeControl()
        {
            this.Name = string.Empty;
            this.SourceViewModel.ResetControl();
            this.TargetViewModel.ResetControl();
            this.appliedCategories.Clear();
        }

        /// <summary>
        /// Creates a <see cref="Relationship" />
        /// </summary>
        /// <returns>A new instance of <see cref="Relationship" /></returns>
        public Relationship CreateRelationshipObject()
        {
            var relationship = new BinaryRelationship
            {
                Name = this.Name,
                Source = this.SourceViewModel.RelatedThing,
                Target = this.TargetViewModel.RelatedThing
            };

            relationship.Category.AddRange(this.AppliedCategories);

            return relationship;
        }

        /// <summary>
        /// Disposes of the view-model
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in this.Subscriptions)
            {
                subscription.Dispose();
            }

            foreach (var categoryItemViewModel in this.PossibleCategories)
            {
                categoryItemViewModel.Dispose();
            }

            this.SourceViewModel.Dispose();
            this.TargetViewModel.Dispose();
        }

        /// <summary>
        /// Initializes the <see cref="requiredRdl" /> field
        /// </summary>
        private void InitializeRequiredRdlSubscription()
        {
            var modelsetup = (EngineeringModelSetup)this.iteration.IterationSetup.Container;
            this.requiredRdl = modelsetup.RequiredRdl.Single();

            var rdl = (ReferenceDataLibrary)this.requiredRdl;

            while (rdl != null)
            {
                var subscriber = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(rdl)
                    .Where(msg => msg.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.PopulateCategories());

                this.Subscriptions.Add(subscriber);
                rdl = rdl.RequiredRdl;
            }
        }

        /// <summary>
        /// Populates the possible <see cref="Category" />
        /// </summary>
        private void PopulateCategories()
        {
            this.PossibleCategories.Clear();
            this.PossibleCategories.AddRange(this.requiredRdl.AggregatedReferenceDataLibrary.SelectMany(x => x.DefinedCategory).Where(x => x.PermissibleClass.Contains(ClassKind.BinaryRelationship)).OrderBy(x => x.Name));
        }
    }
}
