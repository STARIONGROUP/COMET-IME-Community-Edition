﻿// -------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScaleRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="MeasurementScaleRowViewModel{T}"/>
    /// </summary>
    /// <typeparam name="T">MeasurementScale</typeparam>
    public partial class MeasurementScaleRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
        }
    }
}
