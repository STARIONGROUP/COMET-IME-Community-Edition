// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterStateRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The row representing an <see cref="ActualFiniteState"/>
    /// </summary>
    public class ParameterStateRowViewModel : ParameterValueBaseRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterStateRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">The associated <see cref="ParameterBase"/></param>
        /// <param name="option">The associated <see cref="Option"/></param>
        /// <param name="actualState">The associated <see cref="ActualFiniteState"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container row</param>
        /// <param name="isReadOnly">A value indicating whether the row is read-only</param>
        public ParameterStateRowViewModel(ParameterBase parameterBase, Option option, ActualFiniteState actualState, ISession session, IRowViewModelBase<Thing> containerViewModel, bool isReadOnly)
            : base(parameterBase, session, option, actualState, containerViewModel, 0, isReadOnly)
        {
            this.Name = this.ActualState.Name;
            this.State = this.ActualState.Name;
            this.IsDefault = this.ActualState.IsDefault;
            this.Option = this.ActualOption;

            foreach (var possibleFiniteState in this.ActualState.PossibleState)
            {
                var stateListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(possibleFiniteState)
                                   .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                                   .ObserveOn(RxApp.MainThreadScheduler)
                                   .Subscribe(x => { this.Name = this.ActualState.Name; });
                this.Disposables.Add(stateListener);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PossibleFiniteState"/> is the default value of the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }
    }
}