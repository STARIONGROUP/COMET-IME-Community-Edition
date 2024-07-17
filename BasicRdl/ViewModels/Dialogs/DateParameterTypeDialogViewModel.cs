// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateParameterTypeDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of the <see cref="DateParameterTypeDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="DateParameterType"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.DateParameterType)]
    public class DateParameterTypeDialogViewModel : CDP4CommonView.DateParameterTypeDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateParameterTypeDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public DateParameterTypeDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateParameterTypeDialogViewModel"/> class
        /// </summary>
        /// <param name="dateParameterType">
        /// The <see cref="DateParameterType"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="thingTransaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="thingDialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public DateParameterTypeDialogViewModel(DateParameterType dateParameterType, IThingTransaction thingTransaction, ISession session, bool isRoot, ThingDialogKind thingDialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(dateParameterType, thingTransaction, session, isRoot, thingDialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
    }
}