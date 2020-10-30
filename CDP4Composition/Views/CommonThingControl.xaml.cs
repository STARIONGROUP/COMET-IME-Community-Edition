// -------------------------------------------------------------------------------------------------
// <copyright file="CommonThingControl.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smieckowski
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using DevExpress.Xpf.Grid;

    using System.Windows;

    using DevExpress.Utils;
    using DevExpress.Xpf.Bars;

    using NLog;

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
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="GridView"/> setter method.
        /// </summary>
        private static readonly DependencyProperty GridViewProperty = DependencyProperty.Register("GridView", typeof(GridDataViewBase), typeof(CommonThingControl), new PropertyMetadata(OnGridViewChanged));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="IsFavoriteToggleVisible"/> setter method.
        /// </summary>
        private static readonly DependencyProperty IsFavoriteToggleVisibleProperty = DependencyProperty.Register("IsFavoriteToggleVisible", typeof(bool), typeof(CommonThingControl));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="HasCustomFilterPanel"/> setter method.
        /// </summary>
        private static readonly DependencyProperty HasCustomFilterPanelProperty = DependencyProperty.Register("HasCustomFilterPanel", typeof(bool), typeof(CommonThingControl), new PropertyMetadata(OnHasCustomFilterPanelChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonThingControl"/> class.
        /// </summary>
        public CommonThingControl()
        {
            this.InitializeComponent();
            this.IsFavoriteToggleVisible = false;
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
        /// The boolean that enables or disables the visibility of a custom FilterPanel instead of the default one.
        /// </summary>
        public bool HasCustomFilterPanel
        {
            get => this.GetValue(HasCustomFilterPanelProperty) is bool && (bool) this.GetValue(HasCustomFilterPanelProperty);
            set => this.SetValue(HasCustomFilterPanelProperty, value);
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
                SetAllowFilterEditorProperty(commonThingControl, commonThingControl.HasCustomFilterPanel);
            } 
        }

        /// <summary>
        /// Executes when <see cref="HasCustomFilterPanelProperty"/> value changes.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/></param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private static void OnHasCustomFilterPanelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommonThingControl commonThingControl)
            {
                SetAllowFilterEditorProperty(commonThingControl, (bool)e.NewValue);
            }
        }

        /// <summary>
        /// Sets the <see cref="DataViewBase.AllowFilterEditorProperty"/> based on the fact if a custom filtereditor control should be used.
        /// If a custom filtereditor control is used, the default filtereditor control should not be used anymore.
        /// </summary>
        /// <param name="commonThingControl">The <see cref="CommonThingControl"/></param>
        /// <param name="newValue">The new </param>
        private static void SetAllowFilterEditorProperty(CommonThingControl commonThingControl, bool newValue)
        {
            var defaultBoolean = newValue ? DefaultBoolean.True : DefaultBoolean.False;
            commonThingControl.GridView?.SetValue(DataViewBase.AllowFilterEditorProperty, defaultBoolean);
        }

        /// <summary>
        /// Handles the OpenFilterPanel button click.
        /// Opens a default or custom Filter panel
        /// </summary>
        /// <param name="sender">The <see cref="sender"/></param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/></param>
        private void OpenFilterPanel(object sender, ItemClickEventArgs e)
        {   
            if (!this.HasCustomFilterPanel)
            {
                this.GridView.ShowFilterEditor(null);
            }
            else
            {
                var customFilterEditorDialog = new CustomFilterEditorDialog(this.GridView.DataControl.FilteringContext);
                customFilterEditorDialog.DataContext = this.DataContext;
                customFilterEditorDialog.ShowDialog();
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