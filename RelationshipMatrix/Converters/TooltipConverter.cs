// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TooltipConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using DevExpress.Xpf.Grid;
    using ViewModels;

    /// <summary>
    /// The converter to retrieve the tooltip to display for a row based on the <see cref="EditGridCellData"/>
    /// </summary>
    public class TooltipConverter : BaseMatrixCellViewModelConverter<string>
    {
        /// <summary>
        /// Returns a specific value 
        /// </summary>
        /// <param name="matrixCellViewModel">The <see cref="MatrixCellViewModel"/> that helps to return the right value</param>
        /// <returns></returns>
        protected override string GetValue(MatrixCellViewModel matrixCellViewModel)
        {
            return matrixCellViewModel?.Tooltip ?? "-";
        }
    }
}
