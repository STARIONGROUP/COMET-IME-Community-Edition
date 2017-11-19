﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveScaleToObjectListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// <summary>
//   Defines the ReactiveScaleToObjectListConverte type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Converters
{
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;
    using System.Collections.Generic;

    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of <see cref="Category"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class ReactiveScaleToObjectListConverter : GenericListToObjectListConverter<MeasurementScale>
    {
    }
}