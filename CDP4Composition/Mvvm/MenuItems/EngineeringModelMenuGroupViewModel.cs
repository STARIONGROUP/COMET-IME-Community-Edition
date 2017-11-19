// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelMenuGroupViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Events;
    using MenuItems;
    using ReactiveUI;

    /// <summary>
    /// Represents the selected <see cref="EngineeringModel"/> to open based on the selected <see cref="Iteration"/>s
    /// </summary>
    public class EngineeringModelMenuGroupViewModel : MenuGroupViewModelBase<EngineeringModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelMenuGroupViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> to add
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public EngineeringModelMenuGroupViewModel(Iteration iteration, ISession session)
            : base(iteration.Container as EngineeringModel, session)
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(
                this.Thing.EngineeringModelSetup)
                .Where(
                    objectChange =>
                        objectChange.EventKind == EventKind.Updated &&
                        objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());
            this.Disposables.Add(engineeringModelSetupSubscription);

            this.SelectedIterations = new ReactiveList<RibbonMenuItemIterationDependentViewModel>();
        }

        /// <summary>
        /// Derives the name string based on containment
        /// </summary>
        /// <returns>The formatted name of the group.</returns>
        protected override string DeriveName()
        {
            return string.Format("{0} : {1}", this.Session.Name, this.Thing.EngineeringModelSetup.Name);
        }
        
        /// <summary>
        /// Gets the list of <see cref="RibbonMenuItemIterationDependentViewModel"/> based on the <see cref="Iteration"/>s available
        /// </summary>
        public ReactiveList<RibbonMenuItemIterationDependentViewModel> SelectedIterations { get; private set; }       
    }
}