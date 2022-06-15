// -------------------------------------------------------------------------------------------------
// <copyright file="NestedElementDialogViewModel.cs" company="RHEA System S.A.">
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
    /// dialog-view-model class representing a <see cref="NestedElement"/>
    /// </summary>
    public partial class NestedElementDialogViewModel : DialogViewModelBase<NestedElement>
    {
        /// <summary>
        /// Backing field for <see cref="IsVolatile"/>
        /// </summary>
        private bool isVolatile;

        /// <summary>
        /// Backing field for <see cref="SelectedRootElement"/>
        /// </summary>
        private ElementDefinition selectedRootElement;

        /// <summary>
        /// Backing field for <see cref="SelectedNestedParameter"/>
        /// </summary>
        private NestedParameterRowViewModel selectedNestedParameter;


        /// <summary>
        /// Initializes a new instance of the <see cref="NestedElementDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public NestedElementDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedElementDialogViewModel"/> class
        /// </summary>
        /// <param name="nestedElement">
        /// The <see cref="NestedElement"/> that is the subject of the current view-model. This is the object
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
        public NestedElementDialogViewModel(NestedElement nestedElement, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers) : base(nestedElement, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if(container != null)
            {
                var containerThing = container as Option;
                if(containerThing == null)
                {
                    var errorMessage =
                        string.Format(
                            "The container parameter is of type {0}, it shall be of type Option",
                            container.GetType());
                    throw new ArgumentException(errorMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the IsVolatile
        /// </summary>
        public virtual bool IsVolatile
        {
            get { return this.isVolatile; }
            set { this.RaiseAndSetIfChanged(ref this.isVolatile, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedRootElement
        /// </summary>
        public virtual ElementDefinition SelectedRootElement
        {
            get { return this.selectedRootElement; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRootElement, value); }
        }

        /// <summary>
        /// Gets or sets the possible <see cref="ElementDefinition"/>s for <see cref="SelectedRootElement"/>
        /// </summary>
        public ReactiveList<ElementDefinition> PossibleRootElement { get; protected set; }
        
        /// <summary>
        /// Gets or sets the selected <see cref="NestedParameterRowViewModel"/>
        /// </summary>
        public NestedParameterRowViewModel SelectedNestedParameter
        {
            get { return this.selectedNestedParameter; }
            set { this.RaiseAndSetIfChanged(ref this.selectedNestedParameter, value); }
        }

        /// <summary>
        /// Gets or sets the list of <see cref="NestedParameter"/>
        /// </summary>
        public ReactiveList<NestedParameterRowViewModel> NestedParameter { get; protected set; }
        
        /// <summary>
        /// Backing field for <see cref="ElementUsage"/>s
        /// </summary>
        private ReactiveList<ElementUsage> elementUsage;

        /// <summary>
        /// Gets or sets the list of selected <see cref="ElementUsage"/>s
        /// </summary>
        public ReactiveList<ElementUsage> ElementUsage 
        { 
            get { return this.elementUsage; } 
            set { this.RaiseAndSetIfChanged(ref this.elementUsage, value); } 
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="ElementUsage"/> for <see cref="ElementUsage"/>
        /// </summary>
        public ReactiveList<ElementUsage> PossibleElementUsage { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect the <see cref="SelectedRootElement"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSelectedRootElementCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a NestedParameter
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateNestedParameterCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a NestedParameter
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteNestedParameterCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a NestedParameter
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditNestedParameterCommand { get; protected set; }
        
        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a NestedParameter
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectNestedParameterCommand { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            var canExecuteCreateNestedParameterCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedNestedParameterCommand = this.WhenAny(vm => vm.SelectedNestedParameter, v => v.Value != null);
            var canExecuteEditSelectedNestedParameterCommand = this.WhenAny(vm => vm.SelectedNestedParameter, v => v.Value != null && !this.IsReadOnly);

            this.CreateNestedParameterCommand = ReactiveCommandCreator.Create(canExecuteCreateNestedParameterCommand);
            this.CreateNestedParameterCommand.Subscribe(_ => this.ExecuteCreateCommand<NestedParameter>(this.PopulateNestedParameter));

            this.DeleteNestedParameterCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedNestedParameterCommand);
            this.DeleteNestedParameterCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedNestedParameter.Thing, this.PopulateNestedParameter));

            this.EditNestedParameterCommand = ReactiveCommandCreator.Create(canExecuteEditSelectedNestedParameterCommand);
            this.EditNestedParameterCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedNestedParameter.Thing, this.PopulateNestedParameter));

            this.InspectNestedParameterCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedNestedParameterCommand);
            this.InspectNestedParameterCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedNestedParameter.Thing));
            var canExecuteInspectSelectedRootElementCommand = this.WhenAny(vm => vm.SelectedRootElement, v => v.Value != null);
            this.InspectSelectedRootElementCommand = ReactiveCommandCreator.Create(canExecuteInspectSelectedRootElementCommand);
            this.InspectSelectedRootElementCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedRootElement));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.IsVolatile = this.IsVolatile;
            clone.RootElement = this.SelectedRootElement;

            if (!clone.ElementUsage.SortedItems.Values.SequenceEqual(this.ElementUsage))
            {
                var elementUsageCount = this.ElementUsage.Count;
                for (var i = 0; i < elementUsageCount; i++)
                {
                    var item = this.ElementUsage[i];
                    var currentIndex = clone.ElementUsage.IndexOf(item);

                    if (currentIndex != -1 && currentIndex != i)
                    {
                        clone.ElementUsage.Move(currentIndex, i);
                    }
                    else if (currentIndex == -1)
                    {
                        clone.ElementUsage.Insert(i, item);
                    }
                }

                // remove items that are no longer referenced
                for (var i = elementUsageCount; i < clone.ElementUsage.Count; i++)
                {
                    var toRemove = clone.ElementUsage[i];
                    clone.ElementUsage.Remove(toRemove);
                }
            }

        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleRootElement = new ReactiveList<ElementDefinition>();
            this.ElementUsage = new ReactiveList<ElementUsage>();
            this.PossibleElementUsage = new ReactiveList<ElementUsage>();
            this.NestedParameter = new ReactiveList<NestedParameterRowViewModel>();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.IsVolatile = this.Thing.IsVolatile;
            this.SelectedRootElement = this.Thing.RootElement;
            this.PopulatePossibleRootElement();
            this.PopulateNestedParameter();
        }

        /// <summary>
        /// Populates the <see cref="NestedParameter"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected virtual void PopulateNestedParameter()
        {
            this.NestedParameter.Clear();
            foreach (var thing in this.Thing.NestedParameter.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new NestedParameterRowViewModel(thing, this.Session, this);
                this.NestedParameter.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="PossibleRootElement"/> property
        /// </summary>
        protected virtual void PopulatePossibleRootElement()
        {
            this.PossibleRootElement.Clear();
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
            foreach(var nestedParameter in this.NestedParameter)
            {
                nestedParameter.Dispose();
            }
        }
    }
}
