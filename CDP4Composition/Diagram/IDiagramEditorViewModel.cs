// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDiagramEditorViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Diagram
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4CommonView.Diagram;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;

    using DevExpress.Diagram.Core;

    /// <summary>
    /// The interface that describes the dirty mechanism of DiagramEditorViewModel
    /// </summary>
    public interface IDiagramEditorViewModel : IViewModelBase<DiagramCanvas>, ICdp4DiagramContainer, IISession
    {
        /// <summary>
        /// Defines the method that update <see cref="IsDirty"/> property
        /// </summary>
        void UpdateIsDirty();

        /// <summary>
        /// Defines the <see cref="IsDirty"/> property
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Gets or sets the collection of diagram items.
        /// </summary>
        DisposableReactiveList<IThingDiagramItemViewModel> ThingDiagramItemViewModels { get; set; }
        DisposableReactiveList<IDiagramConnectorViewModel> ConnectorViewModels { get; set; }

        /// <summary>
        /// Removes a diagram item and its connectors.
        /// </summary>
        /// <param name="contentItemContent">The item to remove.</param>
        void RemoveDiagramThingItem(object contentItemContent);

        /// <summary>
        /// Removes a diagram item and its connectors by <see cref="Thing"/>.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> by which to find and remove diagram things.</param>
        void RemoveDiagramThingItemByThing(Thing thing);

        /// <summary>
        /// Initiate the create command of a certain Thing represented by T
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="container">The contaier of the object to be created</param>
        /// <typeparam name="TThing">The type of Thing to be creates</typeparam>
        TThing Create<TThing>(object sender, Thing container = null) where TThing : Thing, new();

        /// <summary>
        /// Activate a connector tool.
        /// </summary>
        /// <typeparam name="TTool">The type of tool</typeparam>
        /// <param name="sender">The sender object.</param>
        /// <returns>An empty task</returns>
        Task ActivateConnectorTool<TTool>(object sender) where TTool : DiagramTool, IConnectorTool, new();

        /// <summary>
        /// Shows a context menu in the diagram at the current mouse position with the specified options
        /// </summary>
        /// <param name="contextMenuItems">The menu options to display</param>
        void ShowDropContextMenuOptions(IEnumerable<ContextMenuItemViewModel> contextMenuItems);
    }
}
