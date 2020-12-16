// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingSelectorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The base view-model needed to select elements and parameters for building cross view editor sheet
    /// </summary>
    public class ThingSelectorViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="ClassKind"/> property
        /// </summary>
        private ClassKind thingClassKind;

        /// <summary>
        /// Gets or sets thing type
        /// </summary>
        private ClassKind ThingClassKind
        {
            get => this.thingClassKind;
            set => this.RaiseAndSetIfChanged(ref this.thingClassKind, value);
        }

        /// <summary>
        /// Gets the <see cref="Iteration"/>
        /// </summary>
        public Iteration Iteration { get; private set; }

        public ISession Session { get; private set; }

        /// <summary>
        /// Gets/sets the move command <see cref="ReactiveCommand"/> from target to source
        /// </summary>
        public ReactiveCommand<object> MoveItemsToSource { get; private set; }

        /// <summary>
        /// Gets/sets the move command <see cref="ReactiveCommand"/> from source to target
        /// </summary>
        public ReactiveCommand<object> MoveItemsToTarget { get; private set; }

        /// <summary>
        /// Gets/sets the moving up element list command <see cref="ReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> MoveItemsUp { get; private set; }

        /// <summary>
        /// Gets/sets the moving down element list command <see cref="ReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> MoveItemsDown { get; private set; }

        /// <summary>
        /// Gets/sets the sorting list command <see cref="ReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> SortItems { get; private set; }

        /// <summary>
        /// Gets/sets the clearing list command <see cref="ReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> ClearItems { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingSelectorViewModel"/> class.
        /// </summary>
        /// <param name="iteration">Current opened iteration <see cref="Iteration"/> </param>
        /// <param name="session"></param>
        /// <param name="thingClassKind">Current type of thing <see cref="ClassKind"/></param>
        public ThingSelectorViewModel(Iteration iteration, ISession session, ClassKind thingClassKind)
        {
            this.Iteration = iteration;
            this.Session = session;
            this.ThingClassKind = thingClassKind;
            this.AddSubscriptions();
        }

        /// <summary>
        /// Bind source data depends on thing type <see cref="ClassKind"/>
        /// </summary>
        public virtual void BindData()
        {
        }

        /// <summary>
        /// Executes move to source command <see cref="MoveItemsToSource"/>
        /// Things will be moved back to selection source
        /// </summary>
        protected virtual void ExecuteMoveToSource()
        {
        }

        /// <summary>
        /// Executes move to target command <see cref="MoveItemsToTarget"/>
        /// Things will be selected for Cross View Editor table/report
        /// </summary>
        protected virtual void ExecuteMoveToTarget()
        {
        }

        /// <summary>
        /// Executes clear selected items command <see cref="ClearItems"/>
        /// </summary>
        protected virtual void ExecuteClear()
        {
        }

        /// <summary>
        /// Move elements beween two list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceList">Source list</param>
        /// <param name="targetList">Target list</param>
        /// <param name="selectedElements">Elements that will be moved beween source and target</param>
        protected static void ExecuteMove<T>(ReactiveList<T> sourceList, ReactiveList<T> targetList, ReactiveList<T> selectedElements)
        {
            if (selectedElements.Count == 0)
            {
                return;
            }

            targetList.AddRange(selectedElements);
            var reverseList = selectedElements.Reverse();

            foreach (var element in reverseList)
            {
                sourceList.Remove(element);
            }

            selectedElements.Clear();
        }

        /// <summary>
        /// Add subscriptions
        /// </summary>
        private void AddSubscriptions()
        {
            this.MoveItemsToSource = ReactiveCommand.Create();
            this.MoveItemsToSource.Subscribe(_ => this.ExecuteMoveToSource());

            this.MoveItemsToTarget = ReactiveCommand.Create();
            this.MoveItemsToTarget.Subscribe(_ => this.ExecuteMoveToTarget());

            this.MoveItemsUp = ReactiveCommand.Create();
            this.MoveItemsUp.Subscribe(_ => this.ExecuteMoveUp());

            this.MoveItemsDown = ReactiveCommand.Create();
            this.MoveItemsDown.Subscribe(_ => this.ExecuteMoveDown());

            this.ClearItems = ReactiveCommand.Create();
            this.ClearItems.Subscribe(_ => this.ExecuteClear());

            this.SortItems = ReactiveCommand.Create();
            this.SortItems.Subscribe(_ => this.ExecuteSort());
        }

        /// <summary>
        /// Executes move items command <see cref="MoveItemsUp"/>
        /// ExecuteMove selected <see cref="Thing"/> one position up inside selection list
        /// </summary>
        private void ExecuteMoveUp()
        {
            // TODO #625: Implement ordering and sorting support for selected elements
        }

        /// <summary>
        /// Executes move items command <see cref="MoveItemsDown"/>
        /// ExecuteMove selected <see cref="Thing"/> one position down inside selection list
        /// </summary>
        private void ExecuteMoveDown()
        {
            // TODO #625: Implement ordering and sorting support for selected elements
        }

        /// <summary>
        /// Executes alphabetical sort items command <see cref="SortItems"/>
        /// </summary>
        private void ExecuteSort()
        {
            // TODO #624 Implement filtering for initially displayed elements
        }
    }
}
