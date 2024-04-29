// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridUpdateService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Services
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;

    using DevExpress.Xpf.Grid;

    using NLog;

    /// <summary>
    /// The service used to lock the update of a grid view when update in the view-model are occuring
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class GridUpdateService
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="DependencyProperty"/> used to lock the update in a grid
        /// </summary>
        public static readonly DependencyProperty UpdateStartedProperty = DependencyProperty.RegisterAttached("UpdateStarted", typeof(bool?), typeof(GridUpdateService), new FrameworkPropertyMetadata(false, UpdateStartedPropertyChanged));

        /// <summary>
        /// Sets the <see cref="UpdateStartedProperty"/> property
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> where the value is set</param>
        /// <param name="value">The <see cref="bool"/> value</param>
        public static void SetUpdateStarted(UIElement element, bool? value)
        {
            try
            {
                element.SetValue(UpdateStartedProperty, value);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"A problem occurend when SetUpdateStarted was called");
            }
        }

        /// <summary>
        /// Gets the <see cref="UpdateStartedProperty"/> property
        /// </summary>
        /// <param name="element">The <see cref="UIElement"/> where the value is used</param>
        /// <returns>A value indicating whether an update is occuring or not</returns>
        public static bool? GetUpdateStarted(UIElement element)
        {
            return (bool?)element.GetValue(UpdateStartedProperty);
        }

        /// <summary>
        /// The <see cref="DependencyPropertyChangedEventArgs"/> event-handler for the <see cref="UpdateStartedProperty"/> property
        /// </summary>
        /// <param name="source">The grid which visual and internal update should be prevented</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private static void UpdateStartedPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var grid = source as GridDataControlBase;

            if (grid == null)
            {
                throw new InvalidOperationException("The Service can only be used on GridDataControlBase view elements such as GridControl or TreeListControl.");
            }

            try
            {
                if ((bool?)e.NewValue == true)
                {
                    grid.BeginDataUpdate();
                }
                else if ((bool?)e.NewValue == false)
                {
                    var treeListControl = grid as TreeListControl;

                    if (treeListControl != null)
                    {
                        treeListControl.View.CancelRowEdit();
                        treeListControl.EndDataUpdate();
                    }
                    else
                    {
                        var gridControl = grid as GridControl;

                        if (gridControl != null && gridControl.DataController.IsUpdateLocked)
                        {
                            // cancel out of any active edit.
                            gridControl.View.CancelRowEdit();
                            grid.EndDataUpdate();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, $"A problem occurend when UpdateStartedPropertyChanged was called");
            }
        }
    }
}
