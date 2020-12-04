// -------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="PossibleFiniteStateList"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.PossibleFiniteStateList)]
    public class PossibleFiniteStateListDialogViewModel : CDP4CommonView.PossibleFiniteStateListDialogViewModel, IThingDialogViewModel
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PossibleFiniteStateListDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListDialogViewModel"/> class
        /// </summary>
        /// <param name="possibleFiniteStateList">
        /// The <see cref="PossibleFiniteStateList"/> that is the subject of the current view-model. This is the object
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
        /// <param name="container">The Container <see cref="Thing"/> of the created <see cref="MultiRelationshipRule"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public PossibleFiniteStateListDialogViewModel(PossibleFiniteStateList possibleFiniteStateList, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(possibleFiniteStateList, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        #endregion

        #region properties

        /// <summary>
        /// Gets the <see cref="ICommand"/> to set the default <see cref="PossibleFiniteState"/>
        /// </summary>
        public ReactiveCommand<object> SetDefaultStateCommand { get; private set; } 
        #endregion

        #region DialogBase Methods

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.SelectedOwner).Subscribe(_ => this.UpdateOkCanExecute());
            this.PossibleState.ChangeTrackingEnabled = true;
            this.PossibleState.CountChanged.Subscribe(_ => this.UpdateOkCanExecute());
            this.SetDefaultStateCommand =
                ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedPossibleState).Select(x => x != null && !this.IsReadOnly));
            this.SetDefaultStateCommand.Subscribe(_ => this.ExecuteSetDefaultCommand());
        }

        /// <summary>
        /// Populate the <see cref="PossibleCategory"/> and <see cref="Category"/>
        /// </summary>
        protected override void PopulateCategory()
        {
            // populate the possible categories
            var iteration = (Iteration)this.Container;
            var model = (EngineeringModel)iteration.Container;

            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var possibleRdls = mrdl.GetRequiredRdls().ToList();
            possibleRdls.Add(mrdl);

            var categories = possibleRdls.SelectMany(x => x.DefinedCategory.Where(cat => cat.PermissibleClass.Contains(ClassKind.PossibleFiniteStateList))).OrderBy(x => x.Name).ToList();
            this.PossibleCategory.Clear();
            this.PossibleCategory.AddRange(categories);

            base.PopulateCategory();
        }

        /// <summary>
        /// Populate the possible owners
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            var iteration = (Iteration)this.Container;
            var model = (EngineeringModel)iteration.Container;
            this.PossibleOwner.AddRange(model.EngineeringModelSetup.ActiveDomain.OrderBy(d => d.Name));
        }

        /// <summary>
        /// Populate the possible <see cref="PossibleFiniteState"/> rows
        /// </summary>
        protected override void PopulatePossibleState()
        {
            this.PossibleState.Clear();

            var defaultStateIid = (this.Thing.DefaultState == null) ? Guid.Empty : this.Thing.DefaultState.Iid;
            foreach (PossibleFiniteState thing in this.Thing.PossibleState.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new PossibleFiniteStateRowViewModel(thing, this.Session, this);
                this.PossibleState.Add(row);
                row.Index = this.Thing.PossibleState.IndexOf(thing);
                row.IsDefault = thing.Iid == defaultStateIid;
            }

            this.SelectedDefaultState = this.PossibleState.Select(s => s.Thing).SingleOrDefault(x => x.Iid == defaultStateIid);
        }

        /// <summary>
        /// Update the <see cref="OkCanExecute"/> property
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.SelectedOwner != null && this.PossibleState.Count > 0;
        }
        #endregion

        /// <summary>
        /// Executes the <see cref="SetDefaultStateCommand"/>
        /// </summary>
        private void ExecuteSetDefaultCommand()
        {
            foreach (PossibleFiniteStateRowViewModel row in this.PossibleState)
            {
                row.IsDefault = false;
            }

            ((PossibleFiniteStateRowViewModel)this.SelectedPossibleState).IsDefault = true;
            this.SelectedDefaultState = this.SelectedPossibleState.Thing;
        }
    }
}