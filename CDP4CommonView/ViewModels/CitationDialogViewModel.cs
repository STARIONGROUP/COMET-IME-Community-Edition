// -------------------------------------------------------------------------------------------------
// <copyright file="CitationDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.ViewModels
{
    using CDP4Common.SiteDirectoryData;
    using System.Linq;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
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
                referenceSources = rdlsInChain.SelectMany(x => ((ReferenceDataLibrary)x).ReferenceSource);
            }

            this.PossibleSource.AddRange(referenceSources);
        }

    }
}
