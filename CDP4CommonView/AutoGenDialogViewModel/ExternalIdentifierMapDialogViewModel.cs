// -------------------------------------------------------------------------------------------------
// <copyright file="ExternalIdentifierMapDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ExternalIdentifierMap"/>
    /// </summary>
    public partial class ExternalIdentifierMapDialogViewModel : DialogViewModelBase<ExternalIdentifierMap>
    {
        /// <summary>
        /// Backing field for <see cref="ExternalModelName"/>
        /// </summary>
        private string externalModelName;

        /// <summary>
        /// Backing field for <see cref="ExternalToolName"/>
        /// </summary>
        private string externalToolName;

        /// <summary>
        /// Backing field for <see cref="ExternalToolVersion"/>
        /// </summary>
        private string externalToolVersion;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="SelectedExternalFormat"/>
        /// </summary>
        private ReferenceSource selectedExternalFormat;

        /// <summary>
        /// Backing field for <see cref="SelectedOwner"/>
        /// </summary>
        private DomainOfExpertise selectedOwner;

        /// <summary>
        /// Backing field for <see cref="SelectedCorrespondence"/>
        /// </summary>
        private IdCorrespondenceRowViewModel selectedCorrespondence;


        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalIdentifierMapDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ExternalIdentifierMapDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalIdentifierMapDialogViewModel"/> class
        /// </summary>
        /// <param name="externalIdentifierMap">
        /// The <see cref="ExternalIdentifierMap"/> that is the subject of the current view-model. This is the object
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
        public ExternalIdentifierMapDialogViewModel(ExternalIdentifierMap externalIdentifierMap, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(externalIdentifierMap, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Iteration;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Iteration",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ExternalModelName
        /// </summary>
        public virtual string ExternalModelName
        {
            get { return this.externalModelName; }
            set { this.RaiseAndSetIfChanged(ref this.externalModelName, value); }
        }

        /// <summary>
        /// Gets or sets the ExternalToolName
        /// </summary>
        public virtual string ExternalToolName
        {
            get { return this.externalToolName; }
            set { this.RaiseAndSetIfChanged(ref this.externalToolName, value); }
        }

        /// <summary>
        /// Gets or sets the ExternalToolVersion
        /// </summary>
        public virtual string ExternalToolVersion
        {
            get { return this.externalToolVersion; }
            set { this.RaiseAndSetIfChanged(ref this.externalToolVersion, value); }
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public virtual string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedExternalFormat
        /// </summary>
        public virtual ReferenceSource SelectedExternalFormat
        {
            get { return this.selectedExternalFormat; }
            set { this.RaiseAndSetIfChanged(ref this.selectedExternalFormat, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ReferenceSource"/>s for <see cref="SelectedExternalFormat"/>
        /// </summary>
        public ReactiveList<ReferenceSource> PossibleExternalFormat { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedOwner
        /// </summary>
        public virtual DomainOfExpertise SelectedOwner
        {
            get { return this.selectedOwner; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOwner, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="DomainOfExpertise"/>s for <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> PossibleOwner { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="IdCorrespondenceRowViewModel"/>
        /// </summary>
        public IdCorrespondenceRowViewModel SelectedCorrespondence
        {
            get { return this.selectedCorrespondence; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCorrespondence, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="IdCorrespondence"/>
        /// </summary>
        public ReactiveList<IdCorrespondenceRowViewModel> Correspondence { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedExternalFormat"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedExternalFormatCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedOwner"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedOwnerCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a IdCorrespondence
        /// </summary>
        public ReactiveCommand<object> CreateCorrespondenceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a IdCorrespondence
        /// </summary>
        public ReactiveCommand<object> DeleteCorrespondenceCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a IdCorrespondence
        /// </summary>
        public ReactiveCommand<object> EditCorrespondenceCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a IdCorrespondence
        /// </summary>
        public ReactiveCommand<object> InspectCorrespondenceCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateCorrespondenceCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedCorrespondenceCommand = this.WhenAny(vm => vm.SelectedCorrespondence, v => v.Value != null);
            var canExecuteEditSelectedCorrespondenceCommand = this.WhenAny(vm => vm.SelectedCorrespondence, v => v.Value != null && !this.IsReadOnly);

            this.CreateCorrespondenceCommand = ReactiveCommand.Create(canExecuteCreateCorrespondenceCommand);
            this.CreateCorrespondenceCommand.Subscribe(_ => this.ExecuteCreateCommand<IdCorrespondence>(this.PopulateCorrespondence));

            this.DeleteCorrespondenceCommand = ReactiveCommand.Create(canExecuteEditSelectedCorrespondenceCommand);
            this.DeleteCorrespondenceCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedCorrespondence.Thing, this.PopulateCorrespondence));

            this.EditCorrespondenceCommand = ReactiveCommand.Create(canExecuteEditSelectedCorrespondenceCommand);
            this.EditCorrespondenceCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedCorrespondence.Thing, this.PopulateCorrespondence));

            this.InspectCorrespondenceCommand = ReactiveCommand.Create(canExecuteInspectSelectedCorrespondenceCommand);
            this.InspectCorrespondenceCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedCorrespondence.Thing));
            var canExecuteInspectSelectedExternalFormatCommand = this.WhenAny(vm => vm.SelectedExternalFormat, v => v.Value != null);
            this.InspectSelectedExternalFormatCommand = ReactiveCommand.Create(canExecuteInspectSelectedExternalFormatCommand);
            this.InspectSelectedExternalFormatCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedExternalFormat));
            var canExecuteInspectSelectedOwnerCommand = this.WhenAny(vm => vm.SelectedOwner, v => v.Value != null);
            this.InspectSelectedOwnerCommand = ReactiveCommand.Create(canExecuteInspectSelectedOwnerCommand);
            this.InspectSelectedOwnerCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedOwner));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ExternalModelName = this.ExternalModelName;
            clone.ExternalToolName = this.ExternalToolName;
            clone.ExternalToolVersion = this.ExternalToolVersion;
            clone.Name = this.Name;
            clone.ExternalFormat = this.SelectedExternalFormat;
            clone.Owner = this.SelectedOwner;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleExternalFormat = new ReactiveList<ReferenceSource>();
            this.PossibleOwner = new ReactiveList<DomainOfExpertise>();
            this.Correspondence = new ReactiveList<IdCorrespondenceRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ExternalModelName = this.Thing.ExternalModelName;
            this.ExternalToolName = this.Thing.ExternalToolName;
            this.ExternalToolVersion = this.Thing.ExternalToolVersion;
            this.Name = this.Thing.Name;
            this.SelectedExternalFormat = this.Thing.ExternalFormat;
            this.PopulatePossibleExternalFormat();
            this.SelectedOwner = this.Thing.Owner;
            this.PopulatePossibleOwner();
            this.PopulateCorrespondence();
        }

        /// <summary>
        /// Populates the <see cref="Correspondence"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateCorrespondence()
        {
            this.Correspondence.Clear();
            foreach (var thing in this.Thing.Correspondence.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new IdCorrespondenceRowViewModel(thing, this.Session, this);
                this.Correspondence.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleExternalFormat"/> property
        /// </summary>
        protected virtual void PopulatePossibleExternalFormat()
        {
            this.PossibleExternalFormat.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleOwner"/> property
        /// </summary>
        protected virtual void PopulatePossibleOwner()
        {
            this.PossibleOwner.Clear();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        /// <remarks>
        /// This method is called by the <see cref="ThingDialogNavigationService"/> when the Dialog is closed
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach(var correspondence in this.Correspondence)
            {
                correspondence.Dispose();
            }
        }
    }
}
