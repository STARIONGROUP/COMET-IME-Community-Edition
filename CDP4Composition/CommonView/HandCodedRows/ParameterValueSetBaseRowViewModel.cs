// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueSetBaseRowViewModel.cs" company="RHEA System S.A.">
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
    using CDP4Common.EngineeringModelData;

    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="ParameterValueSetBaseRowViewModel{T}"/>
    /// </summary>
    public partial class ParameterValueSetBaseRowViewModel<T>
    {
        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private string manual;

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private string computed;

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private string reference;

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private string value;

        /// <summary>
        /// Gets the Value
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
        }

        /// <summary>
        /// computed parameter value
        /// Note: This is value of the associated Parameter as computed by the parameter's owner <see cref="DomainOfExpertise"/>
        /// </summary>
        public string Computed
        {
            get { return this.computed; }
            set { this.RaiseAndSetIfChanged(ref this.computed, value); }
        }

        /// <summary>
        /// manually assigned parameter value
        /// Note: The <i>manual</i> value is typically used in the beginning of the modelling process, when computed and published values are not yet available, in order to enable starting computations with <see cref="ParameterSubscription"/>s.
        /// </summary>
        public string Manual
        {
            get { return this.manual; }
            set { this.RaiseAndSetIfChanged(ref this.manual, value); }
        }


        /// <summary>
        /// reference parameter value
        /// Note: The reference value is typically a value originating from outside the current <see cref="EngineeringModel"/> to be used as a reference to be compared with the (newly) computed value. However the reference values may be used for any purpose that is deemed useful by the users of the <see cref="EngineeringModel"/>.
        /// </summary>
        public string Reference
        {
            get { return this.reference; }
            set { this.RaiseAndSetIfChanged(ref this.reference, value); }
        }
    }
}
