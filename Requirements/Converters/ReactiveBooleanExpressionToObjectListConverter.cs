﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveBooleanExpressionToObjectListConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// <summary>
//   Defines the ReactiveBooleanExpressionToObjectListConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Converters
{
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Converters;


    /// <summary>
    /// Converts the <see cref="ReactiveList{T}"/> of <see cref="BooleanExpression"/> to <see cref="List{T}"/> of <see cref="object"/> and back.
    /// </summary>
    public class ReactiveBooleanExpressionToObjectListConverter : GenericListToObjectListConverter<BooleanExpression>
    {
    }
}