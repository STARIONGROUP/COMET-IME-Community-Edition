// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteReferenceDataLibraryDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4SiteDirectory.Views;

    /// <summary>
    /// The corresponding view-model for the <see cref="SiteReferenceDataLibraryDialog"/> view used to create, edit or inspect a <see cref="SiteReferenceDataLibrary"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.SiteReferenceDataLibrary)]
    public class SiteReferenceDataLibraryDialogViewModel : CDP4CommonView.SiteReferenceDataLibraryDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteReferenceDataLibraryDialogViewModel"/> class.
        /// </summary>
        public SiteReferenceDataLibraryDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteReferenceDataLibraryDialogViewModel"/> class
        /// </summary>
        /// <param name="siteRdl">The <see cref="SiteReferenceDataLibrary"/> represented</param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DomainOfExpertiseDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DomainOfExpertiseDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="container">The container <see cref="Thing"/> for the created <see cref="Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public SiteReferenceDataLibraryDialogViewModel(SiteReferenceDataLibrary siteRdl, ThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(siteRdl, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Populate the possible required rdl
        /// </summary>
        protected override void PopulatePossibleRequiredRdl()
        {
            if (this.dialogKind != ThingDialogKind.Create && this.dialogKind != ThingDialogKind.Update)
            {
                return;
            }

            base.PopulatePossibleRequiredRdl();
            var sitedir = (SiteDirectory) this.Container;
            var possibleRdl =
                sitedir.SiteReferenceDataLibrary.Where(x => x.Iid != this.Thing.Iid && x.GetRequiredRdls().All(y => y.Iid != this.Thing.Iid));

            this.PossibleRequiredRdl.AddRange(possibleRdl);
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            logger.Trace("UpdateProperties called.");
            this.UpdateOkCanExecute();
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.PopulateAlias();
            this.PopulateDefinition();
            this.PopulateHyperLink();
            this.IsDeprecated = this.Thing.IsDeprecated;
            this.SelectedRequiredRdl = this.Thing.RequiredRdl;
            this.PopulatePossibleRequiredRdl();
        }
    }
}