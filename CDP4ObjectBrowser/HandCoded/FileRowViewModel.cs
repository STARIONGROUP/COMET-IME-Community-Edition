// -------------------------------------------------------------------------------------------------
// <copyright file="FileRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;

    /// <summary>
    /// Partial class representing <see cref="FileRowViewModel"/>
    /// </summary>
    public partial class FileRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.FileRevision.First().Name;
            this.ShortName = string.Empty;
        }
    }
}
