// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementTypeMappingDialogResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using ReqIFSharp;

    /// <summary>
    /// The <see cref="SpecType"/> and <see cref="AttributeDefinition"/> mapping result
    /// </summary>
    public class RequirementTypeMappingDialogResult : MappingDialogNavigationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementTypeMappingDialogResult"/> class
        /// </summary>
        /// <param name="specTypeMap">The result of the <see cref="SpecType"/> mapping</param>
        /// <param name="goNext">A value indicating whether the next dialog to open shall be the next mapping step or the previous one</param>
        /// <param name="res">A value indicating whether the task in the </param>
        public RequirementTypeMappingDialogResult(IDictionary<SpecObjectType, SpecObjectTypeMap> specTypeMap, bool? goNext, bool? res)
            : base(goNext, res)
        {
            this.ReqCategoryMap = specTypeMap;
        }

        /// <summary>
        /// Gets the result of the <see cref="SpecType"/> mapping 
        /// </summary>
        public IDictionary<SpecObjectType, SpecObjectTypeMap> ReqCategoryMap { get; private set; }
    }
}