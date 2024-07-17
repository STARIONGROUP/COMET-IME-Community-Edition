// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ParticipantRowViewModel"/>
    /// </summary>
    public partial class ParticipantRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Person.Name;
            this.ShortName = this.Thing.Person.ShortName;
        }
    }
}
