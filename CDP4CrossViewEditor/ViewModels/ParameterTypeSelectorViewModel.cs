// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeSelectorViewModel.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4CrossViewEditor.RowModels;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The specialized view-model needed to select parameter types for building cross view editor sheet
    /// </summary>
    public class ParameterTypeSelectorViewModel : ThingSelectorViewModel
    {
        /// <summary>
        /// Gets or sets source parameter list
        /// </summary>
        public ReactiveList<ParameterTypeRowViewModel> ParameterTypeSourceList { get; set; }

        /// <summary>
        /// Gets or sets target parameter list
        /// </summary>
        public ReactiveList<ParameterTypeRowViewModel> ParameterTypeTargetList { get; set; }

        /// <summary>
        /// Gets or sets selected parameter type source list
        /// </summary>
        public ReactiveList<ParameterTypeRowViewModel> SelectedSourceList { get; set; }

        /// <summary>
        /// Gets or sets selected parameter type target list
        /// </summary>
        public ReactiveList<ParameterTypeRowViewModel> SelectedTargetList { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeSelectorViewModel"/> class.
        /// </summary>
        /// <param name="iteration">Current opened iteration <see cref="Iteration"/></param>
        /// <param name="session">Current opened session <see cref="ISession"/></param>
        public ParameterTypeSelectorViewModel(Iteration iteration, ISession session) : base(iteration, session, ClassKind.ParameterBase)
        {
            this.ParameterTypeSourceList = new ReactiveList<ParameterTypeRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.ParameterTypeTargetList = new ReactiveList<ParameterTypeRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.SelectedSourceList = new ReactiveList<ParameterTypeRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.SelectedTargetList = new ReactiveList<ParameterTypeRowViewModel>
            {
                ChangeTrackingEnabled = true
            };
        }

        /// <summary>
        /// Get parameter type data from the current iteration
        /// </summary>
        public override void BindData()
        {
            var iterationElements = this.Iteration.Element.AsEnumerable<ElementBase>();

            if (iterationElements == null)
            {
                return;
            }

            var elements = iterationElements.Union(this.Iteration.Element.SelectMany(e => e.ContainedElement).AsEnumerable<ElementBase>());

            foreach (var element in elements)
            {
                switch (element)
                {
                    case ElementDefinition definition:
                    {
                        this.AddParameterType(definition.Parameter);
                        break;
                    }

                    case ElementUsage usage:
                    {
                        this.AddParameterType(usage.ParameterOverride);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Add parameter type to source parameter type list
        /// </summary>
        /// <typeparam name="T">Generic type <see cref="ParameterBase"/></typeparam>
        /// <param name="containerList">Source for <see cref="Parameter"/>/<see cref="ParameterOverride"/></param>
        private void AddParameterType<T>(IEnumerable<T> containerList) where T : ParameterBase
        {
            foreach (var parameter in containerList)
            {
                if (this.ParameterTypeSourceList.Any(parameterType => parameterType.Thing == parameter.ParameterType))
                {
                    continue;
                }

                this.ParameterTypeSourceList.Add(new ParameterTypeRowViewModel(parameter.ParameterType, this.Session, null));
            }
        }

        /// <summary>
        /// Move parameter types back to source list
        /// </summary>
        protected override void ExecuteMoveToSource()
        {
            ExecuteMove(this.ParameterTypeTargetList, this.ParameterTypeSourceList, this.SelectedTargetList);
        }

        /// <summary>
        /// Executes clear selected items command
        /// </summary>
        protected override void ExecuteClear()
        {
            this.SelectedTargetList = this.ParameterTypeTargetList;
            this.ExecuteMoveToSource();
        }

        /// <summary>
        /// Move parameter types back to target list
        /// </summary>
        protected override void ExecuteMoveToTarget()
        {
            ExecuteMove(this.ParameterTypeSourceList, this.ParameterTypeTargetList, this.SelectedSourceList);
        }
    }
}
