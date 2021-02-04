// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionSelectorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Adrian Chivu, Cozmin Velciu, Alex Vorobiev
//
//    This file is part of CDP4-Server-Administration-Tool.
//    The CDP4-Server-Administration-Tool is an ECSS-E-TM-10-25 Compliant tool
//    for advanced server administration.
//
//    The CDP4-Server-Administration-Tool is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as
//    published by the Free Software Foundation; either version 3 of the
//    License, or (at your option) any later version.
//
//    The CDP4-Server-Administration-Tool is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.RowModels;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The specialized view-model needed to select element definition for building cross view editor sheet
    /// </summary>
    public class ElementDefinitionSelectorViewModel : ThingSelectorViewModel
    {
        /// <summary>
        /// Gets or sets source element list
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> ElementDefinitionSourceList { get; private set; }

        /// <summary>
        /// Gets or sets target element list
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> ElementDefinitionTargetList { get; private set; }

        /// <summary>
        /// Gets or sets user selected elements source list
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> SelectedSourceList { get; set; }

        /// <summary>
        /// Gets or sets user selected elements target list
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> SelectedTargetList { get; set; }

        public List<Category> Categories { private set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionSelectorViewModel"/> class.
        /// </summary>
        /// <param name="iteration">Current opened iteration <see cref="Iteration"/></param>
        /// <param name="session">Current opened session <see cref="ISession"/></param>
        /// <param name="preservedIids">Current user selection <see cref="List{Guid}"/></param>
        public ElementDefinitionSelectorViewModel(Iteration iteration, ISession session, List<Guid> preservedIids)
            : base(iteration, session, ClassKind.ElementDefinition, preservedIids)
        {
            this.ElementDefinitionSourceList = new ReactiveList<ElementDefinitionRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.ElementDefinitionTargetList = new ReactiveList<ElementDefinitionRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.SelectedSourceList = new ReactiveList<ElementDefinitionRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.SelectedTargetList = new ReactiveList<ElementDefinitionRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.Categories = new List<Category>();
        }

        /// <summary>
        /// Get element definition data from the current iteration
        /// </summary>
        public override void BindData()
        {
            foreach (var elementDefinition in this.Iteration.Element.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                this.ElementDefinitionSourceList.Add(new ElementDefinitionRowViewModel(elementDefinition, this.Session, null));

                var categories = elementDefinition.Category;

                var superCategories = categories
                    .Distinct()
                    .SelectMany(x => x.AllSuperCategories())
                    .ToList();

                this.Categories = this.Categories.Union(categories).Union(superCategories)
                    .ToList();
            }

            this.PreserveSelection();
        }

        /// <summary>
        /// /Preserve element definitions objects selection from workbook xml parts
        /// </summary>
        private void PreserveSelection()
        {
            if (this.PreservedIids == null)
            {
                return;
            }

            this.SelectedSourceList.Clear();

            this.SelectedSourceList.AddRange(this.PreservedIids
                .Select(iid => this.ElementDefinitionSourceList.FirstOrDefault(row => row.Thing.Iid == iid))
                .Where(row => row != null));

            this.ExecuteMoveToTarget();
        }

        /// <summary>
        /// Move element definition back to source list
        /// </summary>
        protected internal override void ExecuteMoveToSource()
        {
            ExecuteMove(this.ElementDefinitionTargetList, this.ElementDefinitionSourceList, this.SelectedTargetList);
        }

        /// <summary>
        /// Move element definition back to target list
        /// </summary>
        protected internal override void ExecuteMoveToTarget()
        {
            ExecuteMove(this.ElementDefinitionSourceList, this.ElementDefinitionTargetList, this.SelectedSourceList);
        }

        /// <summary>
        /// Executes clear selected items command
        /// </summary>
        protected override void ExecuteClear()
        {
            this.SelectedTargetList = this.ElementDefinitionTargetList;
            this.ExecuteMoveToSource();
        }
    }
}
