// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOptionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    /// The row representing an <see cref="Option"/> of a <see cref="ParameterBase"/>
    /// </summary>
    public class ParameterOptionRowViewModel : ParameterValueBaseRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOptionRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">The associated <see cref="ParameterBase"/></param>
        /// <param name="option">The associated <see cref="Option"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container row</param>
        /// <param name="isReadOnly">A value indicating whether the row is read-only</param>
        public ParameterOptionRowViewModel(ParameterBase parameterBase, Option option, ISession session, IRowViewModelBase<Thing> containerViewModel, bool isReadOnly)
            : base(parameterBase, session, option, null, containerViewModel, 0, isReadOnly)
        {
            this.Name = this.ActualOption.Name;
            this.Option = this.ActualOption;

            var optionListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Option)
                                   .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                                   .ObserveOn(RxApp.MainThreadScheduler)
                                   .Subscribe(x => { this.Name = this.ActualOption.Name; });
            this.Disposables.Add(optionListener);
        }
    }
}