// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportingDataSourceCategory.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.DataSource
{
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Abstract base class from which all category columns for a <see cref="ReportingDataSourceRow"/> need to derive.
    /// </summary>
    public abstract class ReportingDataSourceCategory<T> : ReportingDataSourceColumn<T> where T : ReportingDataSourceRow, new()
    {
        /// <summary>
        /// Gets or sets the associated <see cref="Category"/> short name.
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// Flag indicating whether the associated <see cref="Category"/> is present.
        /// </summary>
        public bool Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportingDataSourceCategory{T}"/> class.
        /// </summary>
        protected ReportingDataSourceCategory()
        {
            this.ShortName = GetParameterAttribute(this.GetType())?.ShortName;
        }

        /// <summary>
        /// Initializes a reported category column based on the corresponding <see cref="ElementBase"/>
        /// within the associated <see cref="ReportingDataSourceNode{T}"/>.
        /// </summary>
        /// <param name="node">
        /// The associated <see cref="ReportingDataSourceNode{T}"/>.
        /// </param>
        internal override void Initialize(ReportingDataSourceNode<T> node)
        {
            this.Node = node;
            this.Value = this.IsMemberOfCategory();
        }

        /// <summary>
        /// Checks if the <see cref="ElementDefinition.Category"/>, or <see cref="ElementUsage.Category"/> contains a <see cref="Category"/>
        /// that has <see cref="ReportingDataSourceCategory{T}.ShortName"/> as its <see cref="Category.ShortName"/>, or somewhere in the
        /// <see cref="Category.SuperCategory"/> hierarchy.
        /// </summary>
        /// <returns>
        /// True if <see cref="Category.ShortName"/> was found, otherwise false.
        /// </returns>
        private bool IsMemberOfCategory()
        {
            var categories =
                this.Node.ElementBase.Cache
                    .Where(x => x.Value.Value.ClassKind == ClassKind.Category)
                    .Select(x => x.Value.Value)
                    .Cast<Category>()
                    .Where(x => x.ShortName == this.ShortName)
                    .ToList();

            foreach (var category in categories)
            {
                if (this.Node.NestedElement.IsMemberOfCategory(category))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
