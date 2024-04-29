// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueSetBaseRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ParameterValueSetBaseRowViewModel{T}"/>
    /// </summary>
    /// <typeparam name="T">ParameterValueSetBase</typeparam>
    public partial class ParameterValueSetBaseRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = string.Format("{{ {0} }}", string.Join(", ", this.Thing.ActualValue));
            this.ShortName = string.Empty;
        }
    }
}
