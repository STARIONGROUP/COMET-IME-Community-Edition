// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonPermissionRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    This is an auto-generated class. Any manual changes to this file will be overwritten!
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using CDP4Common.CommonData;

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
