// -------------------------------------------------------------------------------------------------
// <copyright file="FileStoreRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="FileStoreRowViewModel{T}"/>
    /// </summary>
    /// /// <typeparam name="T">FileStore</typeparam>
    public partial class FileStoreRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.Name;
        }
    }
}
