// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterUsageKind.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The different kinds of a parameter usage 
    /// </summary>
    public enum ParameterUsageKind
    {
        /// <summary>
        /// The <see cref="ParameterOrOverrideBase"/> is neither used or owned by the active <see cref="DomainOfExpertise"/>
        /// </summary>
        Unused = 0,

        /// <summary>
        /// The <see cref="ParameterOrOverrideBase"/> has subscription from other <see cref="DomainOfExpertise"/> than the active one
        /// </summary>
        SubscribedByOthers = 1,

        /// <summary>
        /// The active <see cref="DomainOfExpertise"/> has subscribed to the <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        Subscribed = 2
    }
}