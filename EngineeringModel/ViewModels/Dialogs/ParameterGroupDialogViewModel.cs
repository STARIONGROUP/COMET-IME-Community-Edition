// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="ParameterGroup"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ParameterGroup)]
    public class ParameterGroupDialogViewModel : CDP4CommonView.ParameterGroupDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="SelectedGroupSelection"/>
        /// </summary>
        private GroupSelectionViewModel selectedGroupSelection;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterGroupDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupDialogViewModel"/> class
        /// </summary>
        /// <param name="parameterGroup">
        /// The <see cref="ParameterGroup"/> that is the subject of the current view-model. This is the object
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
        /// <param name="permissionService">
        /// The <see cref="IPermissionService"/> that is used to compute whether the current <see cref="ISession"/> can execute CRUD.
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="MultiRelationshipRule"/></param>
        /// <param name="isNewContainer">
        /// Value indicating whether the container of the <see cref="Thing"/> that is being created is new
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ParameterGroupDialogViewModel(ParameterGroup parameterGroup, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(parameterGroup, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.SelectedGroupSelection).Subscribe(x => this.SelectedContainingGroup = x != null ? x.Thing : null);

        }
        #endregion

        /// <summary>
        /// Gets or sets the selected <see cref="GroupSelectionViewModel"/>
        /// </summary>
        public GroupSelectionViewModel SelectedGroupSelection
        {
            get { return this.selectedGroupSelection; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGroupSelection, value); }
        }

        /// <summary>
        /// Gets the possible groups
        /// </summary>
        public ReactiveList<GroupSelectionViewModel> PossibleGroups { get; private set; }

        /// <summary>
        /// Initializes the view-model
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossibleGroups = new ReactiveList<GroupSelectionViewModel>();
        }

        /// <summary>
        /// Populate the <see cref="ParameterGroupDialogViewModel.PossibleContainingGroup"/>
        /// </summary>
        protected override void PopulatePossibleContainingGroup()
        {
            base.PopulatePossibleContainingGroup();
            this.PossibleGroups.Clear();

            var elementDefinition = this.Container as ElementDefinition;
            if (elementDefinition == null)
            {
                return;
            }

            this.PossibleGroups.AddRange(elementDefinition.ParameterGroup.Where(this.IsValidPossibleContainerGroup).Select(x => new GroupSelectionViewModel(x)));
            this.SelectedGroupSelection = this.PossibleGroups.SingleOrDefault(x => x.Thing == this.Thing.ContainingGroup);
        }

        /// <summary>
        /// This Method checks if a given <see cref="ParameterGroup"/> should be included in the <see cref="PossibleContainingGroup"/>
        /// </summary>
        /// <param name="parameterGroup">
        /// The parameter group to check.
        /// </param>
        /// <returns>
        /// Whether the given <see cref="ParameterGroup"/> can be added to <see cref="PossibleContainingGroup"/> without creating a cycle.
        /// </returns>
        private bool IsValidPossibleContainerGroup(ParameterGroup parameterGroup)
        {
            if (parameterGroup.Iid == this.Thing.Iid)
            {
                return false;
            }

            return parameterGroup.ContainingGroup == null || this.IsValidPossibleContainerGroup(parameterGroup.ContainingGroup);
        }
    }
}