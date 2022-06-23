// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DatatypeDefinitionMappingRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    using ReqIFSharp;

    /// <summary>
    /// The row-view-model for the datatype-definition to parameter type mapping
    /// </summary>
    public class DatatypeDefinitionMappingRowViewModel : MappingRowViewModelBase<DatatypeDefinition>
    {
        /// <summary>
        /// All the <see cref="ParameterType"/>
        /// </summary>
        private readonly ReactiveList<ParameterType> allParameterType;

        /// <summary>
        /// Backing field for <see cref="MappedThing"/>
        /// </summary>
        private ParameterType mappedThing;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatatypeDefinitionMappingRowViewModel"/> class
        /// </summary>
        /// <param name="datatypeDefinition">The <see cref="DatatypeDefinition"/></param>
        /// <param name="parameterTypes">The <see cref="CDP4Common.SiteDirectoryData.ParameterType"/>s available</param>
        public DatatypeDefinitionMappingRowViewModel(DatatypeDefinition datatypeDefinition, ReactiveList<ParameterType> parameterTypes) : base(datatypeDefinition)
        {
            this.allParameterType = parameterTypes;
            this.PossibleThings = new ReactiveList<ParameterType>();
            this.EnumValue = new ReactiveList<EnumValueMappingRowViewModel>();

            this.FilterPossibleParameterTypes();
            this.PopulateEnumValues();

            this.WhenAnyValue(x => x.MappedThing).Subscribe(_ =>
            {
                this.RefreshEnumValuePossibleList();
            });
        }

        /// <summary>
        /// Gets or sets the mapped <see cref="Thing"/>
        /// </summary>
        public ParameterType MappedThing
        {
            get { return this.mappedThing; }
            set { this.RaiseAndSetIfChanged(ref this.mappedThing, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="ParameterType"/>
        /// </summary>
        public ReactiveList<ParameterType> PossibleThings { get; private set; }

        /// <summary>
        /// Gets the <see cref="EnumValue"/> mapping rows
        /// </summary>
        public ReactiveList<EnumValueMappingRowViewModel> EnumValue { get; private set; } 

        /// <summary>
        /// Filter the possible <see cref="ParameterType"/>s for this row
        /// </summary>
        public void FilterPossibleParameterTypes()
        {
            this.PossibleThings.Clear();
            var possibleParameterType = new List<ParameterType>();

            if (this.Identifiable is DatatypeDefinitionEnumeration)
            {
                var valueDefinitionAmount = ((DatatypeDefinitionEnumeration)this.Identifiable).SpecifiedValues.Count;
                possibleParameterType.AddRange(this.allParameterType.OfType<EnumerationParameterType>()
                    .Where(pt => pt.ValueDefinition.Count.Equals(valueDefinitionAmount)));
            }
            else if (this.Identifiable is DatatypeDefinitionDate)
            {
                possibleParameterType.AddRange(this.allParameterType.OfType<DateParameterType>());
                possibleParameterType.AddRange(this.allParameterType.OfType<DateTimeParameterType>());
                possibleParameterType.AddRange(this.allParameterType.OfType<TimeOfDayParameterType>());
            }
            else if (this.Identifiable is DatatypeDefinitionBoolean)
            {
                possibleParameterType.AddRange(this.allParameterType.OfType<BooleanParameterType>());
            }
            else
            {
                possibleParameterType.AddRange(this.allParameterType.OfType<TextParameterType>());
                possibleParameterType.AddRange(this.allParameterType.OfType<SimpleQuantityKind>());
                possibleParameterType.AddRange(this.allParameterType.OfType<DerivedQuantityKind>());
                possibleParameterType.AddRange(this.allParameterType.OfType<SpecializedQuantityKind>());
            }

            this.PossibleThings.AddRange(possibleParameterType.OrderBy(x => x.Name));
        }

        /// <summary>
        /// Update the mapping status
        /// </summary>
        public void UpdateMappingStatus()
        {
            this.UpdateIsMapped();
        }

        /// <summary>
        /// Populate the enum Values
        /// </summary>
        private void PopulateEnumValues()
        {
            var enumDatatype = this.Identifiable as DatatypeDefinitionEnumeration;
            if (enumDatatype == null)
            {
                return;
            }

            foreach (var enumValue in enumDatatype.SpecifiedValues)
            {
                this.EnumValue.Add(new EnumValueMappingRowViewModel(enumValue, this));
            }
        }

        /// <summary>
        /// Refresh the possible <see cref="EnumerationValueDefinition"/>
        /// </summary>
        private void RefreshEnumValuePossibleList()
        {
            if (this.EnumValue == null)
            {
                return;
            }

            foreach (var row in this.EnumValue)
            {
                row.PopulatePossibleEnumValueDefinition();
            }
        }

        /// <summary>
        /// Update <see cref="DatatypeDefinitionMappingRowViewModel.IsMapped"/>
        /// </summary>
        /// <remarks>
        /// The <see cref="DatatypeDefinition"/> may be mapped to nothing if it is not used in any mapped Parameter-Value
        /// </remarks>
        protected override void UpdateIsMapped()
        {
            this.IsMapped = true;

            if (this.EnumValue == null)
            {
                return;
            }

            // Verify that all litterals are mapped to a unique EnumValueDefinition
            var enumValueCount = this.EnumValue.Count;
            if (enumValueCount == 0)
            {
                return;
            }

            var distinctMappedEnum = this.EnumValue.Select(x => x.MappedThing).Distinct().Count();
            this.IsMapped = this.IsMapped && (enumValueCount == distinctMappedEnum);
        }
    }
}