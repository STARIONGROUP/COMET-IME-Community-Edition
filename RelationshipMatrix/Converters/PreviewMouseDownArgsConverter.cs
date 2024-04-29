// -------------------------------------------------------------------------------------------------
// <copyright file="PreviewMouseDownArgsConverter.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System.Windows;
    using System.Windows.Input;
    using CDP4RelationshipMatrix.DataTypes;
    using DevExpress.Mvvm.UI;
    using DevExpress.Xpf.Grid;
    
    /// <summary>
    /// Converter for double click events in datagrid cells.
    /// </summary>
    public class PreviewMouseDownArgsConverter : MarkupExtensionBase, IEventArgsConverter
    {
        /// <summary>
        /// Converts the supplied arguments to an array of parameters describing the column.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments.</param>
        /// <returns>An array of cell info objects.</returns>
        public object Convert(object sender, object e)
        {
            if (e is MouseButtonEventArgs args)
            {
                if (args.ChangedButton != MouseButton.Left)
                {
                    return null;
                }

                var tableView = (TableView)sender;
                var tableViewHitInfo = tableView.CalcHitInfo((DependencyObject)args.OriginalSource);

                int? row = null;
                if (tableViewHitInfo.RowHandle != int.MinValue)
                {
                    row = tableViewHitInfo.RowHandle;
                }
                
                var matrixAddress = new MatrixAddress
                {
                    Row = row,
                    Column = tableViewHitInfo.Column != null ? tableViewHitInfo.Column.FieldName : string.Empty
                };

                return matrixAddress;

            }

            return null;
        }
    }
}