// -------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to create, edit or inspect a <see cref="PossibleFiniteState"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.PossibleFiniteState)]
    public class PossibleFiniteStateDialogViewModel : CDP4CommonView.PossibleFiniteStateDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PossibleFiniteStateDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateDialogViewModel"/> class
        /// </summary>
        /// <param name="possibleFiniteState">
        /// The <see cref="PossibleFiniteState"/> that is the subject of the current view-model. This is the object
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
        public PossibleFiniteStateDialogViewModel(PossibleFiniteState possibleFiniteState, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(possibleFiniteState, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ActualFiniteState"/> is the default value of the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();

            var defaultstate = ((PossibleFiniteStateList) this.Container).DefaultState;
            this.IsDefault = (defaultstate != null) && defaultstate.Iid == this.Thing.Iid;
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var statelist = (PossibleFiniteStateList)this.Container;

            if (this.IsDefault)
            {
                statelist.DefaultState = this.Thing;
            }
            else if(statelist.DefaultState != null && statelist.DefaultState.Iid == this.Thing.Iid && !this.IsDefault)
            {
                statelist.DefaultState = null;
            }
        }
    }
}