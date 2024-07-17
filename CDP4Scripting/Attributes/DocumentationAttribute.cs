// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationAttribute.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4Scripting.Attributes
{
    using System;

    using CDP4Composition.Attributes;

    using CDP4Scripting.Helpers;

    /// <summary>
    /// The purporse of this class is to decorate the commands possible in the scripting engine. 
    /// The command <see cref="ScriptingProxy.Help"/> browses all the commands decorated and displays them to the output.
    /// </summary>
    public class DocumentationAttribute : Attribute, IDescribeMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of a command.</param>
        /// <param name="description">The description of a command.</param>
        public DocumentationAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        public string Description { get; private set; }
    }
}
