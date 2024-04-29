// -------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramControl.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Composition.Mvvm;
    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Interaction logic for Cdp4DiagramControl
    /// </summary>
    public partial class Cdp4DiagramControl : DiagramDesignerControl
    {
        /// <summary>
        /// The dependency property that allows setting the save <see cref="ICommand"/>
        /// </summary>
        public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register("SaveCommand", typeof(ICommand), typeof(Cdp4DiagramControl), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the save <see cref="ICommand"/>
        /// </summary>
        public ICommand SaveCommand
        {
            get { return (ICommand)this.GetValue(SaveCommandProperty); }
            set { this.SetValue(SaveCommandProperty, value); }
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
                yield return new BarButtonItem
                {
                    DataContext = contextMenuItemViewModel
                };
            }
        }

        /// <summary>
        /// Override the behaviour of the quick access save button
        /// </summary>
        /// <param name="e">The <see cref="DiagramShowingSaveDialogEventArgs"/></param>
        protected override void OnShowingSaveDialog(DiagramShowingSaveDialogEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// Override the behaviour of the quick access save button
        /// </summary>
        /// <param name="e">The <see cref="DiagramShowingSaveDialogEventArgs"/></param>
        protected override void OnCustomSaveDocument(DiagramCustomSaveDocumentEventArgs e)
        {
            this.SaveCommand.Execute(null);
        }
    }
}