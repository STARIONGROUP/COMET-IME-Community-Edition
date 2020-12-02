// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryDomainParameterTypeSelectorResult.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed
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

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using System.Collections.Generic;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    /// <summary>
    /// The <see cref="IDialogResult"/> for the <see cref="CategoryDomainParameterTypeSelectorResult"/> dialog
    /// </summary>
    public class CategoryDomainParameterTypeSelectorResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes an instance of the <see cref="ParameteterSubscriptionFilterSelectionResult"/> class
        /// </summary>
        /// <param name="messageBoxResult">
        /// The <see cref="MessageBoxResult"/>
        /// </param>
        /// <param name="isUncategorizedIncluded">
        /// A value indication whether <see cref="CDP4Common.EngineeringModelData.Parameter"/>s contained by <see cref="CDP4Common.EngineeringModelData.ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </param>
        /// <param name="parameterTypes">
        /// the selected instances of <see cref="ParameterType"/>
        /// </param>
        /// <param name="categories">
        /// The selected instances of <see cref="Category"/>
        /// </param>
        /// <param name="domainOfExpertises">
        /// The selected instances of <see cref="DomainOfExpertise"/>
        /// </param>
        public CategoryDomainParameterTypeSelectorResult(bool? messageBoxResult, bool isUncategorizedIncluded, IEnumerable<ParameterType> parameterTypes, IEnumerable<Category> categories, IEnumerable<DomainOfExpertise> domainOfExpertises) : base(messageBoxResult)
        {
            this.IsUncategorizedIncluded = isUncategorizedIncluded;
            this.ParameterTypes = parameterTypes;
            this.Categories = categories;
            this.DomainOfExpertises = domainOfExpertises;
        }

        /// <summary>
        /// Gets or sets a value indication whether <see cref="CDP4Common.EngineeringModelData.Parameter"/>s contained by <see cref="CDP4Common.EngineeringModelData.ElementDefinition"/>s that are
        /// not a member of a <see cref="Category"/> shall be included or not
        /// </summary>
        public bool IsUncategorizedIncluded { get; set; }

        /// <summary>
        /// gets the selected instances of <see cref="ParameterType"/>
        /// </summary>
        public IEnumerable<ParameterType> ParameterTypes { get; private set; }

        /// <summary>
        /// gets the selected instances of <see cref="Category"/>
        /// </summary>
        public IEnumerable<Category> Categories { get; private set; }

        /// <summary>
        /// gets the selected instances of <see cref="DomainOfExpertise"/>
        /// </summary>
        public IEnumerable<DomainOfExpertise> DomainOfExpertises { get; private set; }
    }
}
