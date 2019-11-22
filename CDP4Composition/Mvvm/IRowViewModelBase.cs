// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRowViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System.ComponentModel;
    using CDP4Common.CommonData;
    using CDP4Composition.DragDrop;
    using ReactiveUI;

    /// <summary>
    /// The interface for the row-view-model
    /// </summary>
    /// <typeparam name="T">The <see cref="Thing"/> represented by the row</typeparam>
    public interface IRowViewModelBase<out T> : IViewModelBase<T>, IDragSource, IDataErrorInfo where T : Thing
    {
        /// <summary>
        /// Gets or sets the index of the row
        /// </summary>
        /// <remarks>
        /// this property is used in the case of <see cref="OrderedItemList{T}"/>
        /// </remarks>
        int Index { get; set; }

        /// <summary>
        /// Gets the Contained <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        ReactiveList<IRowViewModelBase<Thing>> ContainedRows { get; }

        /// <summary>
        /// Gets the container <see cref="IViewModelBase{T}"/>
        /// </summary>
        IViewModelBase<Thing> ContainerViewModel { get; }

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
        /// Sets a value indicating that the row is hidden
        /// </summary>
        bool IsHidden { get; }

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