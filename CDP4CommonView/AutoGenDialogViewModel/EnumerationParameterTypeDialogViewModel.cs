// -------------------------------------------------------------------------------------------------
// <copyright file="EnumerationParameterTypeDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="EnumerationParameterType"/>
    /// </summary>
    public partial class EnumerationParameterTypeDialogViewModel : ScalarParameterTypeDialogViewModel<EnumerationParameterType>
    {
        /// <summary>
        /// Backing field for <see cref="AllowMultiSelect"/>
        /// </summary>
        private bool allowMultiSelect;

        /// <summary>
        /// Backing field for <see cref="SelectedValueDefinition"/>
        /// </summary>
        private EnumerationValueDefinitionRowViewModel selectedValueDefinition;


        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerationParameterTypeDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public EnumerationParameterTypeDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerationParameterTypeDialogViewModel"/> class
        /// </summary>
        /// <param name="enumerationParameterType">
        /// The <see cref="EnumerationParameterType"/> that is the subject of the current view-model. This is the object
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
        public EnumerationParameterTypeDialogViewModel(EnumerationParameterType enumerationParameterType, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(enumerationParameterType, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the AllowMultiSelect
        /// </summary>
        public virtual bool AllowMultiSelect
        {
            get { return this.allowMultiSelect; }
            set { this.RaiseAndSetIfChanged(ref this.allowMultiSelect, value); }
        }
        
        /// <summary>
        /// Gets or sets the selected <see cref="EnumerationValueDefinitionRowViewModel"/>
        /// </summary>
        public EnumerationValueDefinitionRowViewModel SelectedValueDefinition
        {
            get { return this.selectedValueDefinition; }
            set { this.RaiseAndSetIfChanged(ref this.selectedValueDefinition, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="EnumerationValueDefinition"/>
        /// </summary>
        public ReactiveList<EnumerationValueDefinitionRowViewModel> ValueDefinition { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a EnumerationValueDefinition
        /// </summary>
        public ReactiveCommand<object> CreateValueDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a EnumerationValueDefinition
        /// </summary>
        public ReactiveCommand<object> DeleteValueDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a EnumerationValueDefinition
        /// </summary>
        public ReactiveCommand<object> EditValueDefinitionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a EnumerationValueDefinition
        /// </summary>
        public ReactiveCommand<object> InspectValueDefinitionCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Move-Up <see cref="ICommand"/> to move up the order of a EnumerationValueDefinition 
        /// </summary>
        public ReactiveCommand<object> MoveUpValueDefinitionCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Move-Down <see cref="ICommand"/> to move down the order of a EnumerationValueDefinition
        /// </summary>
        public ReactiveCommand<object> MoveDownValueDefinitionCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateValueDefinitionCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedValueDefinitionCommand = this.WhenAny(vm => vm.SelectedValueDefinition, v => v.Value != null);
            var canExecuteEditSelectedValueDefinitionCommand = this.WhenAny(vm => vm.SelectedValueDefinition, v => v.Value != null && !this.IsReadOnly);

            this.CreateValueDefinitionCommand = ReactiveCommand.Create(canExecuteCreateValueDefinitionCommand);
            this.CreateValueDefinitionCommand.Subscribe(_ => this.ExecuteCreateCommand<EnumerationValueDefinition>(this.PopulateValueDefinition));

            this.DeleteValueDefinitionCommand = ReactiveCommand.Create(canExecuteEditSelectedValueDefinitionCommand);
            this.DeleteValueDefinitionCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedValueDefinition.Thing, this.PopulateValueDefinition));

            this.EditValueDefinitionCommand = ReactiveCommand.Create(canExecuteEditSelectedValueDefinitionCommand);
            this.EditValueDefinitionCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedValueDefinition.Thing, this.PopulateValueDefinition));

            this.InspectValueDefinitionCommand = ReactiveCommand.Create(canExecuteInspectSelectedValueDefinitionCommand);
            this.InspectValueDefinitionCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedValueDefinition.Thing));

            this.MoveUpValueDefinitionCommand = ReactiveCommand.Create(canExecuteEditSelectedValueDefinitionCommand);
            this.MoveUpValueDefinitionCommand.Subscribe(_ => this.ExecuteMoveUpCommand(this.ValueDefinition, this.SelectedValueDefinition));

            this.MoveDownValueDefinitionCommand = ReactiveCommand.Create(canExecuteEditSelectedValueDefinitionCommand);
            this.MoveDownValueDefinitionCommand.Subscribe(_ => this.ExecuteMoveDownCommand(this.ValueDefinition, this.SelectedValueDefinition));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.AllowMultiSelect = this.AllowMultiSelect;

            if (!clone.ValueDefinition.SortedItems.Values.SequenceEqual(this.ValueDefinition.Select(x => x.Thing)))
            {
                var itemCount = this.ValueDefinition.Count;
                for (var i = 0; i < itemCount; i++)
                {
                    var item = this.ValueDefinition[i].Thing;
                    var currentIndex = clone.ValueDefinition.IndexOf(item);

                    if (currentIndex != i)
                    {
                        clone.ValueDefinition.Move(currentIndex, i);
                    }
                }
            }
            
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ValueDefinition = new ReactiveList<EnumerationValueDefinitionRowViewModel>();
            this.PopulatePossibleContainer();
        }

        /// <summary>
        /// Populate the possible containers for a <see cref="EnumerationParameterType"/>
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
            this.AllowMultiSelect = this.Thing.AllowMultiSelect;
            this.PopulateValueDefinition();
        }

        /// <summary>
        /// Populates the <see cref="ValueDefinition"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateValueDefinition()
        {
            this.ValueDefinition.Clear();
            foreach (EnumerationValueDefinition thing in this.Thing.ValueDefinition.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new EnumerationValueDefinitionRowViewModel(thing, this.Session, this);
                this.ValueDefinition.Add(row);
                row.Index = this.Thing.ValueDefinition.IndexOf(thing);
            }
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
            foreach(var valueDefinition in this.ValueDefinition)
            {
                valueDefinition.Dispose();
            }
        }
    }
}
