// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ReferenceSource"/>
    /// </summary>
    public partial class ReferenceSourceDialogViewModel : DefinedThingDialogViewModel<ReferenceSource>
    {
        /// <summary>
        /// Backing field for <see cref="VersionIdentifier"/>
        /// </summary>
        private string versionIdentifier;

        /// <summary>
        /// Backing field for <see cref="VersionDate"/>
        /// </summary>
        private DateTime? versionDate;

        /// <summary>
        /// Backing field for <see cref="Author"/>
        /// </summary>
        private string author;

        /// <summary>
        /// Backing field for <see cref="PublicationYear"/>
        /// </summary>
        private int? publicationYear;

        /// <summary>
        /// Backing field for <see cref="Language"/>
        /// </summary>
        private string language;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="SelectedPublisher"/>
        /// </summary>
        private Organization selectedPublisher;

        /// <summary>
        /// Backing field for <see cref="SelectedPublishedIn"/>
        /// </summary>
        private ReferenceSource selectedPublishedIn;


        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSourceDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ReferenceSourceDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSourceDialogViewModel"/> class
        /// </summary>
        /// <param name="referenceSource">
        /// The <see cref="ReferenceSource"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ReferenceSourceDialogViewModel(ReferenceSource referenceSource, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(referenceSource, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as ReferenceDataLibrary;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type ReferenceDataLibrary",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the VersionIdentifier
        /// </summary>
        public virtual string VersionIdentifier
        {
            get { return this.versionIdentifier; }
            set { this.RaiseAndSetIfChanged(ref this.versionIdentifier, value); }
        }

        /// <summary>
        /// Gets or sets the VersionDate
        /// </summary>
        public virtual DateTime? VersionDate
        {
            get { return this.versionDate; }
            set { this.RaiseAndSetIfChanged(ref this.versionDate, value); }
        }

        /// <summary>
        /// Gets or sets the Author
        /// </summary>
        public virtual string Author
        {
            get { return this.author; }
            set { this.RaiseAndSetIfChanged(ref this.author, value); }
        }

        /// <summary>
        /// Gets or sets the PublicationYear
        /// </summary>
        public virtual int? PublicationYear
        {
            get { return this.publicationYear; }
            set { this.RaiseAndSetIfChanged(ref this.publicationYear, value); }
        }

        /// <summary>
        /// Gets or sets the Language
        /// </summary>
        public virtual string Language
        {
            get { return this.language; }
            set { this.RaiseAndSetIfChanged(ref this.language, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public virtual bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedPublisher
        /// </summary>
        public virtual Organization SelectedPublisher
        {
            get { return this.selectedPublisher; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPublisher, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Organization"/>s for <see cref="SelectedPublisher"/>
        /// </summary>
        public ReactiveList<Organization> PossiblePublisher { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedPublishedIn
        /// </summary>
        public virtual ReferenceSource SelectedPublishedIn
        {
            get { return this.selectedPublishedIn; }
            set { this.RaiseAndSetIfChanged(ref this.selectedPublishedIn, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ReferenceSource"/>s for <see cref="SelectedPublishedIn"/>
        /// </summary>
        public ReactiveList<ReferenceSource> PossiblePublishedIn { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="Category"/>s
        /// </summary>
        private ReactiveList<Category> category;

        /// <summary>
        /// Gets or sets the list of selected <see cref="Category"/>s
        /// </summary>
        public ReactiveList<Category> Category 
        { 
            get { return this.category; } 
            set { this.RaiseAndSetIfChanged(ref this.category, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="Category"/> for <see cref="Category"/>
        /// </summary>
        public ReactiveList<Category> PossibleCategory { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedPublisher"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedPublisherCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedPublishedIn"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedPublishedInCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedPublisherCommand = this.WhenAny(vm => vm.SelectedPublisher, v => v.Value != null);
            this.InspectSelectedPublisherCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedPublisherCommand);
            this.InspectSelectedPublisherCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPublisher));
            var canExecuteInspectSelectedPublishedInCommand = this.WhenAny(vm => vm.SelectedPublishedIn, v => v.Value != null);
            this.InspectSelectedPublishedInCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedPublishedInCommand);
            this.InspectSelectedPublishedInCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedPublishedIn));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.VersionIdentifier = this.VersionIdentifier;
            clone.VersionDate = this.VersionDate;
            clone.Author = this.Author;
            clone.PublicationYear = this.PublicationYear;
            clone.Language = this.Language;
            clone.IsDeprecated = this.IsDeprecated;
            clone.Publisher = this.SelectedPublisher;
            clone.PublishedIn = this.SelectedPublishedIn;
            clone.Category.Clear();
            clone.Category.AddRange(this.Category);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossiblePublisher = new ReactiveList<Organization>();
            this.PossiblePublishedIn = new ReactiveList<ReferenceSource>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="ReferenceSource"/>
        /// </summary>
        private void PopulatePossibleContainer()
        {
            this.PossibleContainer.Clear();
            // When creating a new Rule, it can be contained by any ReferenceDataLibrary that is currently loaded
            if (this.dialogKind == ThingDialogKind.Create)
            {
                this.PossibleContainer.AddRange(this.Session.OpenReferenceDataLibraries.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
                this.Container = this.PossibleContainer.FirstOrDefault();
                return;
            }

            // When inspecting an existing Rule, only it's container needs to be added to the PossibleContainer property (it cannot be changed)
            if (this.dialogKind == ThingDialogKind.Inspect)
            {
                this.PossibleContainer.Add(this.Thing.Container);
                this.Container = this.Thing.Container;
                return;
            }

            // When updating a Rule, the possible ReferenceDataLibrary can only be the ReferenceDataLibrary in the current chain of ReferenceDataLibrary of the Rule
            if (this.dialogKind == ThingDialogKind.Update)
            {
                var containerRdl = (ReferenceDataLibrary)this.Container;
                this.PossibleContainer.Add(containerRdl);
                var chainOfRdls = containerRdl.GetRequiredRdls();
                this.PossibleContainer.AddRange(chainOfRdls.Where(x => this.PermissionService.CanWrite(x)).Select(x => x.Clone(false)));
            }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.VersionIdentifier = this.Thing.VersionIdentifier;
            this.VersionDate = this.Thing.VersionDate;
            this.Author = this.Thing.Author;
            this.PublicationYear = this.Thing.PublicationYear;
            this.Language = this.Thing.Language;
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.SelectedPublisher = this.Thing.Publisher;
            this.PopulatePossiblePublisher();
            this.SelectedPublishedIn = this.Thing.PublishedIn;
            this.PopulatePossiblePublishedIn();
            this.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="Category"/> property
        /// </summary>
        protected virtual void PopulateCategory()
        {
            this.Category.Clear();

            foreach (var value in this.Thing.Category)
            {
                this.Category.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="PossiblePublisher"/> property
        /// </summary>
        protected virtual void PopulatePossiblePublisher()
        {
            this.PossiblePublisher.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossiblePublishedIn"/> property
        /// </summary>
        protected virtual void PopulatePossiblePublishedIn()
        {
            this.PossiblePublishedIn.Clear();
        }
    }
}
