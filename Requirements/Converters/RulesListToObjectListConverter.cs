// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RulesListToObjectListConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Converters
{
    using System.Collections.Generic;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using ReactiveUI;
    using ViewModels;

    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of <see cref="ParameterizedCategoryRule"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class RulesListToObjectListConverter : GenericListToObjectListConverter<ParameterizedCategoryRule>
    {
    }
}