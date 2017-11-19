// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryViewModelListToObjectListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Converters
{
    using System.Collections.Generic;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Converters;
    using ReactiveUI;
    using ViewModels;


    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of <see cref="CategoryComboBoxItemViewModel"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class CategoryViewModelListToObjectListConverter : GenericListToObjectListConverter<CategoryComboBoxItemViewModel>
    {
    }
}