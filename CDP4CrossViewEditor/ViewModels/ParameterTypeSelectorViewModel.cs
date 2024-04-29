﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeSelectorViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4CrossViewEditor.Generator;
    using CDP4CrossViewEditor.RowModels;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The specialized view-model needed to select parameter types for building cross view editor sheet
    /// </summary>
    public sealed class ParameterTypeSelectorViewModel : ThingSelectorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="PowerParametersEnabled"/>
        /// </summary>
        private bool powerParametersEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether the power button is checked
        /// </summary>
        public bool PowerParametersEnabled
        {
            get { return this.powerParametersEnabled; }
            set { this.RaiseAndSetIfChanged(ref this.powerParametersEnabled, value); }
        }

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
        /// Gets/sets the move power parameter types command <see cref="ReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> PowerParametersCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeSelectorViewModel"/> class.
        /// </summary>
        /// <param name="iteration">Current opened iteration <see cref="Iteration"/></param>
        /// <param name="session">Current opened session <see cref="ISession"/></param>
        /// <param name="preservedIids">Current user selection <see cref="List{Guid}"/></param>
        public ParameterTypeSelectorViewModel(Iteration iteration, ISession session, List<Guid> preservedIids)
            : base(iteration, session, ClassKind.ParameterType, preservedIids)
        {
            this.ParameterTypeSourceList = new ReactiveList<ParameterTypeRowViewModel>();

            this.ParameterTypeTargetList = new ReactiveList<ParameterTypeRowViewModel>();

            this.SelectedSourceList = new ReactiveList<ParameterTypeRowViewModel>();

            this.SelectedTargetList = new ReactiveList<ParameterTypeRowViewModel>();

            this.PowerParametersCommand = ReactiveCommandCreator.Create(this.ExecutePowerParametersCommand);
        }

        /// <summary>
        /// Executes the <see cref="PowerParametersCommand"/>
        /// </summary>
        private void ExecutePowerParametersCommand()
        {
            if (this.PowerParametersEnabled)
            {
                this.ExecuteMovePowerToTarget();
            }
            else
            {
                this.ExecuteMovePowerToSource();
            }
        }

        /// <summary>
        /// Get parameter type data from the current iteration
        /// </summary>
        public override void BindData()
        {
            foreach (var element in this.Iteration.Element.Union(this.Iteration.Element.SelectMany(e => e.ContainedElement).AsEnumerable<ElementBase>()))
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

            this.PreserveSelection();
        }

        /// <summary>
        /// Preserve parameter types objects selection from workbook xml parts
        /// </summary>
        private void PreserveSelection()
        {
            if (this.PreservedIids == null)
            {
                return;
            }

            this.SelectedSourceList.Clear();

            this.SelectedSourceList.AddRange(this.PreservedIids
                .Select(iid => this.ParameterTypeSourceList.FirstOrDefault(row => row.Thing.Iid == iid))
                .Where(row => row != null));

            this.ExecuteMoveToTarget();
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
        protected internal override void ExecuteMoveToSource()
        {
            ExecuteMove(this.ParameterTypeTargetList, this.ParameterTypeSourceList, this.SelectedTargetList);

            this.PowerParametersEnabled =
                CrossviewSheetPMeanUtilities.PowerParameters
                    .Select(shortName =>
                        this.ParameterTypeTargetList.FirstOrDefault(row =>
                            string.Equals(row.Thing.ShortName, shortName, StringComparison.InvariantCultureIgnoreCase)))
                    .Count(row => row != null) == CrossviewSheetPMeanUtilities.PowerParameters.Length;
        }

        /// <summary>
        /// Move parameter types back to target list
        /// </summary>
        protected internal override void ExecuteMoveToTarget()
        {
            ExecuteMove(this.ParameterTypeSourceList, this.ParameterTypeTargetList, this.SelectedSourceList);

            this.PowerParametersEnabled =
                CrossviewSheetPMeanUtilities.PowerParameters
                    .Select(shortName =>
                        this.ParameterTypeTargetList.FirstOrDefault(row =>
                            string.Equals(row.Thing.ShortName, shortName, StringComparison.InvariantCultureIgnoreCase)))
                    .Count(row => row != null) == CrossviewSheetPMeanUtilities.PowerParameters.Length;
        }

        /// <summary>
        /// Executes clear selected items command
        /// </summary>
        protected override void ExecuteClear()
        {
            this.SelectedTargetList.Clear();
            this.SelectedTargetList.AddRange(this.ParameterTypeTargetList);

            this.ExecuteMoveToSource();
        }

        /// <summary>
        /// Move power related parameter types to target list
        /// </summary>
        private void ExecuteMovePowerToTarget()
        {
            var powerParameterTypes = CrossviewSheetPMeanUtilities.PowerParameters
                .Select(shortName => this.ParameterTypeSourceList.FirstOrDefault(row => string.Equals(row.Thing.ShortName, shortName, StringComparison.InvariantCultureIgnoreCase)))
                .Where(row => row != null);

            this.SelectedSourceList.Clear();
            this.SelectedSourceList.AddRange(powerParameterTypes);

            this.ExecuteMoveToTarget();
        }

        /// <summary>
        /// Move power related parameter types to source list
        /// </summary>
        private void ExecuteMovePowerToSource()
        {
            var powerParameterTypes = CrossviewSheetPMeanUtilities.PowerParameters
                .Select(shortName => this.ParameterTypeTargetList.FirstOrDefault(row => string.Equals(row.Thing.ShortName, shortName, StringComparison.InvariantCultureIgnoreCase)))
                .Where(row => row != null);

            this.SelectedTargetList.Clear();
            this.SelectedTargetList.AddRange(powerParameterTypes);

            this.ExecuteMoveToSource();
        }
    }
}
