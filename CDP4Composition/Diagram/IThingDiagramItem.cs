// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IThingDiagramItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
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


namespace CDP4Composition.Diagram
{
    using System;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Dal.Operations;

    /// <summary>
    /// Represents an interface to <see cref="DiagramItem"/> controls that also hold a <see cref="Thing"/>.
    /// </summary>
    public interface IThingDiagramItem : IDisposable
    {
        /// <summary>
        /// Gets or sets the <see cref="Thing"/>.
        /// </summary>
        Thing Thing { get; set; }

        /// <summary>
        /// Gets the value indicating whether the item is dirty
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// The diagram thing
        /// </summary>
        DiagramElementThing DiagramThing { get; set; }

        /// <summary>
        /// Update the transaction with the data contained in this view-model
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="container">The container</param>
        void UpdateTransaction(IThingTransaction transaction, DiagramElementContainer container);
    }
}
