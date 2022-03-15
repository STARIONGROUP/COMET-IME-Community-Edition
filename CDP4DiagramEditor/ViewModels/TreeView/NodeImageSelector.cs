// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeImageSelector.cs" company="RHEA System S.A.">
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
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;

    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    /// <summary>
    /// Selects appropriate node image for the tree.
    /// </summary>
    public class NodeImageSelector : TreeListNodeImageSelector
    {
        /// <summary>
        /// Selects appropriate node image for the tree.
        /// </summary>
        public override ImageSource Select(TreeListRowData rowData)
        {
            var rowElement = rowData.Row as IDiagramElementTreeRowViewModel;

            if (rowElement is null)
            {
                return null;
            }

            var converter = new ThingToIconUriConverter();

            if( rowElement.Thing == null)
            {
                return DXImageHelper.GetImageSource("Images/Conditional Formatting/GreaterThan_16x16.png");
            }

            switch (rowElement.Thing)
            {
                case DiagramPort:
                    return DXImageHelper.GetImageSource("Images/Arrows/Play_16x16.png");
                case DiagramObject:
                    return DXImageHelper.GetImageSource("Images/Grid/FitToContent_16x16.png");
                case DiagramEdge:
                    return DXImageHelper.GetImageSource("Images/Toolbox Items/LineItem_16x16.png");
                case DiagramFrame:
                    return DXImageHelper.GetImageSource("Images/Chart/BottomRightHorizontalInside_16x16.png");
                case ElementDefinition:
                case ElementUsage:
                case DomainOfExpertise:
                case Requirement:
                case Category:
                case ActualFiniteStateList:
                case Parameter:
                default:
                    return converter.Convert(new object[] { rowElement.Thing }, null, null, null) as BitmapImage;
            }
        }
    }
}
