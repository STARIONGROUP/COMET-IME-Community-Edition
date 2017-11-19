// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeComponentRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ParameterTypeComponentRowViewModel"/>
    /// </summary>
    public partial class ParameterTypeComponentRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.ShortName;
            this.ShortName = this.Thing.ShortName;
        }
    }
}
