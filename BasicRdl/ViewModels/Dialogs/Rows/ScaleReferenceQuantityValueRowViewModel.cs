// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScaleReferenceQuantityValueRowViewModel.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    /// <summary>
    /// The row view model that represents a <see cref="ScaleReferenceQuantityValue"/>
    /// </summary>
    public class ScaleReferenceQuantityValueRowViewModel : CDP4CommonView.ScaleReferenceQuantityValueRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleReferenceQuantityValueRowViewModel"/> class
        /// </summary>
        /// <param name="quantityValue">The <see cref="ScaleReferenceQuantityValue"/> represented</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The viewmodel that contains this row</param>
        public ScaleReferenceQuantityValueRowViewModel(ScaleReferenceQuantityValue quantityValue, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(quantityValue, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Scale != null)
            {
                this.ScaleName = this.Scale.Name;
                this.ScaleShortName = this.Scale.ShortName;
            }
        }
    }
}