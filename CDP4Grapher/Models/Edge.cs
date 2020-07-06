// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Edge.cs" company="RHEA System S.A.">
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

namespace CDP4Grapher.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Representation of a graph edge.
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Edge"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        public Edge(string source, string target)
        {
            this.Attributes = new Dictionary<string, string>();
            this.Source = source;
            this.Target = target;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the source, i.e. the id of the source <see cref="Node"/>.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the target, i.e. the id of the target <see cref="Node"/>.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Gets the attributes. The dictionary holds attribute name and value pairs.
        /// </summary>
        public Dictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// Add an attribute to this <see cref="Node"/>.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        public void AddAttribute(string name, string value)
        {
            this.Attributes[name] = value;
        }
    }
}
