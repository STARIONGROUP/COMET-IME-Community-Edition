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
    using System.Collections.Generic;
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
        public ClassKind ThingClassKind
        {
            private set => this.RaiseAndSetIfChanged(ref this.thingClassKind, value);
            get => this.thingClassKind;
        }

        /// <summary>
        /// Gets or sets the <see cref="Iteration"/>
        /// </summary>
        public Iteration Iteration { get; private set; }

        /// <summary>
        /// Gets or sets the user session <see cref="ISession"/>
        /// </summary>
        protected ISession Session { get; private set; }

        /// <summary>
        /// Gets/sets the move command <see cref="ReactiveCommand"/> from target to source
        /// </summary>
        public ReactiveCommand<object> MoveItemsToSource { get; private set; }

        /// <summary>
        /// Gets/sets the move command <see cref="ReactiveCommand"/> from source to target
        /// </summary>
        public ReactiveCommand<object> MoveItemsToTarget { get; private set; }

        /// <summary>
        /// Gets/sets the clearing list command <see cref="ReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> ClearItems { get; private set; }

        /// <summary>
        /// Gets/sets current user selection thing ids
        /// </summary>
        public List<Guid> PreservedIids { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingSelectorViewModel"/> class.
        /// </summary>
        /// <param name="iteration">Current opened iteration <see cref="Iteration"/> </param>
        /// <param name="session">Current opened session <see cref="ISession"/></param>
        /// <param name="thingClassKind">Current type of thing <see cref="ClassKind"/></param>
        /// <param name="preservedIids">Current thing selection<see cref="List{Guid}"/></param>
        protected ThingSelectorViewModel(Iteration iteration, ISession session, ClassKind thingClassKind, List<Guid> preservedIids)
        {
            this.Iteration = iteration;
            this.Session = session;
            this.ThingClassKind = thingClassKind;
            this.PreservedIids = preservedIids;

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
        protected internal virtual void ExecuteMoveToSource()
        {
        }

        /// <summary>
        /// Executes move to target command <see cref="MoveItemsToTarget"/>
        /// Things will be selected for Cross View Editor table/report
        /// </summary>
        protected internal virtual void ExecuteMoveToTarget()
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
        /// <param name="sourceList">Source list</param>
        /// <param name="targetList">Target list</param>
        /// <param name="selectedElements">Elements that will be moved beween source and target</param>
        protected static void ExecuteMove<T>(ReactiveList<T> sourceList, ReactiveList<T> targetList, ReactiveList<T> selectedElements)
            where T : ReactiveObject
        {
            if (selectedElements.Count == 0)
            {
                return;
            }

            targetList.AddRange(selectedElements);

            foreach (var element in selectedElements.Reverse())
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

            this.ClearItems = ReactiveCommand.Create();
            this.ClearItems.Subscribe(_ => this.ExecuteClear());
        }
    }
}
