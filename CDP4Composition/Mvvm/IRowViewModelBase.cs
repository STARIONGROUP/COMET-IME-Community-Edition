// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRowViewModelBase.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.Mvvm
{
    using System.ComponentModel;
    using CDP4Common.CommonData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;

    using CDP4Dal.Permission;

    /// <summary>
    /// The interface for the row-view-model
    /// </summary>
    /// <typeparam name="T">The <see cref="Thing"/> represented by the row</typeparam>
    public interface IRowViewModelBase<out T> : IViewModelBase<T>, IDragSource, IDataErrorInfo, IHaveContainerViewModel, IHaveContainedRows where T : Thing
    {
        /// <summary>
        /// Gets or sets the index of the row
        /// </summary>
        /// <remarks>
        /// this property is used in the case of <see cref="OrderedItemList{T}"/>
        /// </remarks>
        int Index { get; set; }

        /// <summary>
        /// Gets the top container <see cref="IViewModelBase{T}"/>
        /// </summary>
        /// <remarks>
        /// this should either be a <see cref="IDialogViewModelBase{T}"/> or a <see cref="IBrowserViewModelBase{T}"/>
        /// </remarks>
        IViewModelBase<Thing> TopContainerViewModel { get; }

        /// <summary>
        /// Creates, update and write a clone in the data-source when inline-editing with a new value for one of its property
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="fieldName">The property name</param>
        void CreateCloneAndWrite(object newValue, string fieldName);

        /// <summary>
        /// Persist the updated <see cref="Thing"/> after an inline edit.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        void EndInlineEdit(Thing thing);

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">The name of the property whose error message to get</param>
        /// <param name="newValue">The new value for the row</param>
        /// <returns>The error message for the property. The default is an empty string ("").</returns>
        /// <remarks>
        /// Used when inline-editing, the values are updated on focus lost
        /// </remarks>
        string ValidateProperty(string columnName, object newValue);

        /// <summary>
        /// Clears the row highlighting for itself and its children.
        /// </summary>
        void ClearRowHighlighting();

        /// <summary>
        /// Computes for the entire row or for a specific property of the row whether it is editable based on the
        /// result of the <see cref="PermissionService.CanWrite"/> method and potential conditions of the property of the Row that is being edited.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property for which the value is computed. This allows to include the
        /// specific property of the row-view-model in the computation. If the propertyname is empty
        /// then the whole row is taken into account. If a property is specified only that property
        /// is taken into account.
        /// </param>
        /// <returns>
        /// True if the row or the more specific the property is editable or not.
        /// </returns>
        bool IsEditable(string propertyName = "");
        
        /// <summary>
        /// Sets a value indicating that the row is expanded
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// Expands the current row and all contained rows along the containment hierarchy
        /// </summary>
        void ExpandAllRows();

        /// <summary>
        /// Collapases the current row and all contained rows along the containment hierarchy
        /// </summary>
        void CollapseAllRows();
    }
}