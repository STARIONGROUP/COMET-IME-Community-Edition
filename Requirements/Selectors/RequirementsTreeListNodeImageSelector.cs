// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsTreeListNodeImageSelector.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Selectors
{
    using System.Globalization;
    using System.Windows.Media;

    using CDP4Common.CommonData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;

    using CDP4Requirements.ViewModels;
    using CDP4Requirements.ViewModels.RequirementBrowser.Rows;

    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    /// <summary>
    /// RequirementsTreeListNodeImageSelector used to select the tree nodes and adds the icons to it
    /// </summary>
    public class RequirementsTreeListNodeImageSelector : TreeListNodeImageSelector
    {
        /// <summary>
        /// Select node and adds icon to it
        /// </summary>
        /// <param name="rowData"><see cref="TreeListRowData"/></param>
        /// <returns>ImageSource</returns>
        public override ImageSource Select(TreeListRowData rowData)
        {
            var thingStatus = ((IHaveThingStatus)rowData.Row).ThingStatus;
            Thing thing = null;

            if (rowData.Row is ParametricConstraintsFolderRowViewModel parametricConstraintsFolderRow)
            {
                thing = parametricConstraintsFolderRow.Thing;
            }
            else if (rowData.Row is ParametricConstraintRowViewModel parametricConstraintRow)
            {
                thing = parametricConstraintRow.Thing;
            }

            var converter = new ThingToIconUriConverter();

            var image = converter.Convert(new object[] { thing, thingStatus }, null, null, CultureInfo.InvariantCulture);

            return image as ImageSource;
        }
    }
}
