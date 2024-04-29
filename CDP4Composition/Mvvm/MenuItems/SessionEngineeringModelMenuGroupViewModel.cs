﻿// -------------------------------------------------------------------------------------------------
// <copyright file="SessionEngineeringModelSetupMenuGroupViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2017-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Jame Bernar
//
//    This file is part of CDP4-IME Community Edition. 
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    
    using MenuItems;
    
    using ReactiveUI;

    /// <summary>
    /// The session dependent menu group that contains model dependent menu items
    /// </summary>
    public class SessionEngineeringModelMenuGroupViewModel : MenuGroupViewModelBase<EngineeringModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionEngineeringModelMenuGroupViewModel"/> class
        /// </summary>
        /// <param name="engineeringModel">
        /// The <see cref="EngineeringModel"/> to add
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public SessionEngineeringModelMenuGroupViewModel(EngineeringModel engineeringModel, ISession session)
            : base(engineeringModel, session)
        {
            this.EngineeringModels = new ReactiveList<RibbonMenuItemEngineeringModelDependentViewModel>();
        }

        /// <summary>
        /// Derives the name string based on containment
        /// </summary>
        /// <returns>The formatted name of the group.</returns>
        protected override string DeriveName()
        {
            return this.Session.Name;
        }

        /// <summary>
        /// Gets the list of <see cref="RibbonMenuItemEngineeringModelDependentViewModel"/> based on the <see cref="EngineeringModel"/>s available
        /// </summary>
        public ReactiveList<RibbonMenuItemEngineeringModelDependentViewModel> EngineeringModels { get; private set; }
    }
}