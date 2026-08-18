// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVRdlManifest.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CDP4Common.CommonData;

    /// <summary>
    /// The kind of <see cref="CDP4Common.SiteDirectoryData.ParameterType"/> that a V&amp;V attribute requires.
    /// Deliberately restricted to Text/Date/Boolean/Enumeration so the manifest carries no
    /// <see cref="CDP4Common.SiteDirectoryData.MeasurementScale"/> or unit dependencies.
    /// </summary>
    public enum VandVParameterKind
    {
        /// <summary>A <see cref="CDP4Common.SiteDirectoryData.TextParameterType"/></summary>
        Text,

        /// <summary>A <see cref="CDP4Common.SiteDirectoryData.DateParameterType"/></summary>
        Date,

        /// <summary>A <see cref="CDP4Common.SiteDirectoryData.BooleanParameterType"/></summary>
        Boolean,

        /// <summary>An <see cref="CDP4Common.SiteDirectoryData.EnumerationParameterType"/></summary>
        Enumeration
    }

    /// <summary>
    /// Declares one <see cref="CDP4Common.SiteDirectoryData.ParameterType"/> the V&amp;V capability needs.
    /// </summary>
    public sealed class VandVParameterTypeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVParameterTypeDefinition"/> class.
        /// </summary>
        /// <param name="shortName">The unique short-name.</param>
        /// <param name="name">The human readable name.</param>
        /// <param name="kind">The <see cref="VandVParameterKind"/>.</param>
        /// <param name="enumerationValues">The enumeration value names (only used when <paramref name="kind"/> is <see cref="VandVParameterKind.Enumeration"/>).</param>
        public VandVParameterTypeDefinition(string shortName, string name, VandVParameterKind kind, params string[] enumerationValues)
        {
            this.ShortName = shortName;
            this.Name = name;
            this.Kind = kind;
            this.EnumerationValues = enumerationValues ?? new string[0];
        }

        /// <summary>Gets the unique short-name.</summary>
        public string ShortName { get; }

        /// <summary>Gets the human readable name.</summary>
        public string Name { get; }

        /// <summary>Gets the <see cref="VandVParameterKind"/>.</summary>
        public VandVParameterKind Kind { get; }

        /// <summary>Gets the enumeration value names, empty for non-enumeration kinds.</summary>
        public IReadOnlyList<string> EnumerationValues { get; }
    }

    /// <summary>
    /// Declares one <see cref="CDP4Common.SiteDirectoryData.Category"/> the V&amp;V capability needs.
    /// </summary>
    public sealed class VandVCategoryDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVCategoryDefinition"/> class.
        /// </summary>
        /// <param name="shortName">The unique short-name.</param>
        /// <param name="name">The human readable name.</param>
        /// <param name="permissibleClasses">The <see cref="ClassKind"/>s the category may be applied to.</param>
        /// <param name="superCategoryShortName">The short-name of the super-category, or null.</param>
        public VandVCategoryDefinition(string shortName, string name, ClassKind[] permissibleClasses, string superCategoryShortName = null)
        {
            this.ShortName = shortName;
            this.Name = name;
            this.PermissibleClasses = permissibleClasses;
            this.SuperCategoryShortName = superCategoryShortName;
        }

        /// <summary>Gets the unique short-name.</summary>
        public string ShortName { get; }

        /// <summary>Gets the human readable name.</summary>
        public string Name { get; }

        /// <summary>Gets the <see cref="ClassKind"/>s the category may be applied to.</summary>
        public IReadOnlyList<ClassKind> PermissibleClasses { get; }

        /// <summary>Gets the short-name of the super-category, or null when the category has none.</summary>
        public string SuperCategoryShortName { get; }
    }

    /// <summary>
    /// Declares one <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule"/> the V&amp;V capability needs.
    /// </summary>
    public sealed class VandVParameterizedCategoryRuleDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVParameterizedCategoryRuleDefinition"/> class.
        /// </summary>
        /// <param name="shortName">The unique short-name.</param>
        /// <param name="name">The human readable name.</param>
        /// <param name="categoryShortName">The short-name of the <see cref="CDP4Common.SiteDirectoryData.Category"/> the rule applies to.</param>
        /// <param name="parameterTypeShortNames">The short-names of the mandatory <see cref="CDP4Common.SiteDirectoryData.ParameterType"/>s.</param>
        public VandVParameterizedCategoryRuleDefinition(string shortName, string name, string categoryShortName, params string[] parameterTypeShortNames)
        {
            this.ShortName = shortName;
            this.Name = name;
            this.CategoryShortName = categoryShortName;
            this.ParameterTypeShortNames = parameterTypeShortNames;
        }

        /// <summary>Gets the unique short-name.</summary>
        public string ShortName { get; }

        /// <summary>Gets the human readable name.</summary>
        public string Name { get; }

        /// <summary>Gets the short-name of the <see cref="CDP4Common.SiteDirectoryData.Category"/> the rule applies to.</summary>
        public string CategoryShortName { get; }

        /// <summary>Gets the short-names of the mandatory <see cref="CDP4Common.SiteDirectoryData.ParameterType"/>s.</summary>
        public IReadOnlyList<string> ParameterTypeShortNames { get; }
    }

    /// <summary>
    /// Declares one <see cref="CDP4Common.SiteDirectoryData.BinaryRelationshipRule"/> the V&amp;V capability needs. Its
    /// target category is not declared here: it is resolved at seed time to the model's own requirement
    /// <see cref="CDP4Common.SiteDirectoryData.Category"/> (see <see cref="VandVRdlManifest.RequirementCategoryShortNames"/>),
    /// because that category already exists in most models and its short-name varies per model.
    /// </summary>
    public sealed class VandVBinaryRelationshipRuleDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VandVBinaryRelationshipRuleDefinition"/> class.
        /// </summary>
        /// <param name="shortName">The unique short-name.</param>
        /// <param name="name">The human readable name.</param>
        /// <param name="forwardRelationshipName">The forward relationship name (source → target).</param>
        /// <param name="inverseRelationshipName">The inverse relationship name (target → source).</param>
        /// <param name="relationshipCategoryShortName">The short-name of the <see cref="CDP4Common.SiteDirectoryData.Category"/> applied to the relationship itself.</param>
        /// <param name="sourceCategoryShortName">The short-name of the required source <see cref="CDP4Common.SiteDirectoryData.Category"/>.</param>
        public VandVBinaryRelationshipRuleDefinition(string shortName, string name, string forwardRelationshipName, string inverseRelationshipName, string relationshipCategoryShortName, string sourceCategoryShortName)
        {
            this.ShortName = shortName;
            this.Name = name;
            this.ForwardRelationshipName = forwardRelationshipName;
            this.InverseRelationshipName = inverseRelationshipName;
            this.RelationshipCategoryShortName = relationshipCategoryShortName;
            this.SourceCategoryShortName = sourceCategoryShortName;
        }

        /// <summary>Gets the unique short-name.</summary>
        public string ShortName { get; }

        /// <summary>Gets the human readable name.</summary>
        public string Name { get; }

        /// <summary>Gets the forward relationship name (source → target).</summary>
        public string ForwardRelationshipName { get; }

        /// <summary>Gets the inverse relationship name (target → source).</summary>
        public string InverseRelationshipName { get; }

        /// <summary>Gets the short-name of the <see cref="CDP4Common.SiteDirectoryData.Category"/> applied to the relationship itself.</summary>
        public string RelationshipCategoryShortName { get; }

        /// <summary>Gets the short-name of the required source <see cref="CDP4Common.SiteDirectoryData.Category"/>.</summary>
        public string SourceCategoryShortName { get; }
    }

    /// <summary>
    /// The short-names of the <see cref="CDP4Common.SiteDirectoryData.Category"/>s the V&amp;V capability creates and
    /// looks up. Every service, rule and view-model resolves its categories through these constants: the short-names
    /// used to be typed out independently in half a dozen files, where a single typo would compile cleanly and
    /// silently create a disconnected notion of coverage.
    /// </summary>
    public static class VandVCategory
    {
        /// <summary>The category identifying a V&amp;V item.</summary>
        public const string VnVItem = "VnVItem";

        /// <summary>The sub-category identifying a verification item.</summary>
        public const string VerificationItem = "VerificationItem";

        /// <summary>The sub-category identifying a validation item.</summary>
        public const string ValidationItem = "ValidationItem";

        /// <summary>The category identifying a test campaign group.</summary>
        public const string TestCampaign = "TestCampaign";

        /// <summary>The category identifying a stage gate group.</summary>
        public const string StageGateGroup = "StageGateGroup";

        /// <summary>The category identifying a non-conformance report.</summary>
        public const string Ncr = "NCR";

        /// <summary>The category identifying a procedure step.</summary>
        public const string VnVStep = "VnVStep";

        /// <summary>The category of the relationship by which a V&amp;V item verifies a requirement.</summary>
        public const string Verifies = "verifies";

        /// <summary>The category of the relationship by which a V&amp;V item validates a requirement.</summary>
        public const string Validates = "validates";

        /// <summary>The category of the relationship by which a V&amp;V item covers an option.</summary>
        public const string CoversOption = "coversOption";

        /// <summary>The category of the relationship by which a V&amp;V item covers an actual finite state.</summary>
        public const string CoversState = "coversState";

        /// <summary>The category of the relationship by which a V&amp;V item covers a parameter.</summary>
        public const string CoversParameter = "coversParameter";

        /// <summary>The category of the relationship by which a V&amp;V item is verified on an element definition.</summary>
        public const string VerifiedOn = "verifiedOn";

        /// <summary>The category of the relationship by which a V&amp;V item owns a procedure step.</summary>
        public const string HasStep = "hasStep";

        /// <summary>
        /// Gets the categories marking a covering traceability relationship, the ones that make a requirement count as
        /// covered.
        /// </summary>
        public static IReadOnlyList<string> CoverageLinks { get; } = new[] { Verifies, Validates };

        /// <summary>
        /// Gets every category applied to a relationship the V&amp;V capability authors, so a browser can tell a V&amp;V
        /// link apart from an ordinary requirement trace link.
        /// </summary>
        public static IReadOnlyList<string> RelationshipLinks { get; } = new[] { Verifies, Validates, CoversOption, CoversState, CoversParameter, VerifiedOn, HasStep };

        /// <summary>
        /// Gets the categories that must exist before a V&amp;V item can be written. Creating or editing an item is one
        /// user action but several writes (the item, its coverage, its procedure), so all of them are checked up front:
        /// a partially seeded library used to commit the item and then fail on the coverage, leaving an orphan behind.
        /// </summary>
        public static IReadOnlyList<string> RequiredForItemWrite { get; } = new[] { VnVItem, Verifies, Validates, CoversParameter, CoversOption, CoversState, VerifiedOn, VnVStep, HasStep };
    }

    /// <summary>
    /// The single static declaration of every <see cref="CDP4Common.SiteDirectoryData.ParameterType"/>,
    /// <see cref="CDP4Common.SiteDirectoryData.Category"/> and <see cref="CDP4Common.SiteDirectoryData.Rule"/> the
    /// V&amp;V capability needs. The checker (<see cref="VandVRdlService.Check"/>) and the seeder
    /// (<see cref="VandVRdlService.Seed"/>) are both plain functions over these lists, so adding an attribute later
    /// is one list entry, not a code change in two places.
    /// </summary>
    public static class VandVRdlManifest
    {
        /// <summary>
        /// Gets the <see cref="CDP4Common.SiteDirectoryData.ParameterType"/>s the V&amp;V capability needs.
        /// </summary>
        public static IReadOnlyList<VandVParameterTypeDefinition> ParameterTypes { get; } = new[]
        {
            // Planning
            new VandVParameterTypeDefinition("vnv_method", "V&V Method", VandVParameterKind.Enumeration, "Inspection", "Analysis", "Similarity", "Demonstration", "Test", "Review of Design"),
            new VandVParameterTypeDefinition("vnv_stage", "V&V Stage Gate", VandVParameterKind.Enumeration, "SRR", "PDR", "CDR", "TRR", "FAT", "HAT", "SAT", "ORR", "In-Service"),
            new VandVParameterTypeDefinition("vnv_level", "V&V Integration Level", VandVParameterKind.Enumeration, "Equipment", "Subsystem", "System", "System-of-Systems", "Operational"),
            new VandVParameterTypeDefinition("vnv_activity_no", "V&V Activity Number", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_description", "V&V Activity Description", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_preconditions", "V&V Preconditions", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_conditions", "V&V Conditions", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_acceptance", "V&V Acceptance Criteria", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_facility", "V&V Facility", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_responsible_ext", "V&V External Responsible", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_planned_date", "V&V Planned Date", VandVParameterKind.Date),
            new VandVParameterTypeDefinition("vnv_criticality", "V&V Criticality", VandVParameterKind.Enumeration, "Deployment", "Operation", "Mission-critical"),
            new VandVParameterTypeDefinition("vnv_coverage_note", "V&V Coverage Note", VandVParameterKind.Text),

            // Execution
            new VandVParameterTypeDefinition("vnv_status", "V&V Status", VandVParameterKind.Enumeration, "Planned", "Ready", "In Progress", "Executed", "Passed", "Failed", "Waived", "Deviated", "Not Applicable", "Cancelled"),
            new VandVParameterTypeDefinition("vnv_actual_date", "V&V Actual Date", VandVParameterKind.Date),
            new VandVParameterTypeDefinition("vnv_result", "V&V Result", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_evidence_ref", "V&V Evidence Reference", VandVParameterKind.Text),

            // Close-out. ECSS-E-ST-10-02 Annex B requires the compliance status and the close-out status to be
            // recorded separately from the execution status: a test can pass while the requirement is only partly
            // met, which is exactly the case a waiver covers.
            // "Not Assessed" leads deliberately: the dialog defaults to the first value, and a new item must never
            // claim a compliance nobody has judged
            new VandVParameterTypeDefinition("vnv_compliance", "V&V Compliance Status", VandVParameterKind.Enumeration, "Not Assessed", "Compliant", "Partially Compliant", "Non-Compliant", "Not Applicable"),
            new VandVParameterTypeDefinition("vnv_closed", "V&V Closed", VandVParameterKind.Boolean),
            new VandVParameterTypeDefinition("vnv_closeout_reason", "V&V Close-out Reason", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_closed_by", "V&V Closed By", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_closed_on", "V&V Closed On", VandVParameterKind.Date),
            new VandVParameterTypeDefinition("vnv_plan_ref", "V&V Plan Reference", VandVParameterKind.Text),

            // Procedure. ECSS-E-ST-10-03 expects a test procedure of ordered steps, each with what to do, what is
            // expected, and what was actually observed when it was run (the as-run procedure in the test report).
            new VandVParameterTypeDefinition("vnv_procedure_ref", "V&V Procedure Reference", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_step_no", "V&V Step Number", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_step_action", "V&V Step Action", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_step_expected", "V&V Step Expected Result", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_step_actual", "V&V Step Actual Result", VandVParameterKind.Text),
            new VandVParameterTypeDefinition("vnv_step_result", "V&V Step Result", VandVParameterKind.Enumeration, "Not Run", "Pass", "Fail", "Blocked", "Not Applicable")
        };

        /// <summary>
        /// Gets the <see cref="CDP4Common.SiteDirectoryData.Category"/>s the V&amp;V capability needs.
        /// </summary>
        public static IReadOnlyList<VandVCategoryDefinition> Categories { get; } = new[]
        {
            new VandVCategoryDefinition(VandVCategory.VnVItem, "VnV Item", new[] { ClassKind.Requirement }),
            new VandVCategoryDefinition(VandVCategory.VerificationItem, "Verification Item", new[] { ClassKind.Requirement }, VandVCategory.VnVItem),
            new VandVCategoryDefinition(VandVCategory.ValidationItem, "Validation Item", new[] { ClassKind.Requirement }, VandVCategory.VnVItem),
            new VandVCategoryDefinition(VandVCategory.TestCampaign, "Test Campaign", new[] { ClassKind.RequirementsGroup }),
            new VandVCategoryDefinition(VandVCategory.StageGateGroup, "Stage Gate Group", new[] { ClassKind.RequirementsGroup }),
            new VandVCategoryDefinition(VandVCategory.Ncr, "NCR", new[] { ClassKind.ReviewItemDiscrepancy }),
            new VandVCategoryDefinition(VandVCategory.VnVStep, "VnV Procedure Step", new[] { ClassKind.Requirement }),

            // Categories applied to the traceability BinaryRelationships (governing rules are deferred, see VandVRdlService).
            new VandVCategoryDefinition(VandVCategory.Verifies, "verifies", new[] { ClassKind.BinaryRelationship }),
            new VandVCategoryDefinition(VandVCategory.Validates, "validates", new[] { ClassKind.BinaryRelationship }),
            new VandVCategoryDefinition(VandVCategory.CoversOption, "covers option", new[] { ClassKind.BinaryRelationship }),
            new VandVCategoryDefinition(VandVCategory.CoversState, "covers state", new[] { ClassKind.BinaryRelationship }),
            new VandVCategoryDefinition(VandVCategory.CoversParameter, "covers parameter", new[] { ClassKind.BinaryRelationship }),
            new VandVCategoryDefinition(VandVCategory.VerifiedOn, "verified on", new[] { ClassKind.BinaryRelationship }),
            new VandVCategoryDefinition(VandVCategory.HasStep, "has step", new[] { ClassKind.BinaryRelationship })
        };

        /// <summary>
        /// Gets the <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule"/>s the V&amp;V capability needs.
        /// A single rule makes <c>vnv_method</c>, <c>vnv_stage</c>, <c>vnv_acceptance</c> and <c>vnv_status</c> mandatory
        /// on every <c>VnV Item</c>, so incomplete items surface in the existing Rule Verification browser with no code.
        /// </summary>
        public static IReadOnlyList<VandVParameterizedCategoryRuleDefinition> ParameterizedCategoryRules { get; } = new[]
        {
            new VandVParameterizedCategoryRuleDefinition("VnVItemAttributesRule", "V&V Item mandatory attributes", VandVCategory.VnVItem, "vnv_method", "vnv_stage", "vnv_acceptance", "vnv_status")
        };

        /// <summary>
        /// Gets the <see cref="CDP4Common.SiteDirectoryData.BinaryRelationshipRule"/>s the V&amp;V capability needs to
        /// govern the traceability links. Each is seeded only when a requirement target category can be resolved in the
        /// model (see <see cref="RequirementCategoryShortNames"/>); a rule with a <c>VnV Item</c> source category and a
        /// <c>Requirement</c> target category accepts requirements categorized with any sub-category of that target,
        /// because category checking is super-category aware.
        /// </summary>
        public static IReadOnlyList<VandVBinaryRelationshipRuleDefinition> BinaryRelationshipRules { get; } = new[]
        {
            new VandVBinaryRelationshipRuleDefinition("verifiesRule", "verifies (V&V item verifies a requirement)", VandVCategory.Verifies, "is verified by", VandVCategory.Verifies, VandVCategory.VnVItem),
            new VandVBinaryRelationshipRuleDefinition("validatesRule", "validates (V&V item validates a requirement)", VandVCategory.Validates, "is validated by", VandVCategory.Validates, VandVCategory.VnVItem)
        };

        /// <summary>
        /// Gets the short-names (case-insensitive) by which the model's existing requirement
        /// <see cref="CDP4Common.SiteDirectoryData.Category"/> is recognised, so the <c>verifies</c>/<c>validates</c>
        /// rules can target it without the manifest hard-coding a single model's convention.
        /// </summary>
        public static IReadOnlyList<string> RequirementCategoryShortNames { get; } = new[] { "REQUIREMENT", "REQ" };

        /// <summary>
        /// Derives a valid <see cref="CDP4Common.CommonData.DefinedThing.ShortName"/> from a human readable value name
        /// (spaces and hyphens become underscores, any other non-word character is dropped).
        /// </summary>
        /// <param name="value">The human readable value.</param>
        /// <returns>A short-name safe for an <see cref="CDP4Common.SiteDirectoryData.EnumerationValueDefinition"/>.</returns>
        public static string ToShortName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("The value may not be null or empty", nameof(value));
            }

            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                if (character == ' ' || character == '-')
                {
                    builder.Append('_');
                }
                else if (char.IsLetterOrDigit(character) || character == '_')
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }
    }
}
