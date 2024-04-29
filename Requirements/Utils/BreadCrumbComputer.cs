// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BreadCrumbComputer.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Utils
{
    using CDP4Common;
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Exceptions;

    public static class BreadCrumbComputer
    {
        /// <summary>
        /// Computes the bread crumb of a <see cref="RequirementsContainer"/>
        /// </summary>
        /// <param name="requirementsContainer">
        /// The <see cref="RequirementsContainer"/> of which the bread crumb is to be computed
        /// </param>
        /// <returns>
        /// RequirementsSpecification: S:ShortName
        /// RequirementsGroup: RG:ShortName
        /// </returns>
        public static string BreadCrumb(this RequirementsContainer requirementsContainer)
        {
            var container = requirementsContainer.Container as RequirementsContainer;

            if (container != null)
            {
                return string.Format("{0}.{1}", container.BreadCrumb(), requirementsContainer.BreadCrumbPart());
            }

            return BreadCrumbPart(requirementsContainer);
        }

        /// <summary>
        /// Computes the bread crumb of a <see cref="Requirement"/>
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement"/> of which the bread crumb is to be computed
        /// </param>
        /// <returns>
        /// Requirement: R:ShortName
        /// </returns>
        public static string BreadCrumb(this Requirement requirement)
        {
            if (requirement.Container == null)
            {
                throw new ContainmentException("The BreadCrumb can only be computed when the container property is not null");
            }

            if (requirement.Group == null)
            {
                var requirementsSpecification = (RequirementsSpecification)requirement.Container;

                return string.Format("{0}.{1}", requirementsSpecification.BreadCrumbPart(), requirement.BreadCrumbPart());
            }

            var groupBreadCrumb = requirement.Group.BreadCrumb();
            return string.Format("{0}.{1}", groupBreadCrumb, requirement.BreadCrumbPart());
        }

        /// <summary>
        /// Computes the Bread Crumb Part of a <see cref="RequirementsContainer"/>
        /// </summary>
        /// <param name="requirementsContainer">
        /// The <see cref="RequirementsContainer"/> of which the part is computed
        /// </param>
        /// <returns>
        /// RequirementsSpecification: S:ShortName
        /// RequirementsGroup: RG:ShortName
        /// </returns>
        public static string BreadCrumbPart(this RequirementsContainer requirementsContainer)
        {
            var requirementsSpecification = requirementsContainer as RequirementsSpecification;
            if (requirementsSpecification != null)
            {
                return string.Format("S:{0}", requirementsSpecification.ShortName);
            }

            var requirementsGroup = requirementsContainer as RequirementsGroup;
            if (requirementsGroup != null)
            {
                return string.Format("RG:{0}", requirementsGroup.ShortName);
            }

            throw new InvalidOperationException(string.Format("{0} is not supported by the BreadCrumbComputer", requirementsContainer.GetType()));
        }

        /// <summary>
        /// Computes the Bread Crumb Part of a <see cref="Requirement"/>
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement"/> of which the part is computed
        /// </param>
        /// <returns>
        /// Requirement: R:ShortName
        /// </returns>
        public static string BreadCrumbPart(this Requirement requirement)
        {
            return string.Format("R:{0}", requirement.ShortName);
        }
    }
}
