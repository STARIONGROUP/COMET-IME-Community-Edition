// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementParameterRowControlViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.ViewModels
{
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Utilities;

    using ReactiveUI;

    /// <summary>
    /// Represents an element definition or usage to be use with <see cref="Views.ElementParameterRowControl"/>
    /// </summary>
    public class ElementParameterRowControlViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Owner"/> property
        /// </summary>
        private string owner;

        /// <summary>
        /// Backing field for <see cref="category"/> property
        /// </summary>
        private string category;

        /// <summary>
        /// Backing field for <see cref="modelCode"/> property
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Backing field for <see cref="definition"/> property
        /// </summary>
        private string definition;

        /// <summary>
        /// Gets the Parameter Collection of this represented Element
        /// </summary>
        public ReactiveList<ParameterRowControlViewModel> Parameters { get; } = new ReactiveList<ParameterRowControlViewModel>();

        /// <summary>
        /// Gets the Actual Option
        /// </summary>
        public Option ActualOption { get; private set; }

        /// <summary>
        /// Gets the represented element Option
        /// </summary>
        public ElementBase Element { get; private set; }

        /// <summary>
        /// Gets this represented element owner
        /// </summary>
        public string Owner
        {
            get => this.owner;
            private set => this.RaiseAndSetIfChanged(ref this.owner, value);
        }

        /// <summary>
        /// Gets this represented element Categories
        /// </summary>
        public string Category
        {
            get => this.category;
            private set => this.RaiseAndSetIfChanged(ref this.category, value);
        }

        /// <summary>
        /// Gets this represented element Model Code
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            private set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// Gets this represented element Model Code
        /// </summary>
        public string Definition
        {
            get => this.definition;
            private set => this.RaiseAndSetIfChanged(ref this.definition, value);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ElementParameterRowControlViewModel"/>
        /// </summary>
        /// <param name="element">The represented element</param>
        /// <param name="option">The actual option</param>
        public ElementParameterRowControlViewModel(ElementBase element, Option option)
        {
            this.ActualOption = option;
            this.Element = element;
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Update and set all this view model properties
        /// </summary>
        public void UpdateProperties()
        {
            if (this.ActualOption == null)
            {
                return;
            }

            switch (this.Element)
            {
                case ElementUsage elementUsage:
                    this.Parameters.AddRange(elementUsage.ParameterOverride.Select(p => new ParameterRowControlViewModel(p, this.ActualOption)));
                    this.Parameters.AddRange(elementUsage.ElementDefinition.Parameter.Select(p => new ParameterRowControlViewModel(p, this.ActualOption)));
                    this.ModelCode = elementUsage.ModelCode();
                    break;
                case ElementDefinition elementDefinition:
                    this.Parameters.AddRange(elementDefinition.Parameter.Select(p => new ParameterRowControlViewModel(p, this.ActualOption)));
                    this.ModelCode = elementDefinition.ModelCode();
                    break;
            }

            this.Owner = this.Element.Owner != null ? this.Element.Owner.ShortName : "NA";
            var categories = this.Element.GetAllCategoryShortNames();
            this.Category = string.IsNullOrWhiteSpace(categories) ? "-" : categories;

            var firstDefinitionFound = this.Element.Definition.FirstOrDefault();
            this.Definition = firstDefinitionFound != null && !string.IsNullOrWhiteSpace(firstDefinitionFound.Content) ? $"Definition [{firstDefinitionFound.LanguageCode}]: {firstDefinitionFound.Content}" : ": -";
        }
    }
}
