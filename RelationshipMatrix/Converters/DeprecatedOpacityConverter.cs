// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeprecatedOpacityConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using ViewModels;

    /// <summary>
    /// The converter to retrieve the opacity for the cell to be used for deprecated things.
    /// </summary>
    public class DeprecatedOpacityConverter : BaseMatrixCellViewModelConverter<double>
    {
        /// <summary>
        /// Returns a specific value 
        /// </summary>
        /// <param name="matrixCellViewModel">The <see cref="MatrixCellViewModel"/> that helps to return the right value</param>
        /// <returns>Opacity</returns>
        protected override double GetValue(MatrixCellViewModel matrixCellViewModel)
        {
            return matrixCellViewModel.IsDeprecated ? 0.5D : double.PositiveInfinity;
        }
    }
}
