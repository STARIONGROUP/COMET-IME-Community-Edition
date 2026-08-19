// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVRdlCheckResult.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Rdl
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The result of checking a chained <see cref="ReferenceDataLibrary"/> against the <see cref="VandVRdlManifest"/>:
    /// the manifest items that are not yet present anywhere in the chain.
    /// </summary>
    public sealed class VandVRdlCheckResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVRdlCheckResult"/> class.
        /// </summary>
        /// <param name="missingParameterTypes">The <see cref="VandVParameterTypeDefinition"/>s not found in the chain.</param>
        /// <param name="missingCategories">The <see cref="VandVCategoryDefinition"/>s not found in the chain.</param>
        /// <param name="missingParameterizedCategoryRules">The <see cref="VandVParameterizedCategoryRuleDefinition"/>s not found in the chain.</param>
        /// <param name="missingBinaryRelationshipRules">The <see cref="VandVBinaryRelationshipRuleDefinition"/>s not found in the chain.</param>
        /// <param name="requirementTargetCategory">The resolved model requirement <see cref="Category"/> the <c>verifies</c>/<c>validates</c> rules target, or null when none was found.</param>
        public VandVRdlCheckResult(
            IReadOnlyList<VandVParameterTypeDefinition> missingParameterTypes,
            IReadOnlyList<VandVCategoryDefinition> missingCategories,
            IReadOnlyList<VandVParameterizedCategoryRuleDefinition> missingParameterizedCategoryRules,
            IReadOnlyList<VandVBinaryRelationshipRuleDefinition> missingBinaryRelationshipRules,
            Category requirementTargetCategory)
        {
            this.MissingParameterTypes = missingParameterTypes;
            this.MissingCategories = missingCategories;
            this.MissingParameterizedCategoryRules = missingParameterizedCategoryRules;
            this.MissingBinaryRelationshipRules = missingBinaryRelationshipRules;
            this.RequirementTargetCategory = requirementTargetCategory;
        }

        /// <summary>Gets the <see cref="VandVParameterTypeDefinition"/>s not found in the chain.</summary>
        public IReadOnlyList<VandVParameterTypeDefinition> MissingParameterTypes { get; }

        /// <summary>Gets the <see cref="VandVCategoryDefinition"/>s not found in the chain.</summary>
        public IReadOnlyList<VandVCategoryDefinition> MissingCategories { get; }

        /// <summary>Gets the <see cref="VandVParameterizedCategoryRuleDefinition"/>s not found in the chain.</summary>
        public IReadOnlyList<VandVParameterizedCategoryRuleDefinition> MissingParameterizedCategoryRules { get; }

        /// <summary>Gets the <see cref="VandVBinaryRelationshipRuleDefinition"/>s not found in the chain.</summary>
        public IReadOnlyList<VandVBinaryRelationshipRuleDefinition> MissingBinaryRelationshipRules { get; }

        /// <summary>
        /// Gets the model requirement <see cref="Category"/> the <c>verifies</c>/<c>validates</c> rules will target, or
        /// null when the model has no recognisable requirement category (in which case those rules are skipped).
        /// </summary>
        public Category RequirementTargetCategory { get; }

        /// <summary>
        /// Gets a value indicating whether anything that can actually be created is missing and therefore needs to be
        /// seeded. Missing relationship rules only count when a requirement target category exists to bind them to,
        /// otherwise they would be reported forever on a model that has no such category.
        /// </summary>
        public bool HasMissingItems =>
            this.MissingParameterTypes.Any()
            || this.MissingCategories.Any()
            || this.MissingParameterizedCategoryRules.Any()
            || (this.MissingBinaryRelationshipRules.Any() && this.RequirementTargetCategory != null);

        /// <summary>
        /// Builds a human readable, one-item-per-line summary of what is missing, for the confirmation dialog.
        /// </summary>
        /// <returns>A multi-line description of everything that will be created.</returns>
        public string BuildSummary()
        {
            var lines = new List<string>();

            if (this.MissingParameterTypes.Any())
            {
                lines.Add(string.Format("Parameter types ({0}):", this.MissingParameterTypes.Count));
                lines.AddRange(this.MissingParameterTypes.Select(x => string.Format("    • {0} ({1})", x.Name, x.ShortName)));
            }

            if (this.MissingCategories.Any())
            {
                lines.Add(string.Format("Categories ({0}):", this.MissingCategories.Count));
                lines.AddRange(this.MissingCategories.Select(x => string.Format("    • {0} ({1})", x.Name, x.ShortName)));
            }

            if (this.MissingParameterizedCategoryRules.Any())
            {
                lines.Add(string.Format("Rules ({0}):", this.MissingParameterizedCategoryRules.Count));
                lines.AddRange(this.MissingParameterizedCategoryRules.Select(x => string.Format("    • {0} ({1})", x.Name, x.ShortName)));
            }

            if (this.MissingBinaryRelationshipRules.Any())
            {
                if (this.RequirementTargetCategory != null)
                {
                    lines.Add(string.Format("Relationship rules ({0}), targeting requirement category '{1}':", this.MissingBinaryRelationshipRules.Count, this.RequirementTargetCategory.ShortName));
                    lines.AddRange(this.MissingBinaryRelationshipRules.Select(x => string.Format("    • {0} ({1})", x.Name, x.ShortName)));
                }
                else
                {
                    lines.Add("Relationship rules: skipped, no requirement category (e.g. 'REQ'/'REQUIREMENT') found in this model's RDL chain.");
                }
            }

            return string.Join("\n", lines);
        }
    }
}
