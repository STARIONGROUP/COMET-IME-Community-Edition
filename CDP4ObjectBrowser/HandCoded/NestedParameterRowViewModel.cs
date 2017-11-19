// -------------------------------------------------------------------------------------------------
// <copyright file="NestedParameterRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="NestedParameterRowViewModel"/>
    /// </summary>
    public partial class NestedParameterRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.ActualValue;
            this.ShortName = this.Thing.Formula;
        }
    }
}
