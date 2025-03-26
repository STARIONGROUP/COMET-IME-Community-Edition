// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementNotThingContainerRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate
//
//    This file is part of CDP4-COMET-IME Community Edition. 
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementBrowser.Rows
{
    using CDP4Common.CommonData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    /// <summary>
    /// The <see cref="RequirementNotThingContainerRowViewModel"/> row view model.
    /// </summary>
    public class RequirementNotThingContainerRowViewModel : FolderRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementNotThingContainerRowViewModel"/> class
        /// </summary>
        /// <param name="shortname">The short-name for this folder</param>
        /// <param name="name">The Name of the folder</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The view-model that contains this row</param>
        public RequirementNotThingContainerRowViewModel(string shortname, string name, ISession session, IViewModelBase<Thing> containerViewModel) : base(shortname, name, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets a value indicating whether the value set editors are active
        /// </summary>
        public static bool IsValueSetEditorActive => false;
    }
}
