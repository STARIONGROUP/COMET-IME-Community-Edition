﻿// -------------------------------------------------------------------------------------------------
// <copyright file="PublicationRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="PublicationRowViewModel"/>
    /// </summary>
    public partial class PublicationRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.CreatedOn.ToLongDateString();
            this.ShortName = this.Thing.ModifiedOn.ToLongDateString();
        }
    }
}
