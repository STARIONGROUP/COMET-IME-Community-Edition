// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionShortNameRule.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Services;

    /// <summary>
    /// A <see cref="BuiltInRule"/> that verifies whether the short-name property of all <see cref="ElementDefinition"/> objects in an <see cref="Iteration"/> is valid.
    /// </summary>
    [BuiltInRuleMetaDataExport("RHEA", "ElementDefinitionShortName", "A rule that verifies whether the shortname property of all ElementDefinition objects in an Iteration is valid")]
    public class ElementDefinitionShortNameRule : BuiltInRule
    {
        public ElementDefinitionShortNameRule()
        {
        }

        /// <summary>
        /// Verify an <see cref="Iteration"/> with respect to a <see cref="Rule"/> 
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is to be verified.
        /// </param>
        /// <returns>
        /// an <see cref="IEnumerable{RuleViolation}"/>, this may be empty of no <see cref="RuleViolation"/>s have been found.
        /// </returns>
        public override IEnumerable<RuleViolation> Verify(Iteration iteration)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException("iteration", "The iteration may not be null");
            }

            if (!iteration.Element.Any())
            {
                return Enumerable.Empty<RuleViolation>();
            }

            var violations = new List<RuleViolation>();

            foreach (var elementDefinition in iteration.Element)
            {
                var validationPass = Regex.IsMatch(elementDefinition.ShortName, @"^[a-zA-Z][a-zA-Z0-9_]*$");
                if (!validationPass)
                {
                    var violation = new RuleViolation(Guid.NewGuid(), elementDefinition.Cache, elementDefinition.IDalUri);
                    violation.Description = "The ShortName must start with a letter and not contain any spaces or non alphanumeric characters.";
                    violation.ViolatingThing.Add(elementDefinition.Iid);

                    violations.Add(violation);
                }
            }

            return violations;
        }
    }
}
