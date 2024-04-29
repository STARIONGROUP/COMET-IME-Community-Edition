// -------------------------------------------------------------------------------------------------
// <copyright file="UnitFactorRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="UnitFactorRowViewModel"/>
    /// </summary>
    public partial class UnitFactorRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Exponent.ToString();
            this.ShortName = string.Empty;
        }
    }
}
