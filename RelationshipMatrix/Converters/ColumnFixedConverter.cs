// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnFixedConverter.cs" company="RHEA System S.A.">
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
    /// The converter to convert a column field name to <see cref="FixedStyle"/>
    /// </summary>
    public class ColumnFixedConverter : IValueConverter
    {
        /// <summary>
        /// The conversion method returns the <see cref="FixedStyle"/> for the current column
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
            if (value == null)
            {
                return FixedStyle.None;
            }

            var columnFieldName = value.ToString();
            return columnFieldName == MatrixViewModel.ROW_NAME_COLUMN ? FixedStyle.Left : FixedStyle.None;
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
