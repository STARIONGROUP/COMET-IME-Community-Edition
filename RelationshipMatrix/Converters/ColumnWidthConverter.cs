// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnWidthConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using DevExpress.Xpf.Grid;
    using ViewModels;

    /// <summary>
    /// The converter to convert a column field name to a column width
    /// </summary>
    public class ColumnWidthConverter : IValueConverter
    {
        private const int DEFAULT_WIDTH = 30;
        private const int MAIN_NAME_COL_WIDTH = 100;

        /// <summary>
        /// The conversion method returns the width for the current column
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
            var fieldname = value?.ToString();
            if (string.IsNullOrWhiteSpace(fieldname))
            {
                return DEFAULT_WIDTH;
            }

            return fieldname == MatrixViewModel.ROW_NAME_COLUMN ? MAIN_NAME_COL_WIDTH : DEFAULT_WIDTH;
        }

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
