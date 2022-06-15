// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonPermissionRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="PersonPermissionRowViewModel"/>
    /// </summary>
    public partial class PersonPermissionRowViewModel
    {
        /// <summary>
        /// The possible <see cref="PersonAccessRightKind"/>
        /// </summary>
        private readonly ReactiveList<PersonAccessRightKind> possibleRightKinds = new ReactiveList<PersonAccessRightKind>
        {
            PersonAccessRightKind.NONE,
            PersonAccessRightKind.MODIFY,
            PersonAccessRightKind.MODIFY_IF_PARTICIPANT,
            PersonAccessRightKind.MODIFY_OWN_PERSON,
            PersonAccessRightKind.READ,
            PersonAccessRightKind.READ_IF_PARTICIPANT
        };

        /// <summary>
        /// Gets the Possible right kinds
        /// </summary>
        public ReactiveList<PersonAccessRightKind> PossibleRightKinds
        {
            get { return this.possibleRightKinds; }
        }
    }
}