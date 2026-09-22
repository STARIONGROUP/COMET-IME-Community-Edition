// -------------------------------------------------------------------------------------------------
// <copyright file="CommonThingControl.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using System.Collections;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.ViewModels;

    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Grid;

    using NLog;

    using ServiceLocator = CommonServiceLocator.ServiceLocator;

    /// <summary>
    /// Interaction logic for CommonThingControl
    /// </summary>
    public partial class CommonThingControl
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="FilteringMode"/> setter method.
        /// </summary>
        private static readonly DependencyProperty FilteringModeProperty = DependencyProperty.Register("FilteringMode", typeof(TreeListFilteringMode?), typeof(CommonThingControl));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="GridView"/> setter method.
        /// </summary>
        private static readonly DependencyProperty GridViewProperty = DependencyProperty.Register("GridView", typeof(DataViewBase), typeof(CommonThingControl), new PropertyMetadata(OnGridViewChanged));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="IsFavoriteToggleVisible"/> setter method.
        /// </summary>
        private static readonly DependencyProperty IsFavoriteToggleVisibleProperty = DependencyProperty.Register("IsFavoriteToggleVisible", typeof(bool), typeof(CommonThingControl));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="IsExportVisible"/> setter method.
        /// </summary>
        private static readonly DependencyProperty IsExportVisibleProperty = DependencyProperty.Register("IsExportVisible", typeof(bool), typeof(CommonThingControl));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="ExportHint"/> setter method.
        /// </summary>
        private static readonly DependencyProperty ExportHintProperty = DependencyProperty.Register("ExportHint", typeof(string), typeof(CommonThingControl));

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonThingControl"/> class.
        /// </summary>
        public CommonThingControl()
        {
            this.InitializeComponent();
            this.IsFavoriteToggleVisible = false;
            this.AdditionalBarItems.CollectionChanged += this.OnAdditionalBarItemsChanged;
        }

        /// <summary>
        /// Gets the extra bar items a consuming browser appends to the standard toolbar, so a browser that needs its own
        /// buttons (view pickers, an analysis check, ...) keeps them in the single shared toolbar rather than a toolbar
        /// of its own. Declared in the consumer's XAML as the content of this property.
        /// </summary>
        public ObservableCollection<object> AdditionalBarItems { get; } = new ObservableCollection<object>();

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used to navigate to a <see cref="IDialogViewModel"/>
        /// </summary>
        public IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Appends bar items added to <see cref="AdditionalBarItems"/> to the shared toolbar, in the order they are
        /// declared, after the standard buttons.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/>.</param>
        private void OnAdditionalBarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
            {
                return;
            }

            foreach (var item in e.NewItems)
            {
                // Items is a non-generic bar-item collection; the IList cast lets an object-typed bar item be appended.
                ((IList)this.BarControl.Items).Add(item);
            }
        }

        /// <summary>
        /// The <see cref="FilteringMode"/> this <see cref="CommonThingControl"/> is associated with
        /// </summary>
        public TreeListFilteringMode? FilteringMode
        {
            get => this.GetValue(FilteringModeProperty) as TreeListFilteringMode?;
            set => this.SetValue(FilteringModeProperty, value);
        }

        /// <summary>
        /// The <see cref="GridView"/> this <see cref="CommonThingControl"/> is associated with
        /// </summary>
        public GridDataViewBase GridView
        {
            get => this.GetValue(GridViewProperty) as GridDataViewBase;
            set => this.SetValue(GridViewProperty, value);
        }

        /// <summary>
        /// The boolean that enables or disables the visibility of the favorites toggle buttons.
        /// </summary>
        public bool IsFavoriteToggleVisible
        {
            get => this.GetValue(IsFavoriteToggleVisibleProperty) is bool && (bool) this.GetValue(IsFavoriteToggleVisibleProperty);
            set => this.SetValue(IsFavoriteToggleVisibleProperty, value);
        }

        /// <summary>
        /// The boolean that enables or disables the visibility of the export button. It defaults to false, so a
        /// browser that does not export keeps the toolbar it has always had; a browser that does opts in and gets the
        /// export button in its standard place rather than a toolbar of its own.
        /// </summary>
        public bool IsExportVisible
        {
            get => this.GetValue(IsExportVisibleProperty) is bool && (bool) this.GetValue(IsExportVisibleProperty);
            set => this.SetValue(IsExportVisibleProperty, value);
        }

        /// <summary>
        /// The tooltip shown on the export button. A browser that opts into the export button (see
        /// <see cref="IsExportVisible"/>) sets the wording that describes what it exports, e.g. "Export the VCD to
        /// Excel".
        /// </summary>
        public string ExportHint
        {
            get => (string) this.GetValue(ExportHintProperty);
            set => this.SetValue(ExportHintProperty, value);
        }
        
        /// <summary>
        /// Executes when <see cref="GridView"/> property changes.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/></param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private static void OnGridViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommonThingControl commonThingControl)
            {
                SetGridViewProperties(commonThingControl);
            } 
        }

        /// <summary>
        /// Sets the <see cref="DataViewBase.AllowFilterEditorProperty"/> based on the fact if a custom filtereditor control should be used.
        /// If a custom filtereditor control is used, the default filtereditor control should not be used anymore.
        /// </summary>
        /// <param name="commonThingControl">The <see cref="CommonThingControl"/></param>
        private static void SetGridViewProperties(CommonThingControl commonThingControl)
        {
            commonThingControl.GridView.ShowGridMenu += GridView_ShowGridMenu;

            if (commonThingControl.GridView is TreeListView treeListView)
            {
                treeListView.SetValue(
                    TreeListView.FilteringModeProperty, 
                    commonThingControl.FilteringMode ?? TreeListFilteringMode.EntireBranch);

                treeListView.SetValue(TreeListView.EnableDynamicLoadingProperty, false);
            }
        }

        /// <summary>
        /// Customize grid menu.
        ///     - Remove FilterEditor option from column context menu
        /// </summary>
        /// <param name="sender">The sender <see cref="GridDataViewBase"/></param>
        /// <param name="e">The <see cref="GridMenuEventArgs"/></param>
        private static void GridView_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            if (e.MenuType == GridMenuType.Column) 
            {
                e.Customizations.Add(new RemoveAction { ElementName = DefaultColumnMenuItemNamesBase.FilterEditor });
            }
        }

        /// <summary>
        /// Handles the OpenFilterPanel button click.
        /// Opens a default or custom Filter panel
        /// </summary>
        /// <param name="sender">The <see cref="sender"/></param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/></param>
        private void OpenFilterPanel(object sender, ItemClickEventArgs e)
        {
            if (this.GridView != null)
            {
                if (this.DialogNavigationService == null)
                {
                    this.DialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
                }

                var customFilterEditorDialog = new CustomFilterEditorDialogViewModel(this.DialogNavigationService, this.GridView);

                this.DialogNavigationService.NavigateModal(customFilterEditorDialog);
            }
        }

        /// <summary>
        /// Handles the ToggleSearchPanel button click.
        /// Opens a default or custom Filter panel
        /// </summary>
        /// <param name="sender">The <see cref="sender"/></param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/></param>
        private void ToggleSearchPanel(object sender, ItemClickEventArgs e)
        {
            if (this.GridView != null)
            {
                if (this.GridView.ActualShowSearchPanel)
                {
                    this.GridView.HideSearchPanel();
                }
                else
                {
                    this.GridView.ShowSearchPanel(true);
                }
            }
        }
    }
}