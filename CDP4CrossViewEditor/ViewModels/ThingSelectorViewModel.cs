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
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using CDP4CrossViewEditor.RowModels;

    using ReactiveUI;

    /// <summary>
    /// The view-model to select cross view editor for elements and for parameters
    /// </summary>
    public class ThingSelectorViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="ElementDefinitionSourceList"/> property
        /// </summary>
        private ReactiveList<ElementDefinitionRowViewModel> elementDefinitionSourceList;

        /// <summary>
        /// Backing field for the <see cref="ElementDefinitionTargetList"/> property
        /// </summary>
        private ReactiveList<ElementDefinitionRowViewModel> elementDefinitionTargetList;

        /// <summary>
        /// Backing field for the <see cref="ParameterTypeSourceList"/> property
        /// </summary>
        private ReactiveList<ParameterTypeRowViewModel> parameterTypeSourceList;

        /// <summary>
        /// Backing field for the <see cref="ParameterTypeTargetList"/> property
        /// </summary>
        private ReactiveList<ParameterTypeRowViewModel> parameterTypeTargetList;

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
        /// Gets or sets source element list
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> ElementDefinitionSourceList
        {
            get => this.elementDefinitionSourceList;
            private set => this.RaiseAndSetIfChanged(ref this.elementDefinitionSourceList, value);
        }

        /// <summary>
        /// Gets or sets target element list
        /// </summary>
        public ReactiveList<ElementDefinitionRowViewModel> ElementDefinitionTargetList
        {
            get => this.elementDefinitionTargetList;
            private set => this.RaiseAndSetIfChanged(ref this.elementDefinitionTargetList, value);
        }

        /// <summary>
        /// Gets or sets source parameter list
        /// </summary>
        public ReactiveList<ParameterTypeRowViewModel> ParameterTypeSourceList
        {
            get => this.parameterTypeSourceList;
            private set => this.RaiseAndSetIfChanged(ref this.parameterTypeSourceList, value);
        }

        /// <summary>
        /// Gets or sets target parameter list
        /// </summary>
        public ReactiveList<ParameterTypeRowViewModel> ParameterTypeTargetList
        {
            get => this.parameterTypeTargetList;
            private set => this.RaiseAndSetIfChanged(ref this.parameterTypeTargetList, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingSelectorViewModel"/> class.
        /// </summary>
        /// <param name="iteration">Current opened iteration <see cref="Iteration"/> </param>
        /// <param name="thingClassKind">Current type of thing <see cref="ClassKind"/></param>
        public ThingSelectorViewModel(Iteration iteration, ClassKind thingClassKind)
        {
            this.Iteration = iteration;
            this.ThingClassKind = thingClassKind;

            this.ElementDefinitionSourceList = new ReactiveList<ElementDefinitionRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.ElementDefinitionTargetList = new ReactiveList<ElementDefinitionRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.ParameterTypeSourceList = new ReactiveList<ParameterTypeRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.ParameterTypeTargetList = new ReactiveList<ParameterTypeRowViewModel>
            {
                ChangeTrackingEnabled = true
            };

            this.AddSubscriptions();
        }

        /// <summary>
        /// Bind source data depends on thing type <see cref="ClassKind"/>
        /// </summary>
        public void BindData()
        {
            switch (this.ThingClassKind)
            {
                case ClassKind.ElementBase:
                    this.BindElements();
                    break;

                case ClassKind.ParameterBase:
                    this.BindParameters();
                    break;
            }
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
            this.MoveItemsDown.Subscribe(_ => this.ExecuteMoveItemsDown());

            this.ClearItems = ReactiveCommand.Create();
            this.ClearItems.Subscribe(_ => this.ExecuteClearItems());

            this.SortItems = ReactiveCommand.Create();
            this.SortItems.Subscribe(_ => this.ExecuteSortItems());
        }

        /// <summary>
        /// Executes move to source command <see cref="MoveItemsToSource"/>
        /// Things will be moved back to selection source
        /// </summary>
        private void ExecuteMoveToSource()
        {
            // TODO #625: Implement ordering and sorting support for selected elements
        }

        /// <summary>
        /// Executes move to target command <see cref="MoveItemsToTarget"/>
        /// Things will be selected for Cross View Editor table/report
        /// </summary>
        private void ExecuteMoveToTarget()
        {
            // TODO #625: Implement ordering and sorting support for selected elements
        }

        /// <summary>
        /// Executes move items command <see cref="MoveItemsUp"/>
        /// Move selected <see cref="Thing"/> one position up inside selection list
        /// </summary>
        private void ExecuteMoveUp()
        {
            // TODO #625: Implement ordering and sorting support for selected elements
        }

        /// <summary>
        /// Executes move items command <see cref="MoveItemsDown"/>
        /// Move selected <see cref="Thing"/> one position down inside selection list
        /// </summary>
        private void ExecuteMoveItemsDown()
        {
            // TODO #625: Implement ordering and sorting support for selected elements
        }

        /// <summary>
        /// Executes clear selected items command <see cref="ClearItems"/>
        /// </summary>
        private void ExecuteClearItems()
        {
            // TODO #623 Implement core functionality
        }

        /// <summary>
        /// Executes alphabetical sort items command <see cref="SortItems"/>
        /// </summary>
        private void ExecuteSortItems()
        {
            // TODO #624 Implement filtering for initially displayed elements
        }

        /// <summary>
        /// Bind element definition/element usage list based on the selected category
        /// </summary>
        private void BindElements()
        {
            // TODO #623 Implement core functionality
            var de = new DomainOfExpertise
            {
                Iid = new Guid(),
                Name = "Domain1",
                ShortName = "DomainOfExpertise1",
            };

            for (var i = 0; i < 10; i++)
            {
                this.ElementDefinitionSourceList.Add(new ElementDefinitionRowViewModel(new ElementDefinition
                {
                    Iid = new Guid(),
                    Name = $"ElementDefinition{i}",
                    ShortName = $"ED{i}",
                    Owner = de
                }));
            }
        }

        /// <summary>
        /// Bind parameter type list which are linked to the elements list
        /// </summary>
        private void BindParameters()
        {
            // TODO #623 Implement core functionality
            var parameterDictionary = new Dictionary<string, ClassKind>()
            {
                { "m", ClassKind.SimpleQuantityKind },
                { "acceleration", ClassKind.SimpleQuantityKind },
                { "absorptance", ClassKind.SpecializedQuantityKind },
                {"ang_random_walk", ClassKind.SimpleQuantityKind },
                {"area", ClassKind.DerivedQuantityKind }
            };

            for (var i = 0; i < parameterDictionary.Count; i++)
            {
                ParameterType parameterType = null;

                switch (parameterDictionary.Values.ToList()[i])
                {
                    case ClassKind.SimpleQuantityKind:
                        parameterType = new SimpleQuantityKind();
                        break;

                    case ClassKind.SpecializedQuantityKind:
                        parameterType = new SpecializedQuantityKind();
                        break;

                    case ClassKind.DerivedQuantityKind:
                        parameterType = new SpecializedQuantityKind();
                        break;
                }

                if (parameterType == null)
                {
                    continue;
                }

                parameterType.Iid = Guid.NewGuid();
                parameterType.Name = parameterDictionary.Keys.ToList()[i];
                parameterType.ShortName = parameterDictionary.Keys.ToList()[i];

                this.ParameterTypeSourceList.Add(new ParameterTypeRowViewModel(parameterType));
            }
        }
    }
}
