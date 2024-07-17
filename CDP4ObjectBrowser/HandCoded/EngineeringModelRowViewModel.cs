// -------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="EngineeringModelRowViewModel"/>
    /// </summary>
    public partial class EngineeringModelRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.EngineeringModelSetup.Name;
            this.ShortName = this.Thing.EngineeringModelSetup.ShortName;
        }
    }
}