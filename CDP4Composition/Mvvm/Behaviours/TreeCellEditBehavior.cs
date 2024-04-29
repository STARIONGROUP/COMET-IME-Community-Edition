// -------------------------------------------------------------------------------------------------
// <copyright file="TreeCellEditBehavior.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Composition.Mvvm;
    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;
    using NLog;

    /// <summary>
    /// The cell edit helper for TreeListView.
    /// </summary>
    public class TreeCellEditBehavior : Behavior<TreeListView>
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.CellValueChanged += this.SaveCellValue;
            this.AssociatedObject.ValidateCell += this.ValidateCell;
        }
        
        /// <summary>
        /// Remove the subscription on detaching
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.CellValueChanged -= this.SaveCellValue;
            this.AssociatedObject.ValidateCell -= this.ValidateCell;
        }
        
        /// <summary>
        /// Validate the cell value.
        /// </summary>
        /// <param name="sender">
        /// The Grid view that is being edited.
        /// </param>
        /// <param name="e">
        /// The cell validation event.
        /// </param>
        private void ValidateCell(object sender, TreeListCellValidationEventArgs e)
        {
            try
            {
                var row = e.Row as IRowViewModelBase<Thing>;
                if (row == null)
                {
                    return;
                }

                var error = row.ValidateProperty(e.Column.FieldName, e.Value);
                if (error != null)
                {
                    e.IsValid = false;
                    e.ErrorContent = error;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "A problem occurend when executing ValidateCell");
            }
        }

        /// <summary>
        /// Persist the new value of the cell.
        /// </summary>
        /// <param name="sender">
        /// The Grid view that is being edited.
        /// </param>
        /// <param name="e">
        /// The cell value changed event.
        /// </param>
        private void SaveCellValue(object sender, TreeListCellValueChangedEventArgs e)
        {
            try
            {
                var row = e.Row as IRowViewModelBase<Thing>;
                if (row == null)
                {
                    return;
                }

                row.CreateCloneAndWrite(e.Value, e.Column.FieldName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "A problem occurend when executing SaveCellValue");
            }
        }
    }
}