// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMappingDialogResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
    using ReqIFSharp;

    public class RelationshipMappingDialogResult : MappingDialogNavigationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMappingDialogResult"/> class
        /// </summary>
        /// <param name="map">The result of the relationship mapping</param>
        /// <param name="goNext">A value indicating whether the mapping shall proceed</param>
        /// <param name="res">The dialog result</param>
        public RelationshipMappingDialogResult(IReadOnlyDictionary<SpecRelationType, SpecRelationTypeMap> map, bool? goNext, bool? res)
            : base(goNext, res)
        {
            this.Map = map;
        }

        /// <summary>
        /// Gets the result of the mapping 
        /// </summary>
        public IReadOnlyDictionary<SpecRelationType, SpecRelationTypeMap> Map { get; private set; }
    }
}