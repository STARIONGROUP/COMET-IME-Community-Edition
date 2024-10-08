﻿// -------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="IterationSetupRowViewModel"/>
    /// </summary>
    public partial class IterationSetupRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Description;
            this.ShortName = this.Thing.CreatedOn.ToLongDateString();
        }
    }
}
