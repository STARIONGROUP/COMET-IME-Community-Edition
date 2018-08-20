// -------------------------------------------------------------------------------------------------
// <copyright file="TreeCellShowingEditorBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    /// The purpose of the <see cref="TreeCellShowingEditorBehavior"/> is to determine whether or not the 
    /// row or cell is editable or not. The determination of this is delegated to the datacontext, the row-view-model that is bound to the row, 
    /// by invoking the <see cref="IRowViewModelBase{Thing}.IsEditable"/> mehtod.
    /// </summary>
    /// <remarks>
    /// This <see cref="Behavior"/> can be attached to a <see cref="TreeListView"/> and works only in combination with a data context
    /// that implements the <see cref="IRowViewModelBase{Thing}"/> interface
    /// </remarks>
    public class TreeCellShowingEditorBehavior : Behavior<TreeListView>
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The event handler that is raised when the <see cref="Behavior"/> is attached
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.ShowingEditor += this.EnableOrDisableShowingEditor;
        }

        /// <summary>
        /// The event handler that is raised when the <see cref="Behavior"/> is detached
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.ShowingEditor -= this.EnableOrDisableShowingEditor;
        }

        /// <summary>
        /// Enable or disable showing the inline editor for the selected cell
        /// </summary>
        /// <param name="sender">
        /// The Grid view that is being edited.
        /// </param>
        /// <param name="e">
        /// The cell value changed event.
        /// </param>
        private void EnableOrDisableShowingEditor(object sender, TreeListShowingEditorEventArgs e)
        {
            try
            {
                var row = e.Node.Content as IRowViewModelBase<Thing>;
                if (row == null)
                {
                    e.Cancel = true;
                    return;
                }

                e.Cancel = !row.IsEditable(e.Column.FieldName);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "A problem occurend when executing EnableOrDisableShowingEditor");
            }
        }
    }
}