// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDialogViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.Operations;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// dialog-view-model class representing a <see cref="ElementDefinition"/>
    /// </summary>
    public partial class ElementDefinitionDialogViewModel : ElementBaseDialogViewModel<ElementDefinition>
    {
        /// <summary>
        /// Backing field for <see cref="SelectedContainedElement"/>
        /// </summary>
        private ElementUsageRowViewModel selectedContainedElement;

        /// <summary>
        /// Backing field for <see cref="SelectedParameter"/>
        /// </summary>
        private ParameterRowViewModel selectedParameter;

        /// <summary>
        /// Backing field for <see cref="SelectedParameterGroup"/>
        /// </summary>
        private ParameterGroupRowViewModel selectedParameterGroup;


        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ElementDefinitionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionDialogViewModel"/> class
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/> that is the subject of the current view-model. This is the object
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
        public ElementDefinitionDialogViewModel(ElementDefinition elementDefinition, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(elementDefinition, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
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
        /// Gets or sets the selected <see cref="ElementUsageRowViewModel"/>
        /// </summary>
        public ElementUsageRowViewModel SelectedContainedElement
        {
            get { return this.selectedContainedElement; }
            set { this.RaiseAndSetIfChanged(ref this.selectedContainedElement, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ElementUsage"/>
        /// </summary>
        public ReactiveList<ElementUsageRowViewModel> ContainedElement { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParameterRowViewModel"/>
        /// </summary>
        public ParameterRowViewModel SelectedParameter
        {
            get { return this.selectedParameter; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameter, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Parameter"/>
        /// </summary>
        public ReactiveList<ParameterRowViewModel> Parameter { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="ParameterGroupRowViewModel"/>
        /// </summary>
        public ParameterGroupRowViewModel SelectedParameterGroup
        {
            get { return this.selectedParameterGroup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterGroup, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="ParameterGroup"/>
        /// </summary>
        public ReactiveList<ParameterGroupRowViewModel> ParameterGroup { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="ReferencedElement"/>s
        /// </summary>
        private ReactiveList<NestedElement> referencedElement;

        /// <summary>
        /// Gets or sets the list of selected <see cref="NestedElement"/>s
        /// </summary>
        public ReactiveList<NestedElement> ReferencedElement 
        { 
            get { return this.referencedElement; } 
            set { this.RaiseAndSetIfChanged(ref this.referencedElement, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="NestedElement"/> for <see cref="ReferencedElement"/>
        /// </summary>
        public ReactiveList<NestedElement> PossibleReferencedElement { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ElementUsage
        /// </summary>
        public ReactiveCommand<object> CreateContainedElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ElementUsage
        /// </summary>
        public ReactiveCommand<object> DeleteContainedElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ElementUsage
        /// </summary>
        public ReactiveCommand<object> EditContainedElementCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ElementUsage
        /// </summary>
        public ReactiveCommand<object> InspectContainedElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a Parameter
        /// </summary>
        public ReactiveCommand<object> CreateParameterCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a Parameter
        /// </summary>
        public ReactiveCommand<object> DeleteParameterCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a Parameter
        /// </summary>
        public ReactiveCommand<object> EditParameterCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a Parameter
        /// </summary>
        public ReactiveCommand<object> InspectParameterCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a ParameterGroup
        /// </summary>
        public ReactiveCommand<object> CreateParameterGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a ParameterGroup
        /// </summary>
        public ReactiveCommand<object> DeleteParameterGroupCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a ParameterGroup
        /// </summary>
        public ReactiveCommand<object> EditParameterGroupCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a ParameterGroup
        /// </summary>
        public ReactiveCommand<object> InspectParameterGroupCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateContainedElementCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedContainedElementCommand = this.WhenAny(vm => vm.SelectedContainedElement, v => v.Value != null);
            var canExecuteEditSelectedContainedElementCommand = this.WhenAny(vm => vm.SelectedContainedElement, v => v.Value != null && !this.IsReadOnly);

            this.CreateContainedElementCommand = ReactiveCommand.Create(canExecuteCreateContainedElementCommand);
            this.CreateContainedElementCommand.Subscribe(_ => this.ExecuteCreateCommand<ElementUsage>(this.PopulateContainedElement));

            this.DeleteContainedElementCommand = ReactiveCommand.Create(canExecuteEditSelectedContainedElementCommand);
            this.DeleteContainedElementCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedContainedElement.Thing, this.PopulateContainedElement));

            this.EditContainedElementCommand = ReactiveCommand.Create(canExecuteEditSelectedContainedElementCommand);
            this.EditContainedElementCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedContainedElement.Thing, this.PopulateContainedElement));

            this.InspectContainedElementCommand = ReactiveCommand.Create(canExecuteInspectSelectedContainedElementCommand);
            this.InspectContainedElementCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedContainedElement.Thing));
            
            var canExecuteCreateParameterCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParameterCommand = this.WhenAny(vm => vm.SelectedParameter, v => v.Value != null);
            var canExecuteEditSelectedParameterCommand = this.WhenAny(vm => vm.SelectedParameter, v => v.Value != null && !this.IsReadOnly);

            this.CreateParameterCommand = ReactiveCommand.Create(canExecuteCreateParameterCommand);
            this.CreateParameterCommand.Subscribe(_ => this.ExecuteCreateCommand<Parameter>(this.PopulateParameter));

            this.DeleteParameterCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterCommand);
            this.DeleteParameterCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParameter.Thing, this.PopulateParameter));

            this.EditParameterCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterCommand);
            this.EditParameterCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParameter.Thing, this.PopulateParameter));

            this.InspectParameterCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterCommand);
            this.InspectParameterCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameter.Thing));
            
            var canExecuteCreateParameterGroupCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedParameterGroupCommand = this.WhenAny(vm => vm.SelectedParameterGroup, v => v.Value != null);
            var canExecuteEditSelectedParameterGroupCommand = this.WhenAny(vm => vm.SelectedParameterGroup, v => v.Value != null && !this.IsReadOnly);

            this.CreateParameterGroupCommand = ReactiveCommand.Create(canExecuteCreateParameterGroupCommand);
            this.CreateParameterGroupCommand.Subscribe(_ => this.ExecuteCreateCommand<ParameterGroup>(this.PopulateParameterGroup));

            this.DeleteParameterGroupCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterGroupCommand);
            this.DeleteParameterGroupCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedParameterGroup.Thing, this.PopulateParameterGroup));

            this.EditParameterGroupCommand = ReactiveCommand.Create(canExecuteEditSelectedParameterGroupCommand);
            this.EditParameterGroupCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedParameterGroup.Thing, this.PopulateParameterGroup));

            this.InspectParameterGroupCommand = ReactiveCommand.Create(canExecuteInspectSelectedParameterGroupCommand);
            this.InspectParameterGroupCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedParameterGroup.Thing));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.ReferencedElement.Clear();
            clone.ReferencedElement.AddRange(this.ReferencedElement);

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ContainedElement = new ReactiveList<ElementUsageRowViewModel>();
            this.Parameter = new ReactiveList<ParameterRowViewModel>();
            this.ParameterGroup = new ReactiveList<ParameterGroupRowViewModel>();
            this.ReferencedElement = new ReactiveList<NestedElement>();
            this.PossibleReferencedElement = new ReactiveList<NestedElement>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulateContainedElement();
            this.PopulateParameter();
            this.PopulateParameterGroup();
            this.PopulateReferencedElement();
        }

        /// <summary>
        /// Populates the <see cref="ReferencedElement"/> property
        /// </summary>
        protected virtual void PopulateReferencedElement()
        {
            this.ReferencedElement.Clear();

            foreach (var value in this.Thing.ReferencedElement)
            {
                this.ReferencedElement.Add(value);
            }
        } 

        /// <summary>
        /// Populates the <see cref="ContainedElement"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateContainedElement()
        {
            this.ContainedElement.Clear();
            foreach (var thing in this.Thing.ContainedElement.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ElementUsageRowViewModel(thing, this.Session, this);
                this.ContainedElement.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="Parameter"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParameter()
        {
            this.Parameter.Clear();
            foreach (var thing in this.Thing.Parameter.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParameterRowViewModel(thing, this.Session, this);
                this.Parameter.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ParameterGroup"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateParameterGroup()
        {
            this.ParameterGroup.Clear();
            foreach (var thing in this.Thing.ParameterGroup.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParameterGroupRowViewModel(thing, this.Session, this);
                this.ParameterGroup.Add(row);
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
            foreach(var containedElement in this.ContainedElement)
            {
                containedElement.Dispose();
            }
            foreach(var parameter in this.Parameter)
            {
                parameter.Dispose();
            }
            foreach(var parameterGroup in this.ParameterGroup)
            {
                parameterGroup.Dispose();
            }
        }
    }
}
