// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TableViewMenuBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Behaviour
{
    using DevExpress.Xpf.Grid;
    using System.Windows;
    using System.Windows.Input;
    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Utils.Design;
    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Core;


    /// <summary>
    /// Behavior defining manipulation of the column header context menus of the matrix
    /// </summary>
    /// <remarks>Sample modified from https://www.devexpress.com/Support/Center/Question/Details/Q515611/how-to-edit-wpf-grid-header-contextmneu </remarks>
    public class TableViewMenuBehavior : Behavior<TableView>
    {
        /// <summary>
        /// The Command property dependency property
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(TableViewMenuBehavior), null);

        /// <summary>
        /// The command
        /// </summary>
        public ICommand Command
        {
            get
            {
                return (ICommand)this.GetValue(CommandProperty);
            }
            set
            {
                this.SetValue(CommandProperty, value);
            }
        }

        /// <summary>
        /// The attaching view
        /// </summary>
        TableView View
        {
            get
            {
                return this.AssociatedObject;
            }
        }

        /// <summary>
        /// Behavior attachment callback 
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.View.ShowGridMenu += this.ShowGridMenu;
        }

        /// <summary>
        /// Show grid menu event handler
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event arguments</param>
        private void ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            if (e.MenuType != GridMenuType.Column)
            {
                return;
            }

            // Remove the Column Chooser menu item.
            e.Customizations.Add(new RemoveBarItemAndLinkAction()
            {
                ItemName = DefaultColumnMenuItemNames.ColumnChooser
            });

            // Create a custom menu item and add it to the context menu.
            var bi = new BarButtonItem();

            bi.Name = "toggleHighlight";
            bi.Content = "Toggle Highlight";
            bi.Command = this.Command;
            bi.CommandParameter = e.MenuInfo.Column;

            bi.Glyph = DXImageHelper.GetImageSource(@"Images/Conditional Formatting/HighlightCellsRules_16x16.png");

            bi.SetValue(BarItemLinkActionBase.ItemLinkIndexProperty, 0);

            e.Customizations.Add(bi);
        }
    }
}
