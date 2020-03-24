// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtendedRibbonPageCategory.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
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

namespace CDP4Composition.Ribbon
{
    using System.Windows;

    using CDP4Composition.Adapters;

    using DevExpress.Xpf.Ribbon;

    /// <summary>
    /// Extension of the <see cref="RibbonPageCategory"/> that is used by the <see cref="RibbonAdapter"/> to manage the region behavior
    /// </summary>
    public class ExtendedRibbonPageCategory : RibbonPageCategory
    {
        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="ContainerRegionName"/> property
        /// </summary>
        /// <remarks>
        /// The default value of this <see cref="DependencyProperty"/> is <see cref="string.Empty"/>
        /// </remarks>
        public static readonly DependencyProperty ContainerRegionNameProperty = DependencyProperty.Register(
            "ContainerRegionName",
            typeof(string),
            typeof(ExtendedRibbonPageCategory),
            new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the name of the container <see cref="RibbonPage"/> that the current <see cref="ExtendedRibbonPageCategory"/> needs to be added to.
        /// </summary>
        /// <remarks>
        /// The page will only be added by the <see cref="RibbonAdapter"/> if the <see cref="RibbonPage"/> with the specified <see cref="RegionName"/> 
        /// property can be found in the <see cref="RibbonControl"/>
        /// </remarks>
        public string ContainerRegionName
        {
            get => (string)this.GetValue(ContainerRegionNameProperty);
            set => this.SetValue(ContainerRegionNameProperty, value);
        }
    }
}
