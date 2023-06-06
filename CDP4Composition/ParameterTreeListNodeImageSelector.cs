// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTreeListNodeImageSelector.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using CDP4Common.CommonData;
    using CDP4Common.Helpers;

    using CDP4Composition.Services;
    using CDP4Composition.ViewModels;

    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// ParameterTreeListNodeImageSelector used to select the tree nodes and adds the icons to it
    /// </summary>
    public class ParameterTreeListNodeImageSelector : TreeListNodeImageSelector
    {
        /// <summary>
        /// The <see cref="IIconCacheService" />
        /// </summary>
        private IIconCacheService iconCacheService;

        /// <summary>
        /// Select node and adds icon to it
        /// </summary>
        /// <param name="rowData">
        ///     <see cref="TreeListRowData" />
        /// </param>
        /// <returns>ImageSource</returns>
        public override ImageSource Select(TreeListRowData rowData)
        {
            var parameteRow = (ParameterRowControlViewModel)rowData.Row;
            var image = this.Convert(parameteRow);
            return image as ImageSource;
        }

        /// <summary>
        /// Returns an GetImage (icon) based on the <see cref="Thing" /> that is provided
        /// </summary>
        /// <param name="parameterRow">The <see cref="ParameterRowControlViewModel" /> object</param>
        /// <returns></returns>
        private object Convert(ParameterRowControlViewModel parameterRow)
        {
            ClassKind valuesetRowType;
            Enum.TryParse(parameterRow.RowType, out valuesetRowType);

            if (parameterRow.Parameter != null && parameterRow.Parameter.StateDependence != null)
            {
                var stateUri = new Uri(IconUtilities.ImageUri(ClassKind.ActualFiniteState).ToString());
                var baseUri = new Uri(IconUtilities.ImageUri(parameterRow.Parameter.ClassKind).ToString());
                return IconUtilities.WithOverlay(baseUri, stateUri);
            }

            if (parameterRow.Parameter != null)
            {
                return this.QueryIIconCacheService().QueryBitmapImage(new Uri(IconUtilities.ImageUri(parameterRow.Parameter.ClassKind).ToString()));
            }

            var uri = new Uri(IconUtilities.ImageUri(valuesetRowType).ToString());

            return new BitmapImage(uri);
        }

        /// <summary>
        /// Queries the instance of the <see cref="IIconCacheService" /> that is to be used
        /// </summary>
        /// <returns>
        /// An instance of <see cref="IIconCacheService" />
        /// </returns>
        private IIconCacheService QueryIIconCacheService()
        {
            return this.iconCacheService ?? (this.iconCacheService = ServiceLocator.Current.GetInstance<IIconCacheService>());
        }
    }
}
