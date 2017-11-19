﻿// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramThingBaseRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="DiagramThingBaseRowViewModel{T}"/>
    /// </summary>
    /// /// <typeparam name="T">DiagrammingThing</typeparam>
    public partial class DiagramThingBaseRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Name;
        }
    }
}
