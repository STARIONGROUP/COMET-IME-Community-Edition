// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BulkParticipantCreationDialogViewModel.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using ReactiveUI;

    /// <summary>
    /// The dialog-view-model that allows the user to create multiple <see cref="Participant"/>s at once for a single
    /// <see cref="EngineeringModelSetup"/>. The <see cref="SelectedRole"/> and active state chosen on the dialog are
    /// applied to every selected <see cref="Person"/> as a default, while the <see cref="DomainOfExpertise"/> and the
    /// <see cref="ParticipantRole"/> may be overridden per <see cref="Person"/> (the domain defaulting to the
    /// <see cref="Person"/>'s own default domain).
    /// </summary>
    public class BulkParticipantCreationDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="SelectedRole"/>
        /// </summary>
        private ParticipantRole selectedRole;

        /// <summary>
        /// Backing field for <see cref="IsActive"/>
        /// </summary>
        private bool isActive;

        /// <summary>
        /// Backing field for <see cref="OkCanExecute"/>
        /// </summary>
        private bool okCanExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkParticipantCreationDialogViewModel"/> class.
        /// </summary>
        /// <param name="persons">
        /// The <see cref="Person"/>s for which a <see cref="Participant"/> may be created.
        /// </param>
        /// <param name="participantRoles">
        /// The possible <see cref="ParticipantRole"/>s that may be assigned.
        /// </param>
        /// <param name="domainsOfExpertise">
        /// The possible <see cref="DomainOfExpertise"/>s (the active domains of the model) that may be assigned.
        /// </param>
        public BulkParticipantCreationDialogViewModel(IEnumerable<Person> persons, IEnumerable<ParticipantRole> participantRoles, IEnumerable<DomainOfExpertise> domainsOfExpertise)
        {
            if (persons == null)
            {
                throw new ArgumentNullException(nameof(persons), $"The {nameof(persons)} may not be null");
            }

            if (participantRoles == null)
            {
                throw new ArgumentNullException(nameof(participantRoles), $"The {nameof(participantRoles)} may not be null");
            }

            if (domainsOfExpertise == null)
            {
                throw new ArgumentNullException(nameof(domainsOfExpertise), $"The {nameof(domainsOfExpertise)} may not be null");
            }

            this.PossibleRole = participantRoles.OrderBy(x => x.Name).ToList();
            this.PossibleDomain = domainsOfExpertise.OrderBy(x => x.Name).ToList();
            this.Participants = new ReactiveList<BulkParticipantRowViewModel>();
            this.IsActive = true;

            foreach (var person in persons.OrderBy(x => x.Name))
            {
                var row = new BulkParticipantRowViewModel(person, this.PossibleDomain, this.PossibleRole);
                this.Subscriptions.Add(row.WhenAnyValue(x => x.IsSelected, x => x.SelectedDomain, x => x.SelectedRole).Subscribe(_ => this.UpdateOkCanExecute()));
                this.Participants.Add(row);
            }

            this.Subscriptions.Add(this.WhenAnyValue(x => x.SelectedRole).Subscribe(this.ApplyRoleToRows));
            this.Subscriptions.Add(this.WhenAnyValue(x => x.IsActive).Subscribe(this.ApplyIsActiveToRows));

            this.InitializeReactiveCommands();
            this.UpdateOkCanExecute();
        }

        /// <summary>
        /// Gets the rows representing the <see cref="Participant"/>s that may be created.
        /// </summary>
        public ReactiveList<BulkParticipantRowViewModel> Participants { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="ParticipantRole"/>s that may be assigned.
        /// </summary>
        public IReadOnlyList<ParticipantRole> PossibleRole { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="DomainOfExpertise"/>s that may be assigned.
        /// </summary>
        public IReadOnlyList<DomainOfExpertise> PossibleDomain { get; private set; }

        /// <summary>
        /// Gets or sets the default <see cref="ParticipantRole"/> that is applied to every selected
        /// <see cref="Person"/>. The role of an individual <see cref="Person"/> may still be overridden on its
        /// <see cref="BulkParticipantRowViewModel"/>.
        /// </summary>
        public ParticipantRole SelectedRole
        {
            get => this.selectedRole;
            set => this.RaiseAndSetIfChanged(ref this.selectedRole, value);
        }

        /// <summary>
        /// Gets or sets the default value applied to every selected <see cref="Person"/> indicating whether the
        /// created <see cref="Participant"/> shall be active. The active state of an individual <see cref="Person"/>
        /// may still be overridden on its <see cref="BulkParticipantRowViewModel"/>.
        /// </summary>
        public bool IsActive
        {
            get => this.isActive;
            set => this.RaiseAndSetIfChanged(ref this.isActive, value);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="OkCommand"/> can be executed.
        /// </summary>
        public bool OkCanExecute
        {
            get => this.okCanExecute;
            private set => this.RaiseAndSetIfChanged(ref this.okCanExecute, value);
        }

        /// <summary>
        /// Gets the Ok command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel command.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>s.
        /// </summary>
        private void InitializeReactiveCommands()
        {
            this.OkCommand = ReactiveCommandCreator.Create(this.ExecuteOk, this.WhenAnyValue(x => x.OkCanExecute));
            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);
        }

        /// <summary>
        /// Applies the batch-wide <see cref="SelectedRole"/> to every row as the default role for each
        /// <see cref="Person"/>. Individual rows may subsequently be overridden by the user.
        /// </summary>
        /// <param name="role">
        /// The <see cref="ParticipantRole"/> to apply to every row.
        /// </param>
        private void ApplyRoleToRows(ParticipantRole role)
        {
            foreach (var row in this.Participants)
            {
                row.SelectedRole = role;
            }
        }

        /// <summary>
        /// Applies the batch-wide <see cref="IsActive"/> value to every row as the default for each
        /// <see cref="Person"/>. Individual rows may subsequently be overridden by the user.
        /// </summary>
        /// <param name="isActive">
        /// A value indicating whether the created <see cref="Participant"/>s shall be active.
        /// </param>
        private void ApplyIsActiveToRows(bool isActive)
        {
            foreach (var row in this.Participants)
            {
                row.IsActive = isActive;
            }
        }

        /// <summary>
        /// Updates the <see cref="OkCanExecute"/> property.
        /// </summary>
        private void UpdateOkCanExecute()
        {
            var selectedRows = this.Participants.Where(x => x.IsSelected).ToList();

            this.OkCanExecute = selectedRows.Any() && selectedRows.All(x => x.SelectedDomain != null && x.SelectedRole != null);
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>.
        /// </summary>
        private void ExecuteOk()
        {
            var selectedRows = this.Participants.Where(x => x.IsSelected).ToList();

            this.DialogResult = new BulkParticipantCreationResult(true, selectedRows);
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>.
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }
    }
}
