// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VCardTypeConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Converters
{
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using ReactiveUI;

    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of <see cref="VcardTelephoneNumberKind"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class VCardTypeConverter : GenericListToObjectListConverter<VcardTelephoneNumberKind>
    {
    }
}
