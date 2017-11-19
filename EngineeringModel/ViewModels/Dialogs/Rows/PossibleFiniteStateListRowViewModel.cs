// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateListRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The <see cref="PossibleFiniteStateList"/> row used in the <see cref="ActualFiniteStateListDialogViewModel"/>
    /// </summary>
    public class PossibleFiniteStateListRowViewModel : RowViewModelBase<PossibleFiniteStateList>
    {
        /// <summary>
        /// Backing field for <see cref="PossibleFiniteStateList"/>
        /// </summary>
        private PossibleFiniteStateList possibleFiniteStateList;

        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListRowViewModel"/> class
        /// </summary>
        /// <param name="possibleFiniteStateList">The <see cref="PossibleFiniteStateList"/> that this row represent initially</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="possiblePossibleFiniteStateList">the possible <see cref="PossibleFiniteStateList"/> for this row</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that contains this row</param>
        public PossibleFiniteStateListRowViewModel(PossibleFiniteStateList possibleFiniteStateList, ISession session, IEnumerable<PossibleFiniteStateList> possiblePossibleFiniteStateList, IViewModelBase<Thing> containerViewModel)
            : base(possibleFiniteStateList, session, containerViewModel)
        {
            this.PossiblePossibleFiniteStateList =
                new ReactiveList<PossibleFiniteStateList>(possiblePossibleFiniteStateList);
            this.PossibleFiniteStateList = this.PossiblePossibleFiniteStateList.Single(x => x == possibleFiniteStateList);

            var dialog = containerViewModel as ActualFiniteStateListDialogViewModel;
            if (dialog != null)
            {
                this.IsReadOnly = dialog.IsReadOnly;

                // skip the initial value
                this.WhenAnyValue(x => x.PossibleFiniteStateList)
                    .Skip(1)
                    .Subscribe(_ => dialog.RefreshPossibleFiniteStateListRows());
            }
        }

        /// <summary>
        /// Gets a value indicating whether the row is read-only
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="PossibleFiniteStateList"/> selected for this row
        /// </summary>
        public PossibleFiniteStateList PossibleFiniteStateList
        {
            get { return this.possibleFiniteStateList; }
            set { this.RaiseAndSetIfChanged(ref this.possibleFiniteStateList, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="PossibleFiniteStateList"/>s for this row
        /// </summary>
        public ReactiveList<PossibleFiniteStateList> PossiblePossibleFiniteStateList { get; private set; }
    }
}