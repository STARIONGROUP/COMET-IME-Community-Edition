// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition. 
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;
    using CDP4Composition.ViewModels;

    using ReactiveUI;

    /// <summary>
    /// the row-view-model representing a Value of the <see cref="RelationalExpression"/> that contains it.
    /// </summary>
    public class RelationalExpressionValueRowViewModel : PrimitiveRow<string>, IValueSetRow
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// The <see cref="ParameterType"/> of the container <see cref="RelationalExpression"/>
        /// </summary>
        private ParameterType parameterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalExpressionValueRowViewModel"/>
        /// </summary>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that is represented by the view-model
        /// </param>
        public RelationalExpressionValueRowViewModel(ParameterType parameterType)
        {
            this.parameterType = parameterType;
            this.ParameterTypeClassKind = parameterType.ClassKind;
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> represented by this <see cref="IValueSetRow"/>
        /// </summary>
        public ClassKind ParameterTypeClassKind { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ParameterType"/> of this <see cref="Parameter"/> is a <see cref="EnumerationParameterType"/>
        /// </summary>
        public bool IsMultiSelect
        {
            get
            {
                var enumPt = this.parameterType as EnumerationParameterType;
                if (enumPt == null)
                {
                    return false;
                }

                return enumPt.AllowMultiSelect;
            }
        }

        /// <summary>
        /// Gets the list of possible <see cref="EnumerationValueDefinition"/> for this <see cref="ParameterType"/>
        /// </summary>
        public ReactiveList<EnumerationValueDefinition> EnumerationValueDefinition
        {
            get
            {
                var enumValues = new ReactiveList<EnumerationValueDefinition>();

                var enumPt = this.parameterType as EnumerationParameterType;
                if (enumPt != null)
                {
                    enumValues.AddRange(enumPt.ValueDefinition);
                }

                return enumValues;
            }
        }
    }
}
