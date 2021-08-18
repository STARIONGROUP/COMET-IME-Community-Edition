﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArchitectureDiagramDialog.xaml.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.Views
{
    using CDP4Common.CommonData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for ArchitectureDiagramDialog.xaml
    /// </summary>
    [ThingDialogViewExport(ClassKind.ArchitectureDiagram)]
    public partial class ArchitectureDiagramDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArchitectureDiagramDialog" /> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used by MEF to instation the view. The view is instantiated to enable navigation using the
        /// <see cref="IThingDialogNavigationService" />
        /// </remarks>
        public ArchitectureDiagramDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchitectureDiagramDialog" /> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService" />.
        /// </remarks>
        public ArchitectureDiagramDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}