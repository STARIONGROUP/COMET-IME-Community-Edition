// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeComponentRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary, Jaime Bernar
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The row view model that represents a <see cref="ParameterTypeComponent"/>
    /// </summary>
    public class ParameterTypeComponentRowViewModel : CDP4CommonView.ParameterTypeComponentRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="Coordinates"/> property.
        /// </summary>
        private string coordinates;

        /// <summary>
        /// Backing field for the <see cref="SelectedFilter"/> property.
        /// </summary>
        private string selectedFilter;

        /// <summary>
        /// All possible ParameterTypes that are used to populate list of filtered ParameterType.
        /// </summary>
        private readonly ReactiveList<ParameterType> possibleParameterTypes;
            
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeComponentRowViewModel"/> class
        /// </summary>
        /// <param name="component">The <see cref="ParameterTypeComponent"/> represented</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The viewmodel that contains this row</param>
        public ParameterTypeComponentRowViewModel(ParameterTypeComponent component, ISession session, CompoundParameterTypeDialogViewModel containerViewModel)
            : base(component, session, containerViewModel)
        {
            this.PossibleScale = new ReactiveList<MeasurementScale>();
            this.FilteringOptions = new Dictionary<string, Type>();
            this.FilteringOptions.Add(string.Empty, null);

            foreach (var type in TypeResolver.GetDerivedTypes(typeof(ParameterType), typeof(ParameterType).Assembly).Where(x => !x.IsAbstract))
            {
                this.FilteringOptions.Add(type.Name, type);
            }

            this.possibleParameterTypes = containerViewModel.PossibleParameterType;
            this.PossibleParameterType = new ReactiveList<ParameterType>();

            this.PossibleParameterType.AddRange(this.possibleParameterTypes);

            this.WhenAnyValue(x => x.ParameterType).Subscribe(_ => { this.PopulatePossibleScale(); containerViewModel.UpdateOkCanExecuteStatus();});
            this.IsReadOnly = containerViewModel.IsReadOnly;

            this.SelectedFilter = null;
            this.WhenAnyValue(x => x.SelectedFilter).Subscribe(_ => this.FilterPossibleParameterType());
        }

        /// <summary>
        /// Gets or sets a value indicating whether this row is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the SelectedFilter
        /// </summary>
        public string SelectedFilter
        {
            get => this.selectedFilter;
            set => this.RaiseAndSetIfChanged(ref this.selectedFilter, value);
        }

        /// <summary>
        /// Gets or sets the coordinates of a component
        /// </summary>
        public string Coordinates
        {
            get => this.coordinates;
            set => this.RaiseAndSetIfChanged(ref this.coordinates, value);
        }

        /// <summary>
        /// Gets categories by which PossibleParameterTypes can be filtered
        /// </summary>
        public Dictionary<string, Type> FilteringOptions { get; set; }

        /// <summary>
        /// Gets all the possible <see cref="ParameterType"/> for this <see cref="ParameterTypeComponent"/>
        /// </summary>
        public ReactiveList<ParameterType> PossibleParameterType { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="MeasurementScale"/> 
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleScale { get; private set; }

        /// <summary>
        /// Apply selected filter to PossibleParameterType
        /// </summary>
        private void FilterPossibleParameterType()
        {
            if (!string.IsNullOrEmpty(this.SelectedFilter))
            {
                this.ParameterType = null;

                this.PossibleParameterType.Clear();
                var filteredList = this.possibleParameterTypes.Where(x => x.GetType() == this.FilteringOptions[this.SelectedFilter]);
                this.PossibleParameterType.AddRange(filteredList);
            }
            else
            {
                this.PossibleParameterType.Clear();
                this.PossibleParameterType.AddRange(this.possibleParameterTypes);
            }
        }

        /// <summary>
        /// Populate the possible scale for this <see cref="ParameterTypeComponent"/>
        /// </summary>
        private void PopulatePossibleScale()
        {
            this.PossibleScale.Clear();

            var qt = this.ParameterType as QuantityKind;

            if (qt == null)
            {
                this.Scale = null;
                return;
            }

            this.PossibleScale.AddRange(qt.AllPossibleScale);
            
            if (this.PossibleScale.Count > 0)
            {
                this.Scale = this.Thing.Scale ?? qt.DefaultScale ?? this.PossibleScale.First();
            }
        }

        /// <summary>
        /// The indexer used for validation
        /// </summary>
        /// <param name="columnName">The name of the property to validate</param>
        /// <returns>An error message if any.</returns>
        public override string this[string columnName]
        {
            get
            {
                if (columnName == "ShortName")
                {
                    var validationResult = this.ValidationService.ValidateObjectProperty(columnName, this);
                    this.ErrorMsg = validationResult;
                    ((CompoundParameterTypeDialogViewModel)this.ContainerViewModel).UpdateOkCanExecuteStatus();
                    return validationResult != null ? validationResult : string.Empty;
                }

                return string.Empty;
            }
        }
    }
}