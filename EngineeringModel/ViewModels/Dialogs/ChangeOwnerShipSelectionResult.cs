// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeOwnerShipSelectionResult.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed
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

namespace CDP4EngineeringModel.ViewModels.Dialogs
{
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    /// <summary>
    /// The purpose of the <see cref="ChangeOwnerShipSelectionResult"/> is to present possible <see cref="DomainOfExpertise"/>s and to
    /// to select one. A selection that allows the user to indicate whether the contained items shall have their ownership changed as
    /// well is also present
    /// </summary>
    public class ChangeOwnerShipSelectionResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeOwnerShipSelectionResult"/> class.
        /// </summary>
        /// <param name="result"></param>
        public ChangeOwnerShipSelectionResult(bool? result, DomainOfExpertise domainOfExpertise, bool isContainedItemChangeOwnerShipSelected) : base(result)
        {
            this.DomainOfExpertise = domainOfExpertise;
            this.IsContainedItemChangeOwnerShipSelected = isContainedItemChangeOwnerShipSelected;
        }

        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertise"/>
        /// </summary>
        public DomainOfExpertise DomainOfExpertise { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the contained <see cref="IOwnedThing"/>s shall be have the
        /// owner property updated as well.
        /// </summary>
        public bool IsContainedItemChangeOwnerShipSelected { get; private set; }
    }
}
