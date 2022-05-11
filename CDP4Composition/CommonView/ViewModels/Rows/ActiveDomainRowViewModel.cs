// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActiveDomainRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.CommonView.ViewModels
{
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    public class ActiveDomainRowViewModel : ReactiveObject
    {
        /// <summary>
        /// The backing field for <see cref="IsActive" />
        /// </summary>
        private bool isActive;

        /// <summary>
        /// The backing field for <see cref="IsDeprecated" />
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// The backing field for <see cref="IsEnabled" />
        /// </summary>
        private bool isEnabled;

        /// <summary>
        /// The backing field for <see cref="IsVisible" />
        /// </summary>
        private bool isVisible;

        /// <summary>
        /// Initialize an instance of ActiveDomainRowViewModel
        /// </summary>
        /// <param name="domainOfExpertise">
        ///     <see cref="DomainOfExpertise" />
        /// </param>
        /// <param name="isVisible">
        ///     <see cref="IsVisible" />
        /// </param>
        public ActiveDomainRowViewModel(DomainOfExpertise domainOfExpertise, bool isVisible)
        {
            this.DomainOfExpertise = domainOfExpertise;
            this.IsVisible = isVisible;
            this.Name = domainOfExpertise.Name;
            this.IsDeprecated = domainOfExpertise.IsDeprecated;
            this.IsEnabled = false;
        }

        /// <summary>
        /// Gets or sets the visibility of a domain
        /// </summary>
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        /// <summary>
        /// Gets or sets if a domain is enabled for selection or not
        /// </summary>
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.RaiseAndSetIfChanged(ref this.isEnabled, value);
        }

        /// <summary>
        /// Gets or sets if the domain is deprecated or not
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Gets or sets the active status of a domain
        /// </summary>
        public bool IsActive
        {
            get => this.isActive;
            set => this.RaiseAndSetIfChanged(ref this.isActive, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise" />
        /// </summary>
        public DomainOfExpertise DomainOfExpertise { get; private set; }

        /// <summary>
        /// Gets or sets the name of <see cref="DomainOfExpertise" />
        /// </summary>
        public string Name { get; set; }
    }
}
