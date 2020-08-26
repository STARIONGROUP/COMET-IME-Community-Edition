// -------------------------------------------------------------------------------------------------
// <copyright file="CitationDialogViewModel.cs" company="RHEA S.A.">
//   Copyright (c) 2015-2020 RHEA S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Kamil Wojnowski, Nathanael Smiechowski
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

namespace CDP4CommonView.ViewModels
{
    using CDP4Common.SiteDirectoryData;
    using System.Linq;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="CitationDialogViewModel"/> is to allow a <see cref="Citation"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Citation"/> will result in an <see cref="Citation"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Citation)]
    public class CitationDialogViewModel : CDP4CommonView.CitationDialogViewModel, IThingDialogViewModel
    {

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="CitationDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public CitationDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CitationDialogViewModel"/> class.
        /// </summary>
        /// <param name="citation">
        /// The <see cref="Alias"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="CitationDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="CitationDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public CitationDialogViewModel(Citation citation, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(citation, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        #endregion

        /// <summary>
        /// Updates the property indicating whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        /// <remarks>
        /// The <see cref="Container"/> may not be null and there may not be any Validation Errors
        /// </remarks>
        protected override void UpdateOkCanExecute()
        {
           this.OkCanExecute = this.Container != null && this.SelectedSource != null && !this.ValidationErrors.Any();
        }

        /// <summary>
        /// Populates the <see cref="PossibleSource"/> property
        /// </summary>
        protected override void PopulatePossibleSource()
        {
            this.PossibleSource.Clear();
            IEnumerable<ReferenceSource> referenceSources = null;
            var rdlsInChain = this.ChainOfContainer.Where(x => x is ReferenceDataLibrary).ToList();
            if (rdlsInChain.Any())
            {
                referenceSources = rdlsInChain.SelectMany(x => ((ReferenceDataLibrary)x).ReferenceSource).OrderBy(x => x.Name);
            }

            if (referenceSources != null)
            {
                this.PossibleSource.AddRange(referenceSources);
            }
        }
    }
}
