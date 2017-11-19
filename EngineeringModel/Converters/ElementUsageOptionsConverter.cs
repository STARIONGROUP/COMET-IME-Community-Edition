// -------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageOptionsConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Converters
{
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Converters;

    /// <summary>
    /// Converts lists of selected <see cref="Option"/>s in an <see cref="ElementUsage"/> view model.
    /// </summary>
    public class ElementUsageOptionsConverter : GenericListToObjectListConverter<Option>
    {
    }
}
