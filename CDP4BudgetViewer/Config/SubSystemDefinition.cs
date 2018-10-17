// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubSystemDefinition.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Defines the sub-system in the budget calculation
    /// </summary>
    public class SubSystemDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubSystemDefinition"/> class
        /// </summary>
        /// <param name="categories">The <see cref="Category"/> associated to the sub-system</param>
        public SubSystemDefinition(IEnumerable<Category> categories, IEnumerable<Category> elementCategories)
        {
            this.Categories = categories?.ToList();
            if (this.Categories == null)
            {
                throw new ArgumentNullException(nameof(categories));
            }

            this.ElementCategories = elementCategories?.ToList();
            if (this.ElementCategories == null || !this.ElementCategories.Any())
            {
                throw new InvalidOperationException("Categories for elements cannot be null or empty");
            }
        }

        /// <summary>
        /// Gets the categories associated to a sub-system
        /// </summary>
        /// <remarks>
        /// may be empty for element not in sub-systems. A definition of a sub-system with no categories means that the physical element are directly under the system
        /// </remarks>
        public IReadOnlyList<Category> Categories { get; }

        /// <summary>
        /// Gets the <see cref="Category"/> of the element part of this sub-system
        /// </summary>
        public IReadOnlyList<Category> ElementCategories { get; set; }

        /// <summary>
        /// Asserts whether the <paramref name="elementBase"/> is part of this <see cref="SubSystemDefinition"/>
        /// </summary>
        /// <param name="elementBase">The <see cref="ElementBase"/></param>
        /// <returns>True if that is the case</returns>
        public bool IsThisSubSystem(ElementBase elementBase)
        {
            return !this.Categories.Except(elementBase.Category).Any();
        }

        /// <summary>
        /// Asserts whether the <paramref name="elementBase"/> is part of this <see cref="SubSystemDefinition"/>
        /// </summary>
        /// <param name="elementBase">The <see cref="ElementBase"/></param>
        /// <returns>True if that is the case</returns>
        public bool IsThisSubSystemEquipment(ElementBase elementBase)
        {
            return !this.ElementCategories.Except(elementBase.Category).Any();
        }

        /// <summary>
        /// Gets the hash-code
        /// </summary>
        /// <returns>The hash-code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine
            {
                var hash = 17;
                foreach (var category in this.Categories)
                {
                    hash = hash * 23 + category.GetHashCode();
                }
                
                return hash;
            }
        }

        /// <summary>
        /// The equality comparer
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns>True if the <see cref="SubSystemDefinition"/> are equal</returns>
        public override bool Equals(object obj)
        {
            var def = obj as SubSystemDefinition;
            if (def == null)
            {
                return false;
            }

            return !this.Categories.Except(def.Categories).Any() && this.Categories.Count == def.Categories.Count;
        }
    }
}
