// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipGroupMappingDialogResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using ReqIFSharp;

    public class RelationshipGroupMappingDialogResult : MappingDialogNavigationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGroupMappingDialogResult"/> class
        /// </summary>
        /// <param name="map">The result of the relationship mapping</param>
        /// <param name="goNext">A value indicating whether the mapping shall proceed</param>
        /// <param name="res">The dialog result</param>
        public RelationshipGroupMappingDialogResult(IReadOnlyDictionary<RelationGroupType, RelationGroupTypeMap> map, bool? goNext, bool? res)
            : base(goNext, res)
        {
            this.Map = map;
        }

        /// <summary>
        /// Gets the result of the mapping 
        /// </summary>
        public IReadOnlyDictionary<RelationGroupType, RelationGroupTypeMap> Map { get; private set; }
    }
}