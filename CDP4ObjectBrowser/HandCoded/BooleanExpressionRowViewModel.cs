﻿// -------------------------------------------------------------------------------------------------
// <copyright file="BooleanExpressionRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="BooleanExpressionRowViewModel{T}"/>
    /// </summary>
    public partial class BooleanExpressionRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.ClassKind.ToString();
            this.ShortName = string.Empty;
        }
    }
}
