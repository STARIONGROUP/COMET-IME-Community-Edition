// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttributeDefinitionMappingRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
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
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Requirements.ReqIFDal;
    using ReactiveUI;
    using ReqIFSharp;

    /// <summary>
    /// The row view model for the <see cref="AttributeDefinition"/> of a <see cref="SpecType"/>
    /// </summary>
    public class AttributeDefinitionMappingRowViewModel : MappingRowViewModelBase<AttributeDefinition>
    {
        /// <summary>
        /// The mapped <see cref="ParameterType"/>
        /// </summary>
        public readonly ParameterType ParameterType;

        /// <summary>
        /// Backing field for <see cref="AttributeDefinitionMapKind"/>
        /// </summary>
        private AttributeDefinitionMapKind attributeDefinitionMapKind;


        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeDefinitionMappingRowViewModel"/> class
        /// </summary>
        /// <param name="attributeDefinition">The <see cref="AttributeDefinition"/></param>
        /// <param name="mappedParameterType">The <see cref="ParameterType"/> associated to this <see cref="AttributeDefinition"/></param>
        /// <param name="refreshValidation">The action that refreshes the IsRequirement property for the other child rows.</param>
        public AttributeDefinitionMappingRowViewModel(AttributeDefinition attributeDefinition, ParameterType mappedParameterType, Action refreshValidation)
            : base(attributeDefinition)
        {
            this.ParameterType = mappedParameterType;
            this.WhenAnyValue(x => x.AttributeDefinitionMapKind).Subscribe(x =>
            {
                this.UpdateIsMapped();
                refreshValidation();
            });

            var defaultMapKind = GetDefaultMapKind(attributeDefinition.LongName);

            if (defaultMapKind != AttributeDefinitionMapKind.NONE)
            {
                this.AttributeDefinitionMapKind = defaultMapKind;
            }
        }

        /// <summary>
        /// Returns the <see cref="AttributeDefinitionMapKind"/> that an <see cref="AttributeDefinition"/> should default to
        /// based on its name. The prostep ivip reserved <c>ReqIF.*</c> names (used by the COMET DOORS/Capella export as
        /// well as by DOORS and Capella themselves) are pre-mapped so that such files import without manual mapping.
        /// </summary>
        /// <param name="longName">The <see cref="AttributeDefinition.LongName"/></param>
        /// <returns>The default <see cref="AttributeDefinitionMapKind"/></returns>
        private static AttributeDefinitionMapKind GetDefaultMapKind(string longName)
        {
            switch (longName)
            {
                case ThingToReqIfMapper.ReqIfNameAttributeDefName:
                    return AttributeDefinitionMapKind.NAME;
                case ThingToReqIfMapper.ReqIfForeignIdAttributeDefName:
                    return AttributeDefinitionMapKind.SHORTNAME;
                case ThingToReqIfMapper.RequirementTextXhtmlAttributeDefName:
                    return AttributeDefinitionMapKind.FIRST_DEFINITION;
                default:
                    return AttributeDefinitionMapKind.NONE;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="AttributeDefinitionMapKind"/> representing the kind of mapping of this <see cref="AttributeDefinitionMappingRowViewModel.Identifiable"/>
        /// </summary>
        public AttributeDefinitionMapKind AttributeDefinitionMapKind
        {
            get { return this.attributeDefinitionMapKind; }
            set { this.RaiseAndSetIfChanged(ref this.attributeDefinitionMapKind, value); }
        }

        /// <summary>
        /// Update the <see cref="AttributeDefinitionMappingRowViewModel.IsMapped"/> property
        /// </summary>
        protected override void UpdateIsMapped()
        {
            this.IsMapped = this.AttributeDefinitionMapKind != AttributeDefinitionMapKind.PARAMETER_VALUE || this.ParameterType != null;
        }
}
}