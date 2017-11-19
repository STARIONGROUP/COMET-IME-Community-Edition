// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainListToObjectListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// <summary>
//   Defines the DomainListToObjectListConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Converters
{
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using ReactiveUI;

    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of <see cref="DomainOfExpertise"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class DomainListToObjectListConverter : GenericListToObjectListConverter<DomainOfExpertise>
    {
    }
}
