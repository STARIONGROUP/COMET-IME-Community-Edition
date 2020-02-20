// -------------------------------------------------------------------------------------------------
// <copyright file="ModelOpeningDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4ShellDialogs.ViewModels;
    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Interaction logic for ModelOpeningDialogViewModel.xaml
    /// </summary>
    [DialogViewExport("ModelOpeningDialogViewModel", "The Engineering Model Setup Iteration Selection")]
    public partial class ModelOpeningDialog : DXWindow, IDialogView
    {
        public ModelOpeningDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelOpeningDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ModelOpeningDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }

    /// <summary>
    /// The Template Selector
    /// </summary>
    public class CellTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var containerElement = (FrameworkElement)container;
            var cellData = (GridCellData)item;

            if (cellData != null)
            {
                var rowObject = cellData.RowData.Row;
                if (rowObject is ModelSelectionIterationSetupRowViewModel)
                {
                    return (DataTemplate)containerElement.FindResource("iterationSetupDomainOfExpertiseTemplate");
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}