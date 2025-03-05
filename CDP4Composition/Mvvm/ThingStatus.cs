// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingStatus.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Mvvm
{
    using System.Linq;

    using CDP4Common.CommonData;

    using ReactiveUI;

    /// <summary>
    /// A class that gives information on the status of a <see cref="Thing"/>
    /// </summary>
    public class ThingStatus : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsLocked"/>
        /// </summary>
        private bool isLocked = false;

        /// <summary>
        /// Backing field for <see cref="IsHidden"/>
        /// </summary>
        private bool isHidden = false;

        /// <summary>
        /// Backing field for <see cref="IsFavorite"/>
        /// </summary>
        private bool isFavorite = false;

        /// <summary>
        /// Initializes a new instace of the <see cref="ThingStatus"/> class
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        private ThingStatus(Thing thing)
        {
            this.Thing = thing;
            this.HasError = thing.ValidationErrors.Any();
            this.HasRelationship = thing.HasRelationship;
        }

        /// <summary>
        /// Updates the status of the <see cref="ThingStatus"/>
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        public static ThingStatus CreateNewThingStatus(Thing thing)
        {
            return new ThingStatus(thing);
        }

        /// <summary>
        /// Updates the status of the <see cref="ThingStatus"/>
        /// </summary>
        /// <param name="viewModel">The viewmodel where the ThingStatus should be present on</param>
        /// <param name="thing">The <see cref="Thing"/></param>
        public static void SetOrUpdateThingStatus(IHaveThingStatus viewModel, Thing thing)
        {
            if (viewModel.ThingStatus == null)
            {
                viewModel.ThingStatus = CreateNewThingStatus(thing);
            }
            else
            {
                viewModel.ThingStatus.Thing = thing;
                viewModel.ThingStatus.HasError = thing.ValidationErrors.Any();
                viewModel.ThingStatus.HasRelationship = thing.HasRelationship;
            }
        }

        /// <summary>
        /// Gets the <see cref="Thing"/>
        /// </summary>
        public Thing Thing { get; private set; }

        /// <summary>
        /// Asserts whether the <see cref="Thing"/> has errors
        /// </summary>
        public bool HasError { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the thing has associated relationships
        /// </summary>
        public bool HasRelationship { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the thing is marked as a user's favorite
        /// </summary>
        public bool IsFavorite
        {
            get => this.isFavorite;
            set => this.RaiseAndSetIfChanged(ref this.isFavorite, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the thing is marked as locked
        /// </summary>
        public bool IsLocked
        {
            get => this.isLocked;
            set => this.RaiseAndSetIfChanged(ref this.isLocked, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the thing is marked as hidden
        /// </summary>
        public bool IsHidden
        {
            get => this.isHidden;
            set => this.RaiseAndSetIfChanged(ref this.isHidden, value);
        }
    }
}
