// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPaletteItemViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels.Palette
{
    using CDP4Composition.Diagram;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using CDP4Composition.Navigation;

    /// <summary>
    /// Interface for a a palette item (button)
    /// </summary>
    public interface IPaletteItemViewModel : IDisposable
    {
        /// <summary>
        /// Gets the label text
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets the group sort order.Lower number = higher in the group.
        /// </summary>
        int GroupSortOrder { get; }

        /// <summary>
        /// Gets the button Icon string
        /// </summary>
        string Image { get; }

        /// <summary>
        /// Gets the palette group this item belongs to
        /// </summary>
        PaletteGroup Group { get; }

        /// <summary>
        /// Gets the list of supported diagram types. When a supertype is listed all subtypes are also supported.
        /// </summary>
        List<Type> SupportedDiagramTypes { get; }

        /// <summary>
        /// Binds the editor and palette to the item
        /// </summary>
        /// <param name="editor">The editor</param>
        /// <param name="palette">The palette</param>
        void BindEditor(IDiagramEditorViewModel editor, DiagramPaletteViewModel palette);

        /// <summary>
        /// Executes the command of this <see cref="IPaletteItemViewModel" />
        /// </summary>
        /// <returns>Anempty task</returns>
        Task ExecuteAsyncCommand();
    }
}
