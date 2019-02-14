// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;

    /// <summary>
    /// The converter to convert a <see cref="List{Category}"/> to a <see cref="List{T}"/>
    /// </summary>
    public class CategoryListConverter : GenericObjectListConverter<Category>
    {
    }
}
