// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementCreatePaletteItemViewModel.cs" company="RHEA System S.A.">
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
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;

    /// <summary>
    /// Diagram palette button responsible for creating new <see cref="Requirement" />
    /// </summary>
    [Export(typeof(IPaletteItemViewModel))]
    public class RequirementCreatePaletteItemViewModel : PaletteDroppableItemBaseViewModel
    {
        /// <summary>
        /// Gets the label text
        /// </summary>
        public override string Text
        {
            get { return "Requirement"; }
        }

        /// <summary>
        /// Gets the group sort order.Lower number = higher in the group.
        /// </summary>
        public override int GroupSortOrder
        {
            get { return 2000; }
        }

        /// <summary>
        /// Gets the button Icon string
        /// </summary>
        public override string Image
        {
            get { return "Customization_16x16.png"; }
        }

        /// <summary>
        /// Handle mouse move when dropping
        /// </summary>
        /// <param name="dropInfo">The mouse event args.</param>
        /// <param name="createCallback">Callback operation which creates the content</param>
        /// <returns>The callback to be invoked after drop has been handled</returns>
        public override async Task<Thing> HandleMouseDrop(IDropInfo dropInfo, Action<Thing> createCallback = null)
        {
            var contextMenuItems = this.editorViewModel.Thing.GetContainerOfType<Iteration>().RequirementsSpecification
                                                             .Select(s => new ContextMenuItemViewModel(s.Name, string.Empty, t => this.MenuItemCommand(t, createCallback), s, true));

            this.editorViewModel.ShowDropContextMenuOptions(contextMenuItems);

            return await Task.FromResult(default(Thing));
        }

        /// <summary>
        /// Method to be invoked when a manu item is selected by the user.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> associated with the menu item</param>
        /// <param name="createCallback">The callback to be invoked when a requirement has been specified</param>
        private void MenuItemCommand(Thing thing, Action<Thing> createCallback)
        {
            if (thing is null)
            {
                return;
            }

            var requirement = this.editorViewModel.Create<Requirement>(this, thing);

            if(requirement is null)
            {
                return;
            }

            createCallback?.Invoke(requirement);
        }
    }
}
