// -------------------------------------------------------------------------------------------------
// <copyright file="TelephoneNumberRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="TelephoneNumberRowViewModel"/>
    /// </summary>
    public partial class TelephoneNumberRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Value;
            this.ShortName = this.Thing.Value;
        }
    }
}
