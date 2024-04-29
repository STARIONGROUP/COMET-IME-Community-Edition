// -------------------------------------------------------------------------------------------------
// <copyright file="MappingToReferenceScaleRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="MappingToReferenceScaleRowViewModel"/>
    /// </summary>
    public partial class MappingToReferenceScaleRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.DependentScaleValue.ToString();
            this.ShortName = this.Thing.ReferenceScaleValue.ToString();
        }
    }
}
