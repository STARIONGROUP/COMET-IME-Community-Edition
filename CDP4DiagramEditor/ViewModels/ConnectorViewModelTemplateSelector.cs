// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectorViewModelTemplateSelector.cs" company="RHEA System S.A.">
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

namespace CDP4DiagramEditor.ViewModels
{
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Diagram;

    /// <summary>
    /// The <see cref="DataTemplateSelector"/> to select a <see cref="DataTemplate"/> depending on the kind of <see cref="IDiagramConnectorViewModel"/>
    /// </summary>
    public class ConnectorViewModelTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a normal connector
        /// </summary>
        public DataTemplate GenericConnectorTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a DiagramContentItem that represents an <see cref="ElementUsage"/>
        /// </summary>
        public DataTemplate ElementUsageConnectorDataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a DiagramContentItem that represents an <see cref="ElementUsage"/>
        /// </summary>
        public DataTemplate InterfaceConnectorDataTemplate { get; set; }

        /// <summary>
        /// Selects the template for a <see cref="NamedThingDiagramContentItemViewModel"/>
        /// </summary>
        /// <param name="item">The <see cref="NamedThingDiagramContentItemViewModel"/></param>
        /// <param name="container">the dependency-object</param>
        /// <returns>The <see cref="DataTemplate"/> to use</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                ElementUsageEdgeViewModel => this.ElementUsageConnectorDataTemplate,
                InterfaceEdgeViewModel => this.InterfaceConnectorDataTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}
