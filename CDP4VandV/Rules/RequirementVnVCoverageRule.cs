// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementVnVCoverageRule.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Services;

    /// <summary>
    /// A <see cref="BuiltInRule"/> that flags every requirement in an <see cref="Iteration"/> that is not the target of
    /// a <c>verifies</c> or <c>validates</c> <see cref="BinaryRelationship"/> from a V&amp;V item. This is the coverage
    /// check the seeded <see cref="BinaryRelationshipRule"/>s structurally cannot do, since they only validate the
    /// relationships that exist and cannot detect the absence of one.
    /// </summary>
    [BuiltInRuleMetaDataExport("STARION", "RequirementVnVCoverage", "A rule that flags requirements not covered by a 'verifies' or 'validates' relationship from a V&V item")]
    public class RequirementVnVCoverageRule : BuiltInRule
    {
        /// <summary>
        /// The short-names (mirroring <see cref="Rdl.VandVRdlManifest"/>) of the categories applied to a covering
        /// traceability relationship.
        /// </summary>
        private static readonly string[] CoverageCategoryShortNames = { "verifies", "validates" };

        /// <summary>
        /// The short-name (mirroring <see cref="Rdl.VandVRdlManifest"/>) of the category identifying a V&amp;V item, so
        /// V&amp;V items are not themselves flagged as uncovered requirements.
        /// </summary>
        private const string VnVItemCategoryShortName = "VnVItem";

        /// <summary>
        /// Verify an <see cref="Iteration"/> with respect to requirement V&amp;V coverage.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> that is to be verified.</param>
        /// <returns>An <see cref="IEnumerable{RuleViolation}"/>, one per uncovered requirement; empty when all are covered.</returns>
        public override IEnumerable<RuleViolation> Verify(Iteration iteration)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration), "The iteration may not be null");
            }

            var coveredTargetIids = new HashSet<Guid>(
                iteration.Relationship
                    .OfType<BinaryRelationship>()
                    .Where(relationship =>
                        relationship.Target != null
                        && relationship.Source is Requirement source
                        && IsCategorizedAs(source, new[] { VnVItemCategoryShortName })
                        && IsCategorizedAs(relationship, CoverageCategoryShortNames))
                    .Select(relationship => relationship.Target.Iid));

            var violations = new List<RuleViolation>();

            foreach (var specification in iteration.RequirementsSpecification.Where(x => !x.IsDeprecated && x.ShortName != Services.VandVItemCreator.VandVSpecificationShortName))
            {
                foreach (var requirement in specification.Requirement.Where(x => !x.IsDeprecated))
                {
                    if (IsCategorizedAs(requirement, new[] { VnVItemCategoryShortName })
                        || Services.VandVProcedureWriter.IsStep(requirement))
                    {
                        continue;
                    }

                    if (coveredTargetIids.Contains(requirement.Iid))
                    {
                        continue;
                    }

                    var violation = new RuleViolation(Guid.NewGuid(), requirement.Cache, requirement.IDalUri)
                    {
                        Description = string.Format("The requirement '{0}' is not covered by any 'verifies' or 'validates' relationship from a V&V item.", requirement.ShortName)
                    };

                    violation.ViolatingThing.Add(requirement.Iid);
                    violations.Add(violation);
                }
            }

            return violations;
        }

        /// <summary>
        /// Asserts whether a <see cref="ICategorizableThing"/> carries any of the supplied category short-names, directly
        /// or through a super-category.
        /// </summary>
        /// <param name="thing">The <see cref="ICategorizableThing"/> to inspect.</param>
        /// <param name="shortNames">The category short-names to match.</param>
        /// <returns>true when the thing is categorized by any of the <paramref name="shortNames"/>.</returns>
        private static bool IsCategorizedAs(ICategorizableThing thing, IReadOnlyCollection<string> shortNames)
        {
            return thing.Category.Any(category =>
                shortNames.Contains(category.ShortName)
                || category.AllSuperCategories().Any(super => shortNames.Contains(super.ShortName)));
        }
    }
}
