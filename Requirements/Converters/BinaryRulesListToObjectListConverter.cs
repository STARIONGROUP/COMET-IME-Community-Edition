// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryRulesListToObjectListConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Converters
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;

    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of <see cref="BinaryRelationshipRule"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class BinaryRulesListToObjectListConverter : GenericListToObjectListConverter<BinaryRelationshipRule>
    {
    }
}