// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramControl.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Common.CommonData;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm;

    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Interaction logic for Cdp4DiagramControl
    /// </summary>
    public class Cdp4DiagramControl : DiagramDesignerControl
    {
        /// <summary>
        /// The dependency property that allows setting the save <see cref="ICommand" />
        /// </summary>
        public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(Cdp4DiagramControl), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the save <see cref="ICommand" />
        /// </summary>
        public ICommand SaveCommand
        {
            get { return (ICommand)this.GetValue(SaveCommandProperty); }
            set { this.SetValue(SaveCommandProperty, value); }
        }

        /// <summary>
        /// Create appropriate connector
        /// </summary>
        /// <returns>A <see cref="DiagramConnector"/></returns>
        protected override DiagramConnector CreateConnector()
        {
            if (this.ActiveTool is IConnectorTool tool)
            {
                return (DiagramConnector)tool.GetConnector;
            }

            return new DiagramConnector();
        }

        /// <summary>
        /// Override the Dev-Express context-menu
        /// </summary>
        /// <returns>The context-menu</returns>
        protected override IEnumerable<IBarManagerControllerAction> CreateContextMenu()
        {
            var browser = (IBrowserViewModelBase<Thing>)this.DataContext;

            foreach (var contextMenuItemViewModel in browser.ContextMenu)
            {
                yield return this.CreateContextMenuItem(contextMenuItemViewModel);
            }
        }

        /// <summary>
        /// Create menu with subitems
        /// </summary>
        /// <param name="contextMenuItemViewModel">The context menu vm</param>
        /// <returns>The bar item</returns>
        private IBarManagerControllerAction CreateContextMenuItem(ContextMenuItemViewModel contextMenuItemViewModel)
        {
            if (contextMenuItemViewModel.SubMenu != null && contextMenuItemViewModel.SubMenu.Any())
            {
                var subitem = new BarSubItem
                {
                    DataContext = contextMenuItemViewModel
                };

                foreach (var menuItemViewModel in contextMenuItemViewModel.SubMenu)
                {
                    subitem.Items.Add((IBarItem)this.CreateContextMenuItem(menuItemViewModel));
                }

                return subitem;
            }

            return new BarButtonItem
            {
                DataContext = contextMenuItemViewModel
            };
        }

        /// <summary>
        /// Override the behaviour of the quick access save button
        /// </summary>
        /// <param name="e">The <see cref="DiagramShowingSaveDialogEventArgs" /></param>
        protected override void OnShowingSaveDialog(DiagramShowingSaveDialogEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Override the behaviour of the quick access save button
        /// </summary>
        /// <param name="e">The <see cref="DiagramShowingSaveDialogEventArgs" /></param>
        protected override void OnCustomSaveDocument(DiagramCustomSaveDocumentEventArgs e)
        {
            this.SaveCommand.Execute(null);
        }
    }
}
