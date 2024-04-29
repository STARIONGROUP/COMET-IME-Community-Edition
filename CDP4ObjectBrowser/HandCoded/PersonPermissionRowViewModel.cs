﻿// -------------------------------------------------------------------------------------------------
// <copyright file="PersonPermissionRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="PersonPermissionRowViewModel"/>
    /// </summary>
    public partial class PersonPermissionRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.AccessRight.ToString();
            this.ShortName = this.Thing.AccessRight.ToString();
        }
    }
}
