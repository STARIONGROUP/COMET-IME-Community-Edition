// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramRDLHelper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Utilities;

    /// <summary>
    /// Helper class with dealing with RDL specific things in diagramming, such as constraints
    /// </summary>
    public static class DiagramRDLHelper
    {
        /// <summary>
        /// ThingPreference key for constraints
        /// </summary>
        private static readonly string constraintTag = "gcd_constraint";

        /// <summary>
        /// ThingPreference value for optional constraints
        /// </summary>
        private static readonly string constraintOptionalTag = $"{constraintTag}_optional";

        /// <summary>
        /// ThingPreference value for restricted constraints
        /// </summary>
        private static readonly string constraintRestrictedTag = $"{constraintTag}_restricted";

        /// <summary>
        /// ThingPreference value for enforced constraints
        /// </summary>
        private static readonly string constraintEnforcedTag = $"{constraintTag}_enforced";

        /// <summary>
        /// ThingPreference key for implications
        /// </summary>
        private static readonly string implicationTag = "gcd_implication";

        /// <summary>
        /// ThingPreference value for A implies B implication
        /// </summary>
        private static readonly string implicationAImpliesBTag = $"{implicationTag}_AImpliesB";

        /// <summary>
        /// ThingPreference value for A implies not B implication
        /// </summary>
        private static readonly string implicationAImpliesNotBTag = $"{implicationTag}_AImpliesNotB";

        /// <summary>
        /// ThingPreference value for not A implies not B implication
        /// </summary>
        private static readonly string implicationNotAImpliesNotBTag = $"{implicationTag}_NotAImpliesNotB";

        /// <summary>
        /// ThingPreference value for not A implies not B implication
        /// </summary>
        private static readonly string implicationNotAImpliesBTag = $"{implicationTag}_NotAImpliesB";

        /// <summary>
        /// Determines whether a <see cref="BinaryRelationship"/> is a constraint
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/></param>
        /// <returns>True if is a constraint</returns>
        public static bool IsConstraint(this BinaryRelationship relationship)
        {
            var category = GetConstraintCategory(relationship);

            if (category != null)
            {
                var value = category.GetThingPreference(constraintTag);

                if (value == constraintOptionalTag || value == constraintEnforcedTag || value == constraintRestrictedTag)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a <see cref="BinaryRelationship"/> is an implication
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/></param>
        /// <returns>True if is a implication</returns>
        public static bool IsImplication(this BinaryRelationship relationship)
        {
            var category = GetImplicationCategory(relationship);

            if (category != null)
            {
                var value = category.GetThingPreference(implicationTag);

                if (value == implicationAImpliesBTag || value == implicationAImpliesNotBTag || value == implicationNotAImpliesBTag || value == implicationNotAImpliesNotBTag)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Grabs a constraint category if it already exists or creates a new one
        /// </summary>
        /// <param name="iteration">The iteration to check</param>
        /// <param name="kind">The <see cref="ConstraintKind"/> to grab</param>
        /// <param name="category">The returned <see cref="Category"/> with the correct ThingPreference, or new one if created</param>
        /// <param name="rdlClone">The clone of the <see cref="ReferenceDataLibrary"/> that the new category is created in. Null if an existing category is used.</param>
        /// <returns>True if the Category to be used is new.</returns>
        public static bool GetOrAddConstraintCategory(Iteration iteration, ConstraintKind kind, out Category category, out ReferenceDataLibrary rdlClone)
        {
            var rdlChain = ((EngineeringModel)iteration.Container).RequiredRdls.ToList();

            // get a category if it exists already
            category = GetConstraintCategory(rdlChain, kind);

            // if it does not
            if (category == null)
            {
                // choose the top most rdl
                var topRdl = rdlChain.Single(rdl => rdl.RequiredRdl is null);
                rdlClone = topRdl.Clone(false);

                // create new category
                category = CreateByConstraintKind(kind, iteration);

                rdlClone.DefinedCategory.Add(category);

                return true;
            }

            // no need to set rdl clone as no category is being created
            rdlClone = null;

            // return false for not having to create a new category
            return false;
        }

        /// <summary>
        /// Grabs a implication category if it already exists or creates a new one
        /// </summary>
        /// <param name="iteration">The iteration to check</param>
        /// <param name="kind">The <see cref="ImplicationKind"/> to grab</param>
        /// <param name="category">The returned <see cref="Category"/> with the correct ThingPreference, or new one if created</param>
        /// <param name="rdlClone">The clone of the <see cref="ReferenceDataLibrary"/> that the new category is created in. Null if an existing category is used.</param>
        /// <returns>True if the Category to be used is new.</returns>
        public static bool GetOrAddImplicationCategory(Iteration iteration, ImplicationKind kind, out Category category, out ReferenceDataLibrary rdlClone)
        {
            var rdlChain = ((EngineeringModel)iteration.Container).RequiredRdls.ToList();

            // get a category if it exists already
            category = GetImplicationCategory(rdlChain, kind);

            // if it does not
            if (category == null)
            {
                // choose the top most rdl
                var topRdl = rdlChain.Single(rdl => rdl.RequiredRdl is null);
                rdlClone = topRdl.Clone(false);

                // create new category
                category = CreateByImplicationKind(kind, iteration);

                rdlClone.DefinedCategory.Add(category);

                return true;
            }

            // no need to set rdl clone as no category is being created
            rdlClone = null;

            // return false for not having to create a new category
            return false;
        }

        /// <summary>
        /// Creates a new category to be added of the correct constraint kind
        /// </summary>
        /// <param name="kind">The <see cref="ConstraintKind"/> to create</param>
        /// <param name="iteration">The iteration of which the chain of RDLs is observed</param>
        /// <returns></returns>
        private static Category CreateByConstraintKind(ConstraintKind kind, Iteration iteration)
        {
            var category = new Category(Guid.NewGuid(), iteration.Cache, iteration.IDalUri)
            {
                Name = $"{kind} Constraint",
                ShortName = $"constraint_{kind.ToString().ToLower()}",
                PermissibleClass = new List<ClassKind> { ClassKind.BinaryRelationship },
                IsAbstract = false
            };

            category.SetThingPreference(constraintTag, GetTagFromConstraintKind(kind));

            return category;
        }

        /// <summary>
        /// Creates a new category to be added of the correct implication kind
        /// </summary>
        /// <param name="kind">The <see cref="ImplicationKind"/> to create</param>
        /// <param name="iteration">The iteration of which the chain of RDLs is observed</param>
        /// <returns></returns>
        private static Category CreateByImplicationKind(ImplicationKind kind, Iteration iteration)
        {
            var categoryName = Regex.Replace(kind.ToString(), @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");

            var category = new Category(Guid.NewGuid(), iteration.Cache, iteration.IDalUri)
            {
                Name = $"{categoryName} Implication",
                ShortName = $"cimplication_{kind.ToString().ToLower()}",
                PermissibleClass = new List<ClassKind> { ClassKind.BinaryRelationship },
                IsAbstract = false
            };

            category.SetThingPreference(implicationTag, GetTagFromImplicationKind(kind));

            return category;
        }

        /// <summary>
        /// Gets the category of the correct constraint kind if it exists anywhere in the chain of rdls
        /// </summary>
        /// <param name="rdlChain">The chain of RDLs</param>
        /// <param name="kind">The <see cref="ConstraintKind"/> to grab</param>
        /// <returns>The Category if it exists and null if not</returns>
        public static Category GetConstraintCategory(IEnumerable<ReferenceDataLibrary> rdlChain, ConstraintKind kind)
        {
            return rdlChain.SelectMany(r => r.DefinedCategory).FirstOrDefault(c => c.GetThingPreference(constraintTag) == GetTagFromConstraintKind(kind));
        }

        /// <summary>
        /// Gets the category of the correct implication kind if it exists anywhere in the chain of rdls
        /// </summary>
        /// <param name="rdlChain">The chain of RDLs</param>
        /// <param name="kind">The <see cref="ImplicationKind"/> to grab</param>
        /// <returns>The Category if it exists and null if not</returns>
        public static Category GetImplicationCategory(IEnumerable<ReferenceDataLibrary> rdlChain, ImplicationKind kind)
        {
            return rdlChain.SelectMany(r => r.DefinedCategory).FirstOrDefault(c => c.GetThingPreference(implicationTag) == GetTagFromImplicationKind(kind));
        }

        /// <summary>
        /// Gets the ThingPreference value based on the constraint kind
        /// </summary>
        /// <param name="kind">The <see cref="ConstraintKind"/></param>
        /// <returns>The ThingPreference value</returns>
        private static string GetTagFromConstraintKind(ConstraintKind kind)
        {
            return kind switch
            {
                ConstraintKind.Restricted => constraintRestrictedTag,
                ConstraintKind.Optional => constraintOptionalTag,
                ConstraintKind.Enforced => constraintEnforcedTag,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Gets the ThingPreference value based on the Implication kind
        /// </summary>
        /// <param name="kind">The <see cref="ImplicationKind"/></param>
        /// <returns>The ThingPreference value</returns>
        private static string GetTagFromImplicationKind(ImplicationKind kind)
        {
            return kind switch
            {
                ImplicationKind.AImpliesB => implicationAImpliesBTag,
                ImplicationKind.AImpliesNotB => implicationAImpliesNotBTag,
                ImplicationKind.NotAImpliesB => implicationNotAImpliesBTag,
                ImplicationKind.NotAImpliesNotB => implicationNotAImpliesNotBTag,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Gets the constraint category of a <see cref="BinaryRelationship"/>
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/></param>
        /// <returns>The applied <see cref="Category"/></returns>
        private static Category GetConstraintCategory(BinaryRelationship relationship)
        {
            return relationship.Category.FirstOrDefault(c => c.GetThingPreference(constraintTag) != null);
        }

        /// <summary>
        /// Gets the implication category of a <see cref="BinaryRelationship"/>
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/></param>
        /// <returns>The applied <see cref="Category"/></returns>
        private static Category GetImplicationCategory(BinaryRelationship relationship)
        {
            return relationship.Category.FirstOrDefault(c => c.GetThingPreference(implicationTag) != null);
        }

        /// <summary>
        /// Gets the <see cref="ConstraintKind"/> of a <see cref="BinaryRelationship"/>
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/></param>
        /// <returns>The <see cref="ConstraintKind"/> of the Binary Relationship</returns>
        public static ConstraintKind GetConstraintKind(BinaryRelationship relationship)
        {
            var category = relationship.Category.FirstOrDefault(c => c.GetThingPreference(constraintTag) != null);

            if (category != null)
            {
                var value = category.GetThingPreference(constraintTag);

                if (value == constraintOptionalTag)
                {
                    return ConstraintKind.Optional;
                }

                if (value == constraintEnforcedTag)
                {
                    return ConstraintKind.Enforced;
                }

                if (value == constraintRestrictedTag)
                {
                    return ConstraintKind.Restricted;
                }
            }

            throw new ArgumentException("Supplied BinaryRelationship does not have a category applied that would indicate that it is a constraint.", nameof(relationship));
        }

        /// <summary>
        /// Gets the <see cref="ImplicationKind"/> of a <see cref="BinaryRelationship"/>
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/></param>
        /// <returns>The <see cref="ImplicationKind"/> of the Binary Relationship</returns>
        public static ImplicationKind GetImplicationKind(BinaryRelationship relationship)
        {
            var category = relationship.Category.FirstOrDefault(c => c.GetThingPreference(implicationTag) != null);

            if (category != null)
            {
                var value = category.GetThingPreference(implicationTag);

                if (value == implicationAImpliesBTag)
                {
                    return ImplicationKind.AImpliesB;
                }

                if (value == implicationAImpliesNotBTag)
                {
                    return ImplicationKind.AImpliesNotB;
                }

                if (value == implicationNotAImpliesBTag)
                {
                    return ImplicationKind.NotAImpliesB;
                }

                if (value == implicationNotAImpliesNotBTag)
                {
                    return ImplicationKind.NotAImpliesNotB;
                }
            }

            throw new ArgumentException("Supplied BinaryRelationship does not have a category applied that would indicate that it is a implication.", nameof(relationship));
        }
    }
}
