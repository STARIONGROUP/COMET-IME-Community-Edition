// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherDiagramControl.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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
namespace CDP4Grapher.Views
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Grapher.Utilities;
    using CDP4Grapher.ViewModels;

    using DevExpress.Data.Helpers;
    using DevExpress.Utils.Svg;
    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Core.Native;
    using DevExpress.Xpf.Diagram;

    using File = CDP4Common.EngineeringModelData.File;

    /// <summary>
    /// Interaction logic for GrapherDiagramControl.xaml
    /// </summary>
    public partial class GrapherDiagramControl : DiagramControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Grapher"/> class
        /// </summary>
        public GrapherDiagramControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Overrides the Dev-Express context-menu
        /// </summary>
        /// <returns>The context-menu</returns>
        protected override IEnumerable<IBarManagerControllerAction> CreateContextMenu() => ((IGrapherViewModel)this.DataContext).DiagramContextMenuViewModel.ContextMenu;
    }
}
