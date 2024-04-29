// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageDialogViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4SiteDirectory.Views;

    /// <summary>
    /// The corresponding view-model for the <see cref="DomainOfExpertiseDialog"/> view used to create, edit or inspect a <see cref="DomainOfExpertise"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.NaturalLanguage)]
    public class NaturalLanguageDialogViewModel : CDP4CommonView.NaturalLanguageDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalLanguageDialogViewModel"/> class
        /// </summary>
        /// <param name="naturalLanguage">The <see cref="NaturalLanguage"/> represented</param>
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
        public NaturalLanguageDialogViewModel(NaturalLanguage naturalLanguage, ThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(naturalLanguage, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalLanguageDialogViewModel"/> class.
        /// </summary>
        public NaturalLanguageDialogViewModel()
        {
        }
    }
}
