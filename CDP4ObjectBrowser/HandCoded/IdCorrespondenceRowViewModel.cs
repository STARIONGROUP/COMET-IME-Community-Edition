// -------------------------------------------------------------------------------------------------
// <copyright file="IdCorrespondenceRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="IdCorrespondenceRowViewModel"/>
    /// </summary>
    public partial class IdCorrespondenceRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.ExternalId;
            this.ShortName = this.Thing.ExternalId;
        }
    }
}
