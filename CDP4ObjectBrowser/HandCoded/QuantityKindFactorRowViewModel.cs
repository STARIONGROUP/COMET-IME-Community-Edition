﻿// -------------------------------------------------------------------------------------------------
// <copyright file="QuantityKindFactorRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="QuantityKindFactorRowViewModel"/>
    /// </summary>
    public partial class QuantityKindFactorRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.QuantityKind.Name;
            this.ShortName = this.Thing.QuantityKind.ShortName;
        }
    }
}
