// -------------------------------------------------------------------------------------------------
// <copyright file="RowDoubleClickEventArgsConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System.Collections.Generic;
    using DevExpress.Mvvm.UI;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Converter for double click events in datagrid cells.
    /// </summary>
    public class RowDoubleClickEventArgsConverter : MarkupExtensionBase, IEventArgsConverter
    {
        /// <summary>
        /// Converts the supplied arguments to an array of parameters describing the cell.
        /// </summary>
        /// <param name="obj">The event object.</param>
        /// <param name="e">The event arguments.</param>
        /// <returns>An array of cell info objects.</returns>
        public object Convert(object obj, object e)
        {
            if (e is RowDoubleClickEventArgs args)
            {
                var view = (DataViewBase)obj;
                return new List<object> { view.DataControl.GetRow(args.HitInfo.RowHandle), args.HitInfo.Column?.FieldName};
            }

            return null;
        }
    }
}
