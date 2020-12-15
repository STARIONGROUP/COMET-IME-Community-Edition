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
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.RowModels;

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
        /// <param name="iteration">Current opened iteration <see cref="Iteration"/> </param>
        public ParameterTypeSelectorViewModel(Iteration iteration) : base(iteration, ClassKind.ParameterBase)
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
            var elements = this.Iteration.Element.ToList<ElementBase>().Union(this.Iteration.Element.SelectMany(e => e.ContainedElement).ToList<ElementBase>());
            var parameterTypeList = new List<ParameterType>();

            foreach (var element in elements)
            {
                switch (element)
                {
                    case ElementDefinition definition:
                    {
                        foreach (var parameter in definition.Parameter)
                        {
                            if (parameterTypeList.Contains(parameter.ParameterType))
                            {
                                continue;
                            }

                            parameterTypeList.Add(parameter.ParameterType);
                            this.ParameterTypeSourceList.Add(new ParameterTypeRowViewModel(parameter.ParameterType));
                        }

                        break;
                    }

                    case ElementUsage usage:
                    {
                        foreach (var parameterOverride in usage.ParameterOverride)
                        {
                            if (parameterTypeList.Contains(parameterOverride.ParameterType))
                            {
                                continue;
                            }

                            parameterTypeList.Add(parameterOverride.ParameterType);
                            this.ParameterTypeSourceList.Add(new ParameterTypeRowViewModel(parameterOverride.ParameterType));
                        }

                        break;
                    }
                }
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
        /// Move parameter types back to target list
        /// </summary>
        protected override void ExecuteMoveToTarget()
        {
            ExecuteMove(this.ParameterTypeSourceList, this.ParameterTypeTargetList, this.SelectedSourceList);
        }
    }
}
