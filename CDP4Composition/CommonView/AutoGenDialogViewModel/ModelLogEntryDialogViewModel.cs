// -------------------------------------------------------------------------------------------------
// <copyright file="ModelLogEntryDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;
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
    /// dialog-view-model class representing a <see cref="ModelLogEntry"/>
    /// </summary>
    public partial class ModelLogEntryDialogViewModel : DialogViewModelBase<ModelLogEntry>
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
        /// Backing field for <see cref="Level"/>
        /// </summary>
        private LogLevelKind level;

        /// <summary>
        /// Backing field for <see cref="SelectedAuthor"/>
        /// </summary>
        private Person selectedAuthor;


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLogEntryDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ModelLogEntryDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLogEntryDialogViewModel"/> class
        /// </summary>
        /// <param name="modelLogEntry">
        /// The <see cref="ModelLogEntry"/> that is the subject of the current view-model. This is the object
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
        public ModelLogEntryDialogViewModel(ModelLogEntry modelLogEntry, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(modelLogEntry, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as EngineeringModel;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type EngineeringModel",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the LanguageCode
        /// </summary>
        public virtual string LanguageCode
        {
            get { return this.languageCode; }
            set { this.RaiseAndSetIfChanged(ref this.languageCode, value); }
        }

        /// <summary>
        /// Gets or sets the Content
        /// </summary>
        public virtual string Content
        {
            get { return this.content; }
            set { this.RaiseAndSetIfChanged(ref this.content, value); }
        }

        /// <summary>
        /// Gets or sets the CreatedOn
        /// </summary>
        public virtual DateTime CreatedOn
        {
            get { return this.createdOn; }
            set { this.RaiseAndSetIfChanged(ref this.createdOn, value); }
        }

        /// <summary>
        /// Gets or sets the Level
        /// </summary>
        public virtual LogLevelKind Level
        {
            get { return this.level; }
            set { this.RaiseAndSetIfChanged(ref this.level, value); }
        }

        /// <summary>
        /// Backing field for AffectedItemIid
        /// </summary>
        public ReactiveList<Guid> affectedItemIid;

        /// <summary>
        /// Gets or sets the AffectedItemIid
        /// </summary>
        public ReactiveList<Guid> AffectedItemIid
        {
            get { return this.affectedItemIid; }
            set { this.RaiseAndSetIfChanged(ref this.affectedItemIid, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedAuthor
        /// </summary>
        public virtual Person SelectedAuthor
        {
            get { return this.selectedAuthor; }
            set { this.RaiseAndSetIfChanged(ref this.selectedAuthor, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Person"/>s for <see cref="SelectedAuthor"/>
        /// </summary>
        public ReactiveList<Person> PossibleAuthor { get; protected set; }
        
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
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedAuthor"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedAuthorCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedAuthorCommand = this.WhenAny(vm => vm.SelectedAuthor, v => v.Value != null);
            this.InspectSelectedAuthorCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedAuthorCommand);
            this.InspectSelectedAuthorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedAuthor));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.LanguageCode = this.LanguageCode;
            clone.Content = this.Content;
            clone.CreatedOn = this.CreatedOn;
            clone.Level = this.Level;
            clone.AffectedItemIid.Clear();
            clone.AffectedItemIid.AddRange(this.AffectedItemIid);
 
            clone.Author = this.SelectedAuthor;
            clone.Category.Clear();
            clone.Category.AddRange(this.Category);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.AffectedItemIid = new ReactiveList<Guid>();
            this.PossibleAuthor = new ReactiveList<Person>();
            this.Category = new ReactiveList<Category>();
            this.PossibleCategory = new ReactiveList<Category>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.LanguageCode = this.Thing.LanguageCode;
            this.Content = this.Thing.Content;
            this.CreatedOn = this.Thing.CreatedOn;
            this.Level = this.Thing.Level;
            this.PopulateAffectedItemIid();
            this.SelectedAuthor = this.Thing.Author;
            this.PopulatePossibleAuthor();
            this.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="AffectedItemIid"/> property
        /// </summary>
        protected virtual void PopulateAffectedItemIid()
        {
            this.AffectedItemIid.Clear();
            foreach(var value in this.Thing.AffectedItemIid)
            {
                this.AffectedItemIid.Add(value);
            }
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
        /// Populates the <see cref="PossibleAuthor"/> property
        /// </summary>
        protected virtual void PopulatePossibleAuthor()
        {
            this.PossibleAuthor.Clear();
        }
    }
}
