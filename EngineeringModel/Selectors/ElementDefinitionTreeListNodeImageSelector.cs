// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionTreeListNodeImageSelector.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
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

namespace CDP4EngineeringModel.Selectors
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;

    using CDP4EngineeringModel.ViewModels;

    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    /// <summary>
    /// ElementDefinitionTreeListNodeImageSelector used to select the tree nodes and adds the icons to it
    /// </summary>
    public class ElementDefinitionTreeListNodeImageSelector : TreeListNodeImageSelector
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

            var image = this.Convert(new object[] { thingStatus }, null, classKindOverride, CultureInfo.InvariantCulture);

            return image as ImageSource;
        }

        /// <summary>
        /// Returns an GetImage (icon) based on the <see cref="Thing" /> that is provided
        /// </summary>
        /// <param name="value">An instance of <see cref="Thing" /> for which an Icon needs to be returned</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The <see cref="ClassKind" /> of the overlay to use</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>
        /// A <see cref="Uri" /> to an GetImage
        /// </returns>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var genericConverter = new ThingToIconUriConverter();
                var thingStatus = value.SingleOrDefault() as ThingStatus;

                var parameterBase = thingStatus?.Thing as ParameterBase;

                ClassKind valuesetRowType;

                if (parameterBase == null || parameter == null || !Enum.TryParse(parameter.ToString(), out valuesetRowType))
                {
                    return genericConverter.Convert(value, targetType, parameter, culture);
                }

                var isCompound = parameterBase.ParameterType is CompoundParameterType;

                // Value set row
                // row representing an option
                if (valuesetRowType == ClassKind.Option)
                {
                    var optionUri = new Uri(IconUtilities.ImageUri(valuesetRowType).ToString());

                    if (parameterBase.StateDependence != null || isCompound)
                    {
                        return new BitmapImage(optionUri);
                    }

                    var uri = new Uri(IconUtilities.ImageUri(parameterBase.ClassKind).ToString());
                    return IconUtilities.WithOverlay(uri, optionUri);
                }

                // row representing a component
                if (valuesetRowType == ClassKind.ParameterTypeComponent)
                {
                    var componentUri = new Uri(IconUtilities.ImageUri(valuesetRowType).ToString());
                    return new BitmapImage(componentUri);
                }

                // Row representing state
                var stateUri = new Uri(IconUtilities.ImageUri(ClassKind.ActualFiniteState).ToString());

                if (isCompound)
                {
                    return new BitmapImage(stateUri);
                }

                var baseUri = new Uri(IconUtilities.ImageUri(parameterBase.ClassKind).ToString());
                return IconUtilities.WithOverlay(baseUri, stateUri);
            }
            catch (Exception ex)
            {
                // Do nothing, just return null for this edge case. Otherwise the app will crash.
            }

            return null;
        }
    }
}
