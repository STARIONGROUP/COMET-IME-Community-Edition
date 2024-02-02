// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramPaletteViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels.Palette
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Windows.Input;

    using CDP4Common.DiagramData;

    using CDP4Composition.Diagram;
    using CDP4Composition.Mvvm.Types;

    using CommonServiceLocator;

    using DevExpress.Xpf.NavBar;

    using ReactiveUI;

    /// <summary>
    /// The viewmodel responsible for the drawing palette
    /// </summary>
    public class DiagramPaletteViewModel : ReactiveObject
    {
        /// <summary>
        /// The map of all palette items available
        /// </summary>
        private readonly IEnumerable<IPaletteItemViewModel> paletteItems;

        /// <summary>
        /// The backing field for <see cref="Items" />
        /// </summary>
        private DisposableReactiveList<IPaletteItemViewModel> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramPaletteViewModel" /> class
        /// </summary>
        /// <param name="diagram">The diagram of the <see cref="Thing" />s to display</param>
        /// <param name="parent">The parent <see cref="DiagramEditorViewModel" /></param>
        public DiagramPaletteViewModel(DiagramCanvas diagram, IDiagramEditorViewModel parent)
        {
            this.paletteItems = ServiceLocator.Current.GetAllInstances<IPaletteItemViewModel>();

            this.Diagram = diagram;
            this.ParentViewModel = parent;

            this.Items = new DisposableReactiveList<IPaletteItemViewModel>();

            this.InitializePalette();
        }

        /// <summary>
        /// Gets the <see cref="DiagramCanvas" /> this palette is attached to
        /// </summary>
        public DiagramCanvas Diagram { get; private set; }

        /// <summary>
        /// Gets the <see cref="DiagramEditorViewModel" /> that this palette is part of.
        /// </summary>
        public IDiagramEditorViewModel ParentViewModel { get; private set; }

        /// <summary>
        /// Gets or sets he list of <see cref="IPaletteItemViewModel" /> to populate the menu
        /// </summary>
        public DisposableReactiveList<IPaletteItemViewModel> Items
        {
            get { return this.items; }
            private set { this.RaiseAndSetIfChanged(ref this.items, value); }
        }

        /// <summary>
        /// Gets or sets the on mouse move command
        /// </summary>
        public ReactiveCommand<MouseEventArgs, Unit> OnMouseMoveCommand { get; private set; }

        /// <summary>
        /// Initializes the palette
        /// </summary>
        private void InitializePalette()
        {
            // add items only relevant to the type of diagram loaded
            var relevantItems = this.paletteItems.Where(i => i.SupportedDiagramTypes.Select(sdt => sdt.IsInstanceOfType(this.Diagram)).Any(t => t));

            foreach (var paletteItemViewModel in relevantItems)
            {
                paletteItemViewModel.BindEditor(this.ParentViewModel, this);
                this.Items.Add(paletteItemViewModel);
            }

            this.InitializeCommands();
        }

        /// <summary>
        /// Initialize commands
        /// </summary>
        private void InitializeCommands()
        {
            this.OnMouseMoveCommand = ReactiveCommand.Create<MouseEventArgs>(this.ExecuteMouseMoveCommand);
        }

        /// <summary>
        /// Execute mousemove command
        /// </summary>
        /// <param name="e">MOuse move event args</param>
        private void ExecuteMouseMoveCommand(MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed || e.OriginalSource is not NavBarItemControl item)
            {
                return;
            }

            if (((NavBarItem) item.Content).Content is IPaletteDroppableItemViewModel itemViewModel)
            {
                itemViewModel.HandleMouseMove(e);
            }
        }
    }
}
