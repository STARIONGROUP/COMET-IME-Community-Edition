// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BulkParticipantRowViewModel.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    /// <summary>
    /// Represents a single candidate <see cref="Participant"/> to be created in the
    /// <see cref="BulkParticipantCreationDialogViewModel"/>. Each row maps one <see cref="Person"/> to the
    /// <see cref="DomainOfExpertise"/> the created <see cref="Participant"/> shall receive. The
    /// <see cref="ParticipantRole"/> and active state are applied for the whole batch by the owning
    /// <see cref="BulkParticipantCreationDialogViewModel"/>.
    /// </summary>
    public class BulkParticipantRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Backing field for <see cref="SelectedDomain"/>
        /// </summary>
        private DomainOfExpertise selectedDomain;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkParticipantRowViewModel"/> class.
        /// </summary>
        /// <param name="person">
        /// The <see cref="Person"/> for which a <see cref="Participant"/> may be created.
        /// </param>
        /// <param name="possibleDomains">
        /// The possible <see cref="DomainOfExpertise"/>s (the active domains of the model) that may be assigned.
        /// </param>
        public BulkParticipantRowViewModel(Person person, IEnumerable<DomainOfExpertise> possibleDomains)
        {
            this.Person = person;
            this.PossibleDomain = possibleDomains.ToList();
            this.IsSelected = true;

            if (person.DefaultDomain != null && this.PossibleDomain.Contains(person.DefaultDomain))
            {
                this.SelectedDomain = person.DefaultDomain;
            }
        }

        /// <summary>
        /// Gets the <see cref="Person"/> represented by this row.
        /// </summary>
        public Person Person { get; }

        /// <summary>
        /// Gets the name of the <see cref="Person"/> represented by this row.
        /// </summary>
        public string PersonName => this.Person.Name;

        /// <summary>
        /// Gets the possible <see cref="DomainOfExpertise"/>s that may be assigned to this <see cref="Person"/>.
        /// </summary>
        public IReadOnlyList<DomainOfExpertise> PossibleDomain { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="Participant"/> shall be created for this <see cref="Person"/>.
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise"/> that the created <see cref="Participant"/> shall receive.
        /// </summary>
        public DomainOfExpertise SelectedDomain
        {
            get => this.selectedDomain;
            set => this.RaiseAndSetIfChanged(ref this.selectedDomain, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ParticipantRole"/> that the created <see cref="Participant"/> shall receive.
        /// </summary>
        public ParticipantRole SelectedRole { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the created <see cref="Participant"/> shall be active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
