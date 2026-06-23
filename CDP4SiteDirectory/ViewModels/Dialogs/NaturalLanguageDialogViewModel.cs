// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
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

    using ReactiveUI;

    /// <summary>
    /// The corresponding view-model for the <see cref="DomainOfExpertiseDialog"/> view used to create, edit or inspect a <see cref="DomainOfExpertise"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.NaturalLanguage)]
    public class NaturalLanguageDialogViewModel : CDP4CommonView.NaturalLanguageDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="NativeName"/>
        /// </summary>
        private string nativeName;

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

        /// <summary>
        /// Gets or sets the NativeName of the current <see cref="NaturalLanguage"/>
        /// </summary>
        /// <remarks>
        /// The NativeName is a free-text <see cref="string"/>, not a ShortName, so its validation has been disabled.
        /// </remarks>
        [ValidationOverride(false)]
        public override string NativeName
        {
            get => this.nativeName;
            set => this.RaiseAndSetIfChanged(ref this.nativeName, value);
        }
    }
}
