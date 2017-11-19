// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterizedCategoryRuleDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.Views
{
    using System.Windows;
    using BasicRdl.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Interaction logic for ParameterizedCategoryRuleDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.ParameterizedCategoryRule)]
    public partial class ParameterizedCategoryRuleDialog : IThingDialogView
    {
        /// <summary>
        /// The view model for this view.
        /// </summary>
        private ParameterizedCategoryRuleDialogViewModel viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedCategoryRuleDialog"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used by MEF to instation the view. The view is instantiated to enable navigation using the <see cref="IThingDialogNavigationService"/>
        /// </remarks>
        public ParameterizedCategoryRuleDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedCategoryRuleDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public ParameterizedCategoryRuleDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }

        /// <summary>
        /// Filters the ParameterTypesGrid according to the value of the ShowAllParameterTypes checkBox
        /// </summary>
        /// <param name="sender">
        /// The ParameterTypesGrid.
        /// </param>
        /// <param name="e">
        /// The <see cref="RowFilterEventArgs"/> event.
        /// </param>
        private void SelectedParameterTypeFilter(object sender, RowFilterEventArgs e)
        {
            if (this.viewModel == null)
            {
                this.viewModel = this.DataContext as ParameterizedCategoryRuleDialogViewModel;
            }

            if (this.ShowAllParameterTypes.IsChecked.Value)
            {
                e.Visible = true;
            }
            else
            {
                var row = this.ParameterTypesGrid.GetRow(e.ListSourceRowIndex) as ParameterType;
                e.Visible = this.viewModel.ParameterType.Contains(row);
            }
           
            e.Handled = !e.Visible;
        }

        /// <summary>
        /// Refresh the ParameterTypesGrid when the user clicks on the ShowAllParameterTypes checkBox
        /// </summary>
        /// <param name="sender">
        /// The ShowAllParameterTypes checkBox.
        /// </param>
        /// <param name="e">
        /// The click event on the ShowAllParameterTypes checkBox.
        /// </param>
        private void ChkCheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            this.ParameterTypesGrid.RefreshData();
        }
    }
}
