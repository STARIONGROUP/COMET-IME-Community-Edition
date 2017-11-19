﻿// -------------------------------------------------------------------------------------------------
// <copyright file="CellEditBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using CDP4Common.CommonData;
    using CDP4Composition.Mvvm;
    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// The cell edit helper.
    /// </summary>
    public class CellEditBehavior : Behavior<GridViewBase>
    {
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
        private void ValidateCell(object sender, GridCellValidationEventArgs e)
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

        /// <summary>
        /// Persist the new value of the cell.
        /// </summary>
        /// <param name="sender">
        /// The Grid view that is being edited.
        /// </param>
        /// <param name="e">
        /// The cell value changed event.
        /// </param>
        private void SaveCellValue(object sender, CellValueChangedEventArgs e)
        {
            var row = e.Row as IRowViewModelBase<Thing>;
            if (row == null)
            {
                return;
            }

            row.CreateCloneAndWrite(e.Value, e.Column.FieldName);
        }
    }
}
