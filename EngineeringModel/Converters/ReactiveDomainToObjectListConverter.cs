// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveDomainToObjectListConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// <summary>
//   Defines the ReactiveDomainToObjectListConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Converters
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using ReactiveUI;
    using System.Collections.Generic;

    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class ReactiveDomainToObjectListConverter : GenericListToObjectListConverter<DomainOfExpertise>
    {
    }
}