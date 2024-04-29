// -------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
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
    using CDP4Common.Types;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using System.Reactive;

    /// <summary>
    /// dialog-view-model class representing a <see cref="FileRevision"/>
    /// </summary>
    public partial class FileRevisionDialogViewModel : DialogViewModelBase<FileRevision>
    {
        /// <summary>
        /// Backing field for <see cref="ContentHash"/>
        /// </summary>
        private string contentHash;

        /// <summary>
        /// Backing field for <see cref="CreatedOn"/>
        /// </summary>
        private DateTime createdOn;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="SelectedCreator"/>
        /// </summary>
        private Participant selectedCreator;

        /// <summary>
        /// Backing field for <see cref="SelectedContainingFolder"/>
        /// </summary>
        private Folder selectedContainingFolder;


        /// <summary>
        /// Initializes a new instance of the <see cref="FileRevisionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public FileRevisionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRevisionDialogViewModel"/> class
        /// </summary>
        /// <param name="fileRevision">
        /// The <see cref="FileRevision"/> that is the subject of the current view-model. This is the object
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
        public FileRevisionDialogViewModel(FileRevision fileRevision, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(fileRevision, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as File;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type File",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the ContentHash
        /// </summary>
        public virtual string ContentHash
        {
            get { return this.contentHash; }
            set { this.RaiseAndSetIfChanged(ref this.contentHash, value); }
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
        /// Gets or sets the Name
        /// </summary>
        public virtual string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedCreator
        /// </summary>
        public virtual Participant SelectedCreator
        {
            get { return this.selectedCreator; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCreator, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Participant"/>s for <see cref="SelectedCreator"/>
        /// </summary>
        public ReactiveList<Participant> PossibleCreator { get; protected set; }

        /// <summary>
        /// Gets or sets the SelectedContainingFolder
        /// </summary>
        public virtual Folder SelectedContainingFolder
        {
            get { return this.selectedContainingFolder; }
            set { this.RaiseAndSetIfChanged(ref this.selectedContainingFolder, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="Folder"/>s for <see cref="SelectedContainingFolder"/>
        /// </summary>
        public ReactiveList<Folder> PossibleContainingFolder { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="FileType"/>s
        /// </summary>
        private ReactiveList<FileType> fileType;

        /// <summary>
        /// Gets or sets the list of selected <see cref="FileType"/>s
        /// </summary>
        public ReactiveList<FileType> FileType 
        { 
            get { return this.fileType; } 
            set { this.RaiseAndSetIfChanged(ref this.fileType, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="FileType"/> for <see cref="FileType"/>
        /// </summary>
        public ReactiveList<FileType> PossibleFileType { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedCreator"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedCreatorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedContainingFolder"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedContainingFolderCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedCreatorCommand = this.WhenAny(vm => vm.SelectedCreator, v => v.Value != null);
            this.InspectSelectedCreatorCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedCreatorCommand);
            this.InspectSelectedCreatorCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedCreator));
            var canExecuteInspectSelectedContainingFolderCommand = this.WhenAny(vm => vm.SelectedContainingFolder, v => v.Value != null);
            this.InspectSelectedContainingFolderCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedContainingFolderCommand);
            this.InspectSelectedContainingFolderCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedContainingFolder));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.CreatedOn = this.CreatedOn;
            clone.Name = this.Name;
            clone.Creator = this.SelectedCreator;
            clone.ContainingFolder = this.SelectedContainingFolder;

            if (!clone.FileType.SortedItems.Values.SequenceEqual(this.FileType))
            {
                var fileTypeCount = this.FileType.Count;
                for (var i = 0; i < fileTypeCount; i++)
                {
                    var item = this.FileType[i];
                    var currentIndex = clone.FileType.IndexOf(item);

                    if (currentIndex != -1 && currentIndex != i)
                    {
                        clone.FileType.Move(currentIndex, i);
                    }
                    else if (currentIndex == -1)
                    {
                        clone.FileType.Insert(i, item);
                    }
                }

                // remove items that are no longer referenced
                for (var i = fileTypeCount; i < clone.FileType.Count; i++)
                {
                    var toRemove = clone.FileType[i];
                    clone.FileType.Remove(toRemove);
                }
            }

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleCreator = new ReactiveList<Participant>();
            this.PossibleContainingFolder = new ReactiveList<Folder>();
            this.FileType = new ReactiveList<FileType>();
            this.PossibleFileType = new ReactiveList<FileType>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.ContentHash = this.Thing.ContentHash;
            this.CreatedOn = this.Thing.CreatedOn;
            this.Name = this.Thing.Name;
            this.SelectedCreator = this.Thing.Creator;
            this.PopulatePossibleCreator();
            this.SelectedContainingFolder = this.Thing.ContainingFolder;
            this.PopulatePossibleContainingFolder();
        }

        /// <summary>
        /// Populates the <see cref="PossibleCreator"/> property
        /// </summary>
        protected virtual void PopulatePossibleCreator()
        {
            this.PossibleCreator.Clear();
        }

        /// <summary>
        /// Populates the <see cref="PossibleContainingFolder"/> property
        /// </summary>
        protected virtual void PopulatePossibleContainingFolder()
        {
            this.PossibleContainingFolder.Clear();
        }
    }
}
