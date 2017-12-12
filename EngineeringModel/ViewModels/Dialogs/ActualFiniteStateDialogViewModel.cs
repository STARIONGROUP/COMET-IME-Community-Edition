// -------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateDialogViewModel.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// The dialog-view model to edit or inspect a <see cref="ActualFiniteState"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ActualFiniteState)]
    public class ActualFiniteStateDialogViewModel : CDP4CommonView.ActualFiniteStateDialogViewModel, IThingDialogViewModel
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ActualFiniteStateDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateDialogViewModel"/> class
        /// </summary>
        /// <param name="actualFiniteState">
        /// The <see cref="ActualFiniteState"/> that is the subject of the current view-model. This is the object
        /// that will be edited.
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
        public ActualFiniteStateDialogViewModel(ActualFiniteState actualFiniteState, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(actualFiniteState, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.IsStateReadOnly = this.Thing.IsDefault;
        }
        #endregion

        /// <summary>
        /// Gets a value indicating whether the state is read-only
        /// </summary>
        public bool IsStateReadOnly { get; private set; }
    }
}