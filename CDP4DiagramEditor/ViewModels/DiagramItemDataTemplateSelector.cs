﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramItemDataTemplateSelector.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4DiagramEditor.ViewModels
{
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Diagram;

    /// <summary>
    /// The <see cref="DataTemplateSelector"/> to select a <see cref="DataTemplate"/> depending on the kind of budget to compute
    /// </summary>
    public class DiagramItemDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a normal DiagramContentItem
        /// </summary>
        public DataTemplate GenericDiagramItemDataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a DiagramPort
        /// </summary>
        public DataTemplate DiagramPortTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a DiagramCOntentItem that represents an <see cref="ElementBase"/>
        /// </summary>
        public DataTemplate ElementBaseDiagramItemTemplate { get; set; }

        /// <summary>
        /// Selects the template for a <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        /// <param name="item">The <see cref="NamedThingDiagramContentItem"/></param>
        /// <param name="container">the dependency-object</param>
        /// <returns>The <see cref="DataTemplate"/> to use</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                ElementDefinitionDiagramContentItem => this.ElementBaseDiagramItemTemplate,
                NamedThingDiagramContentItem => this.GenericDiagramItemDataTemplate,
                DiagramPortViewModel => this.DiagramPortTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
