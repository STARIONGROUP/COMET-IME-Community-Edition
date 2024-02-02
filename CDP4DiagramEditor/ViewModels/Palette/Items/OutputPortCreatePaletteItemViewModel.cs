﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputPortCreatePaletteItemViewModel.cs" company="RHEA System S.A.">
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
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Diagram palette button responsible for creating new <see cref="Requirement" />
    /// </summary>
    [Export(typeof(IPaletteItemViewModel))]
    public class OutputPortCreatePaletteItemViewModel : PortCreateBasePaletteItemViewModel
    {
        /// <summary>
        /// Gets the label text
        /// </summary>
        public override string Text
        {
            get { return "Output Port"; }
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
            get { return "GoToNextHeaderFooter_16x16.png"; }
        }

        /// <summary>
        /// Executes the command of this <see cref="IPaletteItemViewModel" />
        /// </summary>
        /// <returns>An empty task</returns>
        public override async Task ExecuteAsyncCommand()
        {
            await this.CreatePort(InterfaceEndKind.OUTPUT);
        }
    }
}