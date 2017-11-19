﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ContractDeviationRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ContractDeviationRowViewModel{T}"/>
    /// </summary>
    /// /// <typeparam name="T">ContractDeviation</typeparam>
    public partial class ContractDeviationRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Title;
            this.ShortName = this.Thing.ShortName;
        }
    }
}
