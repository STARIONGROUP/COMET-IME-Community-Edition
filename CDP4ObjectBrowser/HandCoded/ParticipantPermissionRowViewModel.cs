﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantPermissionRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ParticipantPermissionRowViewModel"/>
    /// </summary>
    public partial class ParticipantPermissionRowViewModel
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
