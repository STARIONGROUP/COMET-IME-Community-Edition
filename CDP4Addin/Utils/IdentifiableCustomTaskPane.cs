// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdentifiableCustomTaskPane.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
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

namespace CDP4AddinCE
{
    using System;
    using NetOffice.OfficeApi;

    /// <summary>
    /// The purpose of the <see cref="IdentifiableCustomTaskPane"/> is to Decorate the <see cref="_CustomTaskPane"/>
    /// with extra properties
    /// </summary>
    public class IdentifiableCustomTaskPane : IDisposable
    {
        public IdentifiableCustomTaskPane(Guid identifier, _CustomTaskPane customTaskPane)
        {
            this.Identifier = identifier;
            this.CustomTaskPane = customTaskPane;
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="IdentifiableCustomTaskPane"/>
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Gets the <see cref="_CustomTaskPane"/> that is decorated by the <see cref="IdentifiableCustomTaskPane"/>
        /// </summary>
        public _CustomTaskPane CustomTaskPane { get; private set; }

        /// <summary>
        /// Dispose of the decorated <see cref="_CustomTaskPane"/>
        /// </summary>
        public void Dispose()
        {
            this.CustomTaskPane.Dispose();
        }
    }
}
