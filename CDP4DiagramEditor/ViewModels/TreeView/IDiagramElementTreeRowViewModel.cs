// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDiagramElementTreeRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels.TreeView
{
    using System;
    using CDP4Common.CommonData;

    using CDP4CommonView.Diagram;

    using CDP4Composition.Mvvm.Types;

    /// <summary>
    /// Interface for tree view rows in diagram control
    /// </summary>
    public interface IDiagramElementTreeRowViewModel : IDisposable
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of children diagram item rows from element tree view.
        /// </summary>
        DisposableReactiveList<IDiagramElementTreeRowViewModel> Children { get; set; }

        /// <summary>
        /// Gets or sets the represented Thing
        /// </summary>
        Thing Thing { get; set; }

        /// <summary>
        /// Gets or sets the diagram item view model
        /// </summary>
        IDiagramItemOrConnector ThingDiagramItemViewModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the view model is dirty
        /// </summary>
        bool IsDirty { get; set; }
    }
}
