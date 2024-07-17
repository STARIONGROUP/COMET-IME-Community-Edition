// -------------------------------------------------------------------------------------------------
// <copyright file="HyperlinkRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="HyperLinkRowViewModel"/>
    /// </summary>
    public partial class HyperLinkRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Content;
            this.ShortName = this.Thing.Content;
        }
    }
}
