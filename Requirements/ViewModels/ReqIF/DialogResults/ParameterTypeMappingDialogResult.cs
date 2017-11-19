// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeMappingDialogResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using CDP4Composition.Navigation;
    using ReqIFSharp;

    /// <summary>
    /// The dialog result class for the parameter type mapping
    /// </summary>
    public class ParameterTypeMappingDialogResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeMappingDialogResult"/> class
        /// </summary>
        /// <param name="map">The result of the parameter type mapping</param>
        /// <param name="res">A value indicating whether the task in the </param>
        public ParameterTypeMappingDialogResult(IDictionary<DatatypeDefinition, DatatypeDefinitionMap> map, bool? res)
            : base(res)
        {
            this.Map = map;
        }

        /// <summary>
        /// Gets the result of the <see cref="DatatypeDefinition"/> mapping 
        /// </summary>
        public IDictionary<DatatypeDefinition, DatatypeDefinitionMap> Map { get; private set; }
    }
}