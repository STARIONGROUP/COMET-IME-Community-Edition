// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PaletteItemBaseViewModel.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.DiagramData;

    using CDP4Composition.Diagram;

    using ReactiveUI;

    /// <summary>
    /// Diagram Palette button item base viewmodel.
    /// </summary>
    public abstract class PaletteItemBaseViewModel : ReactiveObject, IPaletteItemViewModel
    {
        /// <summary>
        /// The viewmodel of the attached editor
        /// </summary>
        protected IDiagramEditorViewModel editorViewModel;

        /// <summary>
        /// The viewmodel of the palette viewmodel
        /// </summary>
        protected DiagramPaletteViewModel paletteViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteItemBaseViewModel" /> class
        /// </summary>
        protected PaletteItemBaseViewModel()
        {
            this.AsyncCommand = ReactiveCommand.CreateAsyncTask(_ => this.ExecuteAsyncCommand());
        }

        /// <summary>
        /// The main command of the item
        /// </summary>
        public ReactiveCommand<Unit> AsyncCommand { get; private set; }

        /// <summary>
        /// Gets the label text
        /// </summary>
        public virtual string Text
        {
            get { return "New Item"; }
        }

        /// <summary>
        /// Gets the group sort order.Lower number = higher in the group.
        /// </summary>
        public virtual int GroupSortOrder
        {
            get { return 1000; }
        }

        /// <summary>
        /// Gets the button Icon string
        /// </summary>
        public virtual string Image
        {
            get { return "Product_16x16.png"; }
        }

        /// <summary>
        /// Gets the palette group this item belongs to
        /// </summary>
        public virtual PaletteGroup Group
        {
            get { return PaletteGroup.Create; }
        }

        /// <summary>
        /// Gets the list of supported diagram types. When a supertype is listed all subtypes are also supported.
        /// </summary>
        public virtual List<Type> SupportedDiagramTypes
        {
            get { return new() { typeof(DiagramCanvas) }; }
        }

        /// <summary>
        /// Executes the command of this <see cref="IPaletteItemViewModel" />
        /// </summary>
        /// <returns>Anempty task</returns>
        public abstract Task ExecuteAsyncCommand();

        /// <summary>
        /// Binds the editor viewmodel to the palette item.
        /// </summary>
        /// <param name="editor">The editor viewmodel.</param>
        /// <param name="palette">The palette</param>
        public void BindEditor(IDiagramEditorViewModel editor, DiagramPaletteViewModel palette)
        {
            this.editorViewModel = editor ?? throw new ArgumentNullException(nameof(editor), $"Diagram editor may not be null");
            this.paletteViewModel = palette ?? throw new ArgumentNullException(nameof(palette), $"Palette may not be null");
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            this.AsyncCommand?.Dispose();
        }
    }
}
