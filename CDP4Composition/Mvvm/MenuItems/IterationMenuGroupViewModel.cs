// -------------------------------------------------------------------------------------------------
// <copyright file="IterationMenuGroupViewModel.cs" company="RHEA System S.A.">
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
    /// Represents the selected <see cref="Iteration"/> to open based on the selected <see cref="Option"/>s
    /// </summary>
    public class IterationMenuGroupViewModel : MenuGroupViewModelBase<Iteration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IterationMenuGroupViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> to add
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public IterationMenuGroupViewModel(Iteration iteration, ISession session)
            : base(iteration, session)
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(
                ((EngineeringModel)this.Thing.Container).EngineeringModelSetup)
                .Where(
                    objectChange =>
                        objectChange.EventKind == EventKind.Updated &&
                        objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());
            this.Disposables.Add(engineeringModelSetupSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(
                this.Thing.IterationSetup)
                .Where(
                    objectChange =>
                        objectChange.EventKind == EventKind.Updated &&
                        objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());
            this.Disposables.Add(iterationSetupSubscription);

            this.SelectedOptions = new ReactiveList<RibbonMenuItemOptionDependentViewModel>();
        }

        /// <summary>
        /// Derives the name string based on containment
        /// </summary>
        /// <returns>The formatted name of the group.</returns>
        protected override string DeriveName()
        {
            return string.Format("{0} : {1} : Iteration {2}", this.Session.Name, ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IterationSetup.IterationNumber);
        }
        
        /// <summary>
        /// Gets the list of <see cref="RibbonMenuItemIterationDependentViewModel"/> based on the <see cref="Option"/>s available
        /// </summary>
        public ReactiveList<RibbonMenuItemOptionDependentViewModel> SelectedOptions { get; private set; }
    }
}