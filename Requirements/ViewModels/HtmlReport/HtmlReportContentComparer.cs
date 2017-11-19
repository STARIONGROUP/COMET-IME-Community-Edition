// ------------------------------------------------------------------------------------------------
// <copyright file="HtmlReportContentComparer.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.HtmlReport
{
    using System.Collections.Generic;
    using CDP4Requirements.Utils;
    using CDP4Requirements.ViewModels.RequirementsSpecificationEditor;

    public class HtmlReportContentComparer
    {
        /// <summary>
        /// The <see cref="IComparer{T}"/> used to compare rows of the <see cref="RequirementsSpecificationEditor"/>
        /// </summary>
        private static readonly BreadCrumbComparer BreadCrumbComparer = new BreadCrumbComparer();
    }
}
