// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceDataLibraryRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ParameterBaseRowViewModel{T}"/>
    /// </summary>
    /// <typeparam name="T">ParameterBase</typeparam>
    public partial class ParameterBaseRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.ParameterType.Name;
            this.ShortName = this.Thing.ParameterType.ShortName;
        }
    }
}
