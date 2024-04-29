// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IncludeOptionListToObjectListConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// <summary>
//   Defines the IncludeOptionListToObjectListConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using CDP4Common.EngineeringModelData;
    using ReactiveUI;

    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of included <see cref="Option"/>s to an <see cref="object"/> and back.
    /// </summary>
    public class IncludeOptionListToObjectListConverter : GenericListToObjectListConverter<Option>
    {
    }
}
