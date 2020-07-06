// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphElementTemplateSelector.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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


namespace CDP4Grapher.ViewModels
{
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The <see cref="DataTemplateSelector"/> to select a <see cref="DataTemplate"/> depending on the kind of budget to compute
    /// </summary>
    public class GraphElementDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Selects the template for a <see cref="Thing"/>
        /// </summary>
        /// <param name="item">The <see cref="Thing"/></param>
        /// <param name="container">the dependency-object</param>
        /// <returns>The <see cref="DataTemplate"/> to use</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                ElementDefinition _ => this.TopElementDataTemplate,
                NestedElement _ => this.NestedElementDataTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for the <see cref="Iteration.TopElement"/> template
        /// </summary>
        public DataTemplate TopElementDataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for an <see cref="NestedElement"/> template
        /// </summary>
        public DataTemplate NestedElementDataTemplate { get; set; }
    }
}
