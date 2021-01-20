// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingCellReadOnlyBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ReferenceDataMapper.Behaviors
{
    using System.Data;
    using System.Windows;

    using CDP4ReferenceDataMapper.Managers;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    /// <summary>
    /// Adds behavior to a <see cref="TreeListView"/> that handles setting the readonly property of an Editor control.
    /// When a column is edittable and allowing to edit of cells is conditional for individual rows, we should set the
    /// editor control's readonly property to true for the cells we don't want to allow editting for.
    /// </summary>
    public class MappingCellReadOnlyBehavior : Behavior<TreeListView>
    {
        public static readonly DependencyProperty DataSourceManagerProperty =
            DependencyProperty.Register(
                "DataSourceManager",
                typeof(DataSourceManager),
                typeof(MappingCellReadOnlyBehavior));

        public DataSourceManager DataSourceManager
        {
            get => (DataSourceManager)this.GetValue(DataSourceManagerProperty);
            set => this.SetValue(DataSourceManagerProperty, value);
        }

        /// <summary>
        /// Executes when this behavior gets attached to its UI component
        /// </summary>
        protected override void OnAttached() 
        {
            base.OnAttached();
            this.AssociatedObject.ShownEditor += this.AssociatedObject_ShownEditor;
        }

        /// <summary>
        /// Executes when this behavior is detaching from its UI component
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.ShownEditor -= this.AssociatedObject_ShownEditor;
        }

        /// <summary>
        /// Checks if the Editor in the <see cref="TreeListView"/> should be shown or not, using the Editor's readonly property.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="TreeListEditorEventArgs"/></param>
        private void AssociatedObject_ShownEditor(object sender, TreeListEditorEventArgs e)
        {
            var isReadOnly = true;

            if (sender is TreeListView dataViewBase)
            {
                if (dataViewBase.DataControl.CurrentItem is DataRowView dataRowView)
                {
                    if (this.DataSourceManager.IsActualStateColumn(e.Column.FieldName))
                    {
                        if (dataRowView[DataSourceManager.TypeColumnName].ToString() == DataSourceManager.ParameterMappingType)
                        {
                            isReadOnly = false;
                        }
                    }
                }
            }

            e.Editor.IsReadOnly = isReadOnly;
        }
    }
}
