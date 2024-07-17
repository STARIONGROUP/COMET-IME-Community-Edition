﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOrOverrideBaseRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ParameterOrOverrideBaseRowViewModel{T}"/>
    /// </summary>
    /// <typeparam name="T">ParameterOrOverrideBase</typeparam>
    public partial class ParameterOrOverrideBaseRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.ParameterType.Name;
            this.ShortName = this.Thing.ParameterType.ShortName;
        }
    }
}
