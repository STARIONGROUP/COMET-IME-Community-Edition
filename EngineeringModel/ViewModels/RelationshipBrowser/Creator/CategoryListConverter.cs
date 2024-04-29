﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryListConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;

    /// <summary>
    /// The converter to convert a <see cref="List{Category}"/> to a <see cref="List{Object}"/>
    /// </summary>
    public class CategoryListConverter : GenericObjectListConverter<Category>
    {
    }
}