// -------------------------------------------------------------------------------------------------
// <copyright file="ModellingAnnotationItemRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    /// <summary>
    /// Partial class representing <see cref="ModellingAnnotationItemRowViewModel{T}"/>
    /// </summary>
    /// <typeparam name="T">ModellingAnnotationItem</typeparam>
    public partial class ModellingAnnotationItemRowViewModel<T>
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = this.Thing.Title;
            this.ShortName = this.Thing.LanguageCode;
        }
    }
}
