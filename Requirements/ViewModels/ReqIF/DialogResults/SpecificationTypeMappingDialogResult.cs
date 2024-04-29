// -------------------------------------------------------------------------------------------------
// <copyright file="SpecificationTypeMappingDialogResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using ReqIFSharp;

    /// <summary>
    /// The <see cref="SpecificationType"/> and <see cref="AttributeDefinition"/> mapping result
    /// </summary>
    public class SpecificationTypeMappingDialogResult : MappingDialogNavigationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecificationTypeMappingDialogResult"/> class
        /// </summary>
        /// <param name="specTypeMap">The result of the <see cref="SpecificationType"/> mapping</param>
        /// <param name="goNext">A value indicating whether the next dialog to open shall be the next mapping step or the previous one</param>
        /// <param name="res">A value indicating whether the task in the </param>
        public SpecificationTypeMappingDialogResult(IDictionary<SpecificationType, SpecTypeMap> specTypeMap, bool? goNext, bool? res)
            : base(goNext, res)
        {
            this.SpecificationTypeMap = specTypeMap;
        }

        /// <summary>
        /// Gets the result of the <see cref="SpecType"/> mapping 
        /// </summary>
        public IDictionary<SpecificationType, SpecTypeMap> SpecificationTypeMap { get; private set; }
    }
}