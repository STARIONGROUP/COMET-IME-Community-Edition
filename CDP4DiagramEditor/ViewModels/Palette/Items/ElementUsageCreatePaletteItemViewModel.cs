// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageCreatePaletteItemViewModel.cs" company="RHEA System S.A.">
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
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Diagram palette button responsible for creating new <see cref="Requirement" />
    /// </summary>
    [Export(typeof(IPaletteItemViewModel))]
    public class ElementUsageCreatePaletteItemViewModel : PaletteItemBaseViewModel
    {
        /// <summary>
        /// Gets the label text
        /// </summary>
        public override string Text
        {
            get { return "Element Usage"; }
        }

        /// <summary>
        /// Gets the list of supported diagram types. When a supertype is listed all subtypes are also supported.
        /// </summary>
        public override List<Type> SupportedDiagramTypes
        {
            get { return new() { typeof(DiagramCanvas) }; }
        }

        /// <summary>
        /// Gets the group sort order. Lower number = higher in the group.
        /// </summary>
        public override int GroupSortOrder
        {
            get { return 1000; }
        }

        /// <summary>
        /// Gets the palette group this item belongs to
        /// </summary>
        public override PaletteGroup Group
        {
            get { return PaletteGroup.Connect; }
        }

        /// <summary>
        /// Gets the button Icon string
        /// </summary>
        public override string Image
        {
            get { return "Version_16x16.png"; }
        }

        /// <summary>
        /// Executes the command of this <see cref="IPaletteItemViewModel" />
        /// </summary>
        /// <returns>An empty task</returns>
        public override async Task ExecuteAsyncCommand()
        {
        }
    }
}
