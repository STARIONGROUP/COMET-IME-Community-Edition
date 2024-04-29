// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeprecatedBackgroundConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using ViewModels;

    /// <summary>
    /// The converter to retrieve the coloring for cell background to be used for deprecated things.
    /// </summary>
    public class DeprecatedBackgroundConverter : BaseMatrixCellViewModelConverter<string>
    {
        /// <summary>
        /// Returns a specific value 
        /// </summary>
        /// <param name="matrixCellViewModel">The <see cref="MatrixCellViewModel"/> that helps to return the right value</param>
        /// <returns>Background color by name</returns>
        protected override string GetValue(MatrixCellViewModel matrixCellViewModel)
        {
            return matrixCellViewModel.IsDeprecated ? "LightGray" : "Transparent";
        }
    }
}
