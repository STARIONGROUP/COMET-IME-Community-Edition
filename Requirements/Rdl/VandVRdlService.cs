// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVRdlService.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// Checks a chained <see cref="ReferenceDataLibrary"/> against the <see cref="VandVRdlManifest"/> and seeds whatever
    /// is missing into a <see cref="ModelReferenceDataLibrary"/> in a single <see cref="ThingTransaction"/>.
    /// </summary>
    /// <remarks>
    /// Both operations are plain functions over the manifest. The check walks the whole RDL chain, so anything already
    /// present (in the Model RDL or higher, e.g. a Site RDL curated by an admin) is reused and never re-created.
    /// </remarks>
    public class VandVRdlService
    {
        /// <summary>
        /// Determines which <see cref="VandVRdlManifest"/> items are not present anywhere in the chain of the supplied
        /// <paramref name="rdl"/>.
        /// </summary>
        /// <param name="rdl">The <see cref="ReferenceDataLibrary"/> whose chain is inspected (typically the model's <see cref="ModelReferenceDataLibrary"/>).</param>
        /// <returns>A <see cref="VandVRdlCheckResult"/> listing everything that is missing.</returns>
        public VandVRdlCheckResult Check(ReferenceDataLibrary rdl)
        {
            if (rdl == null)
            {
                throw new ArgumentNullException(nameof(rdl));
            }

            var existingParameterTypes = new HashSet<string>(rdl.QueryParameterTypesFromChainOfRdls().Select(x => x.ShortName));
            var existingCategories = new HashSet<string>(rdl.QueryCategoriesFromChainOfRdls().Select(x => x.ShortName));
            var existingRules = new HashSet<string>(rdl.QueryRulesFromChainOfRdls().Select(x => x.ShortName));

            var missingParameterTypes = VandVRdlManifest.ParameterTypes.Where(x => !existingParameterTypes.Contains(x.ShortName)).ToList();
            var missingCategories = VandVRdlManifest.Categories.Where(x => !existingCategories.Contains(x.ShortName)).ToList();
            var missingRules = VandVRdlManifest.ParameterizedCategoryRules.Where(x => !existingRules.Contains(x.ShortName)).ToList();
            var missingBinaryRelationshipRules = VandVRdlManifest.BinaryRelationshipRules.Where(x => !existingRules.Contains(x.ShortName)).ToList();
            var requirementTargetCategory = ResolveRequirementCategory(rdl);

            return new VandVRdlCheckResult(missingParameterTypes, missingCategories, missingRules, missingBinaryRelationshipRules, requirementTargetCategory);
        }

        /// <summary>
        /// Resolves the model's existing requirement <see cref="Category"/>, the target for the <c>verifies</c>/
        /// <c>validates</c> rules, by matching, case-insensitively and in priority order, the short-names in
        /// <see cref="VandVRdlManifest.RequirementCategoryShortNames"/> among the categories permissible on
        /// <see cref="ClassKind.Requirement"/>, falling back to a category named "Requirement".
        /// </summary>
        /// <param name="rdl">The <see cref="ReferenceDataLibrary"/> whose chain is searched.</param>
        /// <returns>The resolved requirement <see cref="Category"/>, or null when the model has none.</returns>
        private static Category ResolveRequirementCategory(ReferenceDataLibrary rdl)
        {
            var candidates = rdl.QueryCategoriesFromChainOfRdls()
                .Where(x => x.PermissibleClass.Contains(ClassKind.Requirement))
                .ToList();

            foreach (var shortName in VandVRdlManifest.RequirementCategoryShortNames)
            {
                var match = candidates.FirstOrDefault(x => string.Equals(x.ShortName, shortName, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    return match;
                }
            }

            return candidates.FirstOrDefault(x => string.Equals(x.Name, "Requirement", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Asserts whether the current user is permitted to write the V&amp;V reference data into <paramref name="rdl"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="rdl">The target <see cref="ReferenceDataLibrary"/>.</param>
        /// <returns>true when <see cref="ParameterType"/>, <see cref="Category"/> and <see cref="Rule"/> may all be written.</returns>
        public bool CanSeed(ISession session, ReferenceDataLibrary rdl)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (rdl == null)
            {
                throw new ArgumentNullException(nameof(rdl));
            }

            var permissionService = session.PermissionService;

            return permissionService.CanWrite(ClassKind.ParameterType, rdl)
                   && permissionService.CanWrite(ClassKind.Category, rdl)
                   && permissionService.CanWrite(ClassKind.ParameterizedCategoryRule, rdl);
        }

        /// <summary>
        /// Seeds everything in <paramref name="missing"/> into <paramref name="mrdl"/> in a single
        /// <see cref="ThingTransaction"/> and writes it through the <paramref name="session"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/> used to write the transaction.</param>
        /// <param name="mrdl">The target <see cref="ModelReferenceDataLibrary"/>; self-contained and travels with the model.</param>
        /// <param name="missing">The <see cref="VandVRdlCheckResult"/> produced by <see cref="Check"/>.</param>
        /// <param name="stageGates">
        /// The project's stage gates, replacing the manifest's example list when supplied. Stage gates differ per
        /// project, so the set-up flow asks for them rather than silently seeding a hard-coded set.
        /// </param>
        /// <returns>A <see cref="Task"/> that completes when the write has been dispatched.</returns>
        public async Task Seed(ISession session, ModelReferenceDataLibrary mrdl, VandVRdlCheckResult missing, IReadOnlyList<string> stageGates = null)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (mrdl == null)
            {
                throw new ArgumentNullException(nameof(mrdl));
            }

            if (missing == null)
            {
                throw new ArgumentNullException(nameof(missing));
            }

            if (!missing.HasMissingItems)
            {
                return;
            }

            var clone = mrdl.Clone(false);
            var transactionContext = TransactionContextResolver.ResolveContext(mrdl);
            var transaction = new ThingTransaction(transactionContext, clone);

            // Track what this seed creates so same-transaction references (super-categories, rule targets) resolve.
            var createdParameterTypes = new Dictionary<string, ParameterType>();
            var createdCategories = new Dictionary<string, Category>();

            foreach (var definition in missing.MissingParameterTypes)
            {
                var parameterType = this.CreateParameterType(definition, transaction, stageGates);
                clone.ParameterType.Add(parameterType);
                transaction.Create(parameterType);
                createdParameterTypes.Add(definition.ShortName, parameterType);
            }

            foreach (var definition in missing.MissingCategories)
            {
                var category = new Category(Guid.NewGuid(), null, null)
                {
                    ShortName = definition.ShortName,
                    Name = definition.Name
                };

                category.PermissibleClass.AddRange(definition.PermissibleClasses);

                if (definition.SuperCategoryShortName != null)
                {
                    var superCategory = ResolveCategory(definition.SuperCategoryShortName, createdCategories, mrdl);

                    if (superCategory != null)
                    {
                        category.SuperCategory.Add(superCategory);
                    }
                }

                clone.DefinedCategory.Add(category);
                transaction.Create(category);
                createdCategories.Add(definition.ShortName, category);
            }

            foreach (var definition in missing.MissingParameterizedCategoryRules)
            {
                var rule = new ParameterizedCategoryRule(Guid.NewGuid(), null, null)
                {
                    ShortName = definition.ShortName,
                    Name = definition.Name,
                    Category = ResolveCategory(definition.CategoryShortName, createdCategories, mrdl)
                };

                rule.ParameterType.AddRange(
                    definition.ParameterTypeShortNames
                        .Select(shortName => ResolveParameterType(shortName, createdParameterTypes, mrdl))
                        .Where(parameterType => parameterType != null));

                clone.Rule.Add(rule);
                transaction.Create(rule);
            }

            if (missing.RequirementTargetCategory != null)
            {
                foreach (var definition in missing.MissingBinaryRelationshipRules)
                {
                    var rule = new BinaryRelationshipRule(Guid.NewGuid(), null, null)
                    {
                        ShortName = definition.ShortName,
                        Name = definition.Name,
                        ForwardRelationshipName = definition.ForwardRelationshipName,
                        InverseRelationshipName = definition.InverseRelationshipName,
                        RelationshipCategory = ResolveCategory(definition.RelationshipCategoryShortName, createdCategories, mrdl),
                        SourceCategory = ResolveCategory(definition.SourceCategoryShortName, createdCategories, mrdl),
                        TargetCategory = missing.RequirementTargetCategory
                    };

                    clone.Rule.Add(rule);
                    transaction.Create(rule);
                }
            }

            await session.Write(transaction.FinalizeTransaction());
        }

        /// <summary>
        /// Creates the concrete <see cref="ParameterType"/> for a <see cref="VandVParameterTypeDefinition"/>, registering
        /// any contained <see cref="EnumerationValueDefinition"/>s with the <paramref name="transaction"/>.
        /// </summary>
        /// <param name="definition">The <see cref="VandVParameterTypeDefinition"/>.</param>
        /// <param name="transaction">The <see cref="ThingTransaction"/> the value definitions are registered with.</param>
        /// <param name="stageGates">The project's stage gates, used instead of the manifest list for <c>vnv_stage</c>.</param>
        /// <returns>The created <see cref="ParameterType"/>.</returns>
        private ParameterType CreateParameterType(VandVParameterTypeDefinition definition, IThingTransaction transaction, IReadOnlyList<string> stageGates)
        {
            switch (definition.Kind)
            {
                case VandVParameterKind.Text:
                    return new TextParameterType(Guid.NewGuid(), null, null)
                    {
                        ShortName = definition.ShortName,
                        Name = definition.Name,
                        Symbol = definition.ShortName
                    };

                case VandVParameterKind.Date:
                    return new DateParameterType(Guid.NewGuid(), null, null)
                    {
                        ShortName = definition.ShortName,
                        Name = definition.Name,
                        Symbol = definition.ShortName
                    };

                case VandVParameterKind.Boolean:
                    return new BooleanParameterType(Guid.NewGuid(), null, null)
                    {
                        ShortName = definition.ShortName,
                        Name = definition.Name,
                        Symbol = definition.ShortName
                    };

                case VandVParameterKind.Enumeration:
                    var enumerationParameterType = new EnumerationParameterType(Guid.NewGuid(), null, null)
                    {
                        ShortName = definition.ShortName,
                        Name = definition.Name,
                        Symbol = definition.ShortName,
                        AllowMultiSelect = false
                    };

                    var values = definition.ShortName == "vnv_stage" && stageGates != null && stageGates.Any()
                        ? stageGates
                        : definition.EnumerationValues;

                    foreach (var value in values)
                    {
                        var valueDefinition = new EnumerationValueDefinition(Guid.NewGuid(), null, null)
                        {
                            Name = value,
                            ShortName = VandVRdlManifest.ToShortName(value)
                        };

                        enumerationParameterType.ValueDefinition.Add(valueDefinition);
                        transaction.Create(valueDefinition);
                    }

                    return enumerationParameterType;

                default:
                    throw new ArgumentOutOfRangeException(nameof(definition), definition.Kind, "Unsupported V&V parameter kind");
            }
        }

        /// <summary>
        /// Resolves a <see cref="Category"/> by short-name, preferring one created in the current seed and otherwise
        /// searching the whole RDL chain.
        /// </summary>
        /// <param name="shortName">The category short-name.</param>
        /// <param name="created">The categories created so far in this seed.</param>
        /// <param name="rdl">The target <see cref="ReferenceDataLibrary"/> whose chain is searched.</param>
        /// <returns>The resolved <see cref="Category"/>, or null when it cannot be found.</returns>
        private static Category ResolveCategory(string shortName, IReadOnlyDictionary<string, Category> created, ReferenceDataLibrary rdl)
        {
            return created.TryGetValue(shortName, out var category)
                ? category
                : rdl.QueryCategoriesFromChainOfRdls().FirstOrDefault(x => x.ShortName == shortName);
        }

        /// <summary>
        /// Resolves a <see cref="ParameterType"/> by short-name, preferring one created in the current seed and otherwise
        /// searching the whole RDL chain.
        /// </summary>
        /// <param name="shortName">The parameter type short-name.</param>
        /// <param name="created">The parameter types created so far in this seed.</param>
        /// <param name="rdl">The target <see cref="ReferenceDataLibrary"/> whose chain is searched.</param>
        /// <returns>The resolved <see cref="ParameterType"/>, or null when it cannot be found.</returns>
        private static ParameterType ResolveParameterType(string shortName, IReadOnlyDictionary<string, ParameterType> created, ReferenceDataLibrary rdl)
        {
            return created.TryGetValue(shortName, out var parameterType)
                ? parameterType
                : rdl.QueryParameterTypesFromChainOfRdls().FirstOrDefault(x => x.ShortName == shortName);
        }
    }
}
