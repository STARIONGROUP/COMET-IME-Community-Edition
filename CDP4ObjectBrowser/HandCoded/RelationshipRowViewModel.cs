// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="RelationshipRowViewModel{T}"/>
    /// </summary>
    /// <typeparam name="T">Relationship</typeparam>
    public partial class RelationshipRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.ClassKind.ToString();
            this.ShortName = string.Empty;
        }
    }
}
