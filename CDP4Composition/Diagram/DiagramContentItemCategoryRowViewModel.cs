// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramContentItemCategoryRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    /// <summary>
    /// Row class representing a <see cref="Category"/>
    /// </summary>
    public class DiagramContentItemCategoryRowViewModel : CDP4CommonView.CategoryRowViewModel, IDiagramContentItemChild
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramContentItemCategoryRowViewModel"/> class
        /// </summary>
        /// <param name="category">The <see cref="Category"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public DiagramContentItemCategoryRowViewModel(Category category, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(category, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.DiagramContentItemChildString = this.Thing.Name;
        }

        /// <summary>
        /// Updates the <see cref="DiagramContentItemChildString"/> property
        /// </summary>
        public string DiagramContentItemChildString { get; set; }
    }
}
