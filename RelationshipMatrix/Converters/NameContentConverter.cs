// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameContentConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using CDP4Common.CommonData;
    using DevExpress.Xpf.Grid;
    using ViewModels;

    /// <summary>
    /// The converter to retrieve the name to display for a row based on the <see cref="EditGridCellData"/>
    /// </summary>
    public class NameContentConverter : IMultiValueConverter
    {
        /// <summary>
        /// The conversion method returns the object associated to the current fieldname
        /// </summary>
        /// <param name="values">
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
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                return null;
            }

            var gridData = values.FirstOrDefault() as EditGridCellData;

            if (gridData == null)
            {
                return null;
            }

            var row = gridData.RowData?.Row as IDictionary<string, MatrixCellViewModel>;

            if (row == null)
            {
                return null;
            }

            return this.ConvertRowObject(row, gridData.Column.FieldName);
        }

        /// <summary>
        /// Converts the Row object contents into useable name
        /// </summary>
        /// <param name="row">The row data object.</param>
        /// <param name="fieldName">The field name to be used as key in the row object</param>
        /// <returns>The required display name of the row.</returns>
        public object ConvertRowObject(IDictionary<string, MatrixCellViewModel> row, string fieldName)
        {
            var matrixCellViewModel = row[fieldName];

            return matrixCellViewModel?.SourceY is DefinedThing definedThing ? typeof(DefinedThing).GetProperty(matrixCellViewModel.DisplayKind.ToString()).GetValue(matrixCellViewModel.SourceY) : "-";
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">
        /// The incoming collection.
        /// </param>
        /// <param name="targetTypes">
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
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
