// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupSelectionViewModel.cs" company="Starion Group S.A.">
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// This class is used as the row view-model for group selection in a combobox
    /// </summary>
    public class GroupSelectionViewModel : ReactiveObject
    { 
        /// <summary>
        /// Backing field for <see cref="DisplayedName"/>
        /// </summary>
        private string displayedName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupSelectionViewModel"/> class
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup"/></param>
        public GroupSelectionViewModel(ParameterGroup group)
        {
            this.ContainedGroups = new ReactiveList<GroupSelectionViewModel>();
            this.Thing = group;
            this.Parent = this.Thing.ContainingGroup;
            this.DisplayedName = this.Thing.Name;
        }

        /// <summary>
        /// Gets the contained group view-model
        /// </summary>
        public ReactiveList<GroupSelectionViewModel> ContainedGroups { get; private set; }

        /// <summary>
        /// Gets the represented <see cref="ParameterGroup"/>
        /// </summary>
        public ParameterGroup Thing { get; private set; }

        /// <summary>
        /// Gets the containing <see cref="ParameterGroup"/>
        /// </summary>
        public ParameterGroup Parent { get; private set; }

        /// <summary>
        /// Gets the displayed name
        /// </summary>
        public string DisplayedName
        {
            get { return this.displayedName; }
            private set { this.RaiseAndSetIfChanged(ref this.displayedName, value);}
        }
    }
}