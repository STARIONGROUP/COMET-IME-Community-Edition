// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryNoteDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
	using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="BinaryNote"/>
    /// </summary>
    public partial class BinaryNoteDialogViewModel : NoteDialogViewModel<BinaryNote>
    {
        /// <summary>
        /// Backing field for <see cref="Caption"/>
        /// </summary>
        private string caption;

        /// <summary>
        /// Backing field for <see cref="SelectedFileType"/>
        /// </summary>
        private FileType selectedFileType;


        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryNoteDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BinaryNoteDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryNoteDialogViewModel"/> class
        /// </summary>
        /// <param name="binaryNote">
        /// The <see cref="BinaryNote"/> that is the subject of the current view-model. This is the object
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
        public BinaryNoteDialogViewModel(BinaryNote binaryNote, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(binaryNote, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Page;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Page",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Caption
        /// </summary>
        public virtual string Caption
        {
            get { return this.caption; }
            set { this.RaiseAndSetIfChanged(ref this.caption, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedFileType
        /// </summary>
        public virtual FileType SelectedFileType
        {
            get { return this.selectedFileType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedFileType, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="FileType"/>s for <see cref="SelectedFileType"/>
        /// </summary>
        public ReactiveList<FileType> PossibleFileType { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedFileType"/>
        /// </summary>
        public ReactiveCommand<object> InspectSelectedFileTypeCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            var canExecuteInspectSelectedFileTypeCommand = this.WhenAny(vm => vm.SelectedFileType, v => v.Value != null);
            this.InspectSelectedFileTypeCommand = ReactiveCommand.Create(canExecuteInspectSelectedFileTypeCommand);
            this.InspectSelectedFileTypeCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedFileType));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Caption = this.Caption;
            clone.FileType = this.SelectedFileType;
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleFileType = new ReactiveList<FileType>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Caption = this.Thing.Caption;
            this.SelectedFileType = this.Thing.FileType;
            this.PopulatePossibleFileType();
        }

        /// <summary>
        /// Populates the <see cref="PossibleFileType"/> property
        /// </summary>
        protected virtual void PopulatePossibleFileType()
        {
            this.PossibleFileType.Clear();
        }
    }
}
