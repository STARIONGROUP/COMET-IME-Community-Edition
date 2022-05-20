﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionTreeListNodeSelector.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Selectors
{
    using System.Globalization;
    using System.Windows.Media;

    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4EngineeringModel.Converters;
    using CDP4EngineeringModel.ViewModels;

    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    /// <summary>
    /// ElementDefinitionTreeListNodeSelector used to select the tree nodes and adds the icons to it
    /// </summary>
    public class ElementDefinitionTreeListNodeSelector : TreeListNodeImageSelector
    {
        /// <summary>
        /// Select node and adds icon to it
        /// </summary>
        /// <param name="rowData"><see cref="TreeListRowData"/></param>
        /// <returns>ImageSource</returns>
        public override ImageSource Select(TreeListRowData rowData)
        {
            var thingStatus = ((IHaveThingStatus)rowData.Row).ThingStatus;

            object classKindOverride = null;

            if (rowData.Row is ParameterOptionRowViewModel optionRowViewModel)
            {
                classKindOverride = ClassKind.Option;
            }

            if (rowData.Row is ParameterStateRowViewModel stateRowViewModel)
            {
                classKindOverride = ClassKind.ActualFiniteState;
            }

            if (rowData.Row is ParameterComponentValueRowViewModel componentRowViewModel)
            {
                classKindOverride = ClassKind.ParameterTypeComponent;
            }

            var elementDefinitionBrowserIconConverter = new ElementDefinitionBrowserIconConverter();
            var image = elementDefinitionBrowserIconConverter.Convert(new object[] { thingStatus }, null, classKindOverride, CultureInfo.InvariantCulture);

            return image as ImageSource;
        }
    }
}
