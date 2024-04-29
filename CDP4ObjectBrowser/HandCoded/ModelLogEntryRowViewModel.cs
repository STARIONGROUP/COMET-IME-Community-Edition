// -------------------------------------------------------------------------------------------------
// <copyright file="ModelLogEntryRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ModelLogEntryRowViewModel"/>
    /// </summary>
    public partial class ModelLogEntryRowViewModel
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
