// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlRowViewModel.cs" company="RHEA System S.A.">
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
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represent the row of the <see cref="SiteReferenceDataLibrary"/> Grid
    /// </summary>
    public class SiteRdlRowViewModel : SiteReferenceDataLibraryRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CanClose"/>
        /// </summary>
        private bool canClose;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlRowViewModel"/> class
        /// </summary>
        /// <param name="siteRdl">The associated <see cref="SiteReferenceDataLibrary"/></param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public SiteRdlRowViewModel(SiteReferenceDataLibrary siteRdl, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(siteRdl, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="SiteReferenceDataLibrary"/> can be closed
        /// </summary>
        public bool CanClose
        {
            get => this.canClose;
            private set => this.RaiseAndSetIfChanged(ref this.canClose, value);
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateCanClose();

            if (this.RequiredRdl != null)
            {
                this.RequiredRdlName = this.RequiredRdl.Name;
                this.RequiredRdlShortName = this.RequiredRdl.ShortName;
            }
        }

        /// <summary>
        /// Update the <see cref="CanClose"/> value
        /// </summary>
        private void UpdateCanClose()
        {
            // Cannot close a SiteRdl that is required by a ModelRdl
            var mRdls = this.Session.OpenReferenceDataLibraries.OfType<ModelReferenceDataLibrary>().ToList();
            if (mRdls.Any(modelReferenceDataLibrary => modelReferenceDataLibrary.GetRequiredRdls().Contains(this.Thing)))
            {
                this.CanClose = false;
                return;
            }

            this.CanClose = true;
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
    }
}