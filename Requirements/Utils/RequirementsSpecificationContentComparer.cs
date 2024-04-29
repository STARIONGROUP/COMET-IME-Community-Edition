// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationContentComparer.cs" company="Starion Group S.A.">
//   Copyright (c) 2016 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    
    /// <summary>
    /// The purpose of the <see cref="RequirementsSpecificationContentComparer"/> is to sort the <see cref="Requirement"/>, the <see cref="RequirementsGroup"/> 
    /// and <see cref="RequirementsSpecification"/> that are contained by a <see cref="RequirementsSpecification"/> 
    /// </summary>
    public class RequirementsSpecificationContentComparer : IComparer<Thing>
    {
        /// <summary>
        /// The Permissible Kind of child <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        private static readonly List<Type> PermissibleTypes = new List<Type>
                                                                     {
                                                                         typeof(RequirementsSpecification),
                                                                         typeof(RequirementsGroup),
                                                                         typeof(Requirement)
                                                                     };
                
        /// <summary>
        /// Compares two <see cref="IRowViewModelBase{Thing}"/>
        /// </summary>
        /// <param name="x">The first <see cref="IRowViewModelBase{Thing}"/> to compare</param>
        /// <param name="y">The second <see cref="IRowViewModelBase{Thing}"/> to compare</param>
        /// <returns>
        /// Less than zero : x is "lower" than y 
        /// Zero: x "equals" y. 
        /// Greater than zero: x is "greater" than y.
        /// </returns>
        public int Compare(Thing x, Thing y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }

            var xType = x.GetType();
            var yType = y.GetType();

            if (!PermissibleTypes.Any(type => type.IsAssignableFrom(xType)))
            {
                throw new ArgumentException(string.Format("argument x is of type {0} which is not supported", xType.Name));
            }

            if (!PermissibleTypes.Any(type => type.IsAssignableFrom(yType)))
            {
                throw new ArgumentException(string.Format("argument y is of type {0} which is not supported", yType.Name));
            }

            var breadCrumbX = this.ComputeBreadCrumb(x);
            var breadCrumbY = this.ComputeBreadCrumb(y);

            return System.String.Compare(breadCrumbX, breadCrumbY, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Computes the breadcrumb of the provided <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">
        /// An instance of <see cref="Thing"/> of which the breadcrumb is to be computed
        /// </param>
        /// <returns>
        /// a string representing the breadcrumb
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when thing is not an instance of <see cref="RequirementsSpecification"/>, <see cref="RequirementsGroup"/> or <see cref="Requirement"/>
        /// </exception>
        private string ComputeBreadCrumb(Thing thing)
        {
            var requirementsSpecification = thing as RequirementsSpecification;
            if (requirementsSpecification != null)
            {
                return requirementsSpecification.BreadCrumb();
            }

            var requirementsGroup = thing as RequirementsGroup;
            if (requirementsGroup != null)
            {
                return requirementsGroup.BreadCrumb();
            }

            var requirement = thing as Requirement;
            if (requirement != null)
            {
                return requirement.BreadCrumb();
            }

            throw new InvalidOperationException(string.Format(" The Breadcrumb of a {0} cannot be computed", thing.GetType()));
        }
    }
}
