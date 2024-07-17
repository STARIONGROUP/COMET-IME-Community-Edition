// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseMatrixCellViewModelConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using DevExpress.Xpf.Grid;
    using ViewModels;

    /// <summary>
    /// The converter implements the default way to retrieve specific data for a cell to be used.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to be returned by the GetValue method.
    /// In other words, the type of the property it's bound to.
    /// </typeparam>
    public abstract class BaseMatrixCellViewModelConverter<T> : IValueConverter
    {
        /// <summary>
        /// The conversion method returns the color to use for cells having deprecated parents.
        /// </summary>
        /// <param name="value">
        /// The incoming value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// The <see cref="object"/> containing the same objects as the input collection.
        /// </returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var gridData = value as EditGridCellData;

            if (!(gridData?.RowData.Row is IDictionary<string, MatrixCellViewModel> cellData))
            {
                return default(T);
            }

            var matrixCellViewModel = cellData[gridData.Column.FieldName];

            return this.GetValue(matrixCellViewModel);
        }

        /// <summary>
        /// Returns a specific value 
        /// </summary>
        /// <typeparam name="T">The type of the value to be returned</typeparam>
        /// <param name="matrixCellViewModel">The <see cref="MatrixCellViewModel"/> that helps to return the right value</param>
        /// <returns>Value of type T</returns>
        protected abstract T GetValue(MatrixCellViewModel matrixCellViewModel);

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">
        /// The incoming collection.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// The result 
        /// </returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
