// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRelationshipCreatorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The interface for the view-model used to set the properties of a <see cref="Relationship"/>
    /// </summary>
    public interface IRelationshipCreatorViewModel : IDisposable, IISession
    {
        /// <summary>
        /// Gets the name of the <see cref="Relationship"/> to create
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the type of the <see cref="Relationship"/> to create
        /// </summary>
        string CreatorKind { get; }

        /// <summary>
        /// Gets the possible <see cref="Category"/>
        /// </summary>
        ReactiveList<Category> PossibleCategories { get; }

        /// <summary>
        /// Gets the <see cref="Category"/> applied to the <see cref="Relationship"/> to create
        /// </summary>
        List<Category> AppliedCategories { get; }

        /// <summary>
        /// Gets a value indicating whether a <see cref="Relationship"/> may be created
        /// </summary>
        bool CanCreate { get; }

        /// <summary>
        /// Re-Initializes the view-model
        /// </summary>
        void ReInitializeControl();

        /// <summary>
        /// Creates a <see cref="Relationship"/> 
        /// </summary>
        /// <returns>A new instance of <see cref="Relationship"/></returns>
        Relationship CreateRelationshipObject();
    }
}