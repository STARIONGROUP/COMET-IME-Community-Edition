﻿// -------------------------------------------------------------------------------------------------
// <copyright file="SiteLogEntryRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="SiteLogEntryRowViewModel"/>
    /// </summary>
    public partial class SiteLogEntryRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Content;
            this.ShortName = this.Thing.Level.ToString();
        }
    }
}
