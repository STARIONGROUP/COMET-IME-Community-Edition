// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BulkParticipantCreationDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;

    using CDP4SiteDirectory.ViewModels;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class BulkParticipantCreationDialogViewModelTestFixture
    {
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private DomainOfExpertise thermal;
        private DomainOfExpertise systems;
        private ParticipantRole role;
        private Person john;
        private Person jane;
        private Person personWithoutDefaultDomain;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.uri = new Uri("https://www.stariongroup.eu");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.thermal = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Thermal" };
            this.systems = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Systems" };

            this.role = new ParticipantRole(Guid.NewGuid(), this.cache, this.uri) { Name = "Team Member" };

            this.john = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "John", Surname = "Doe", DefaultDomain = this.thermal };
            this.jane = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "Jane", Surname = "Roe", DefaultDomain = this.systems };
            this.personWithoutDefaultDomain = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "No", Surname = "Domain" };
        }

        private BulkParticipantCreationDialogViewModel CreateDialogViewModel()
        {
            return new BulkParticipantCreationDialogViewModel(
                new[] { this.john, this.jane, this.personWithoutDefaultDomain },
                new[] { this.role },
                new[] { this.thermal, this.systems });
        }

        private BulkParticipantRowViewModel RowFor(BulkParticipantCreationDialogViewModel viewModel, Person person)
        {
            return viewModel.Participants.Single(x => x.Person == person);
        }

        [Test]
        public void VerifyThatArgumentNullExceptionsAreThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new BulkParticipantCreationDialogViewModel(null, new[] { this.role }, new[] { this.thermal }));
            Assert.Throws<ArgumentNullException>(() => new BulkParticipantCreationDialogViewModel(new[] { this.john }, null, new[] { this.thermal }));
            Assert.Throws<ArgumentNullException>(() => new BulkParticipantCreationDialogViewModel(new[] { this.john }, new[] { this.role }, null));
        }

        [Test]
        public void VerifyThatRowsArePopulatedWithDefaults()
        {
            var viewModel = this.CreateDialogViewModel();

            Assert.That(viewModel.Participants.Count, Is.EqualTo(3));
            Assert.That(viewModel.Participants.All(x => x.IsSelected), Is.True);
            Assert.That(viewModel.IsActive, Is.True);

            Assert.That(this.RowFor(viewModel, this.john).SelectedDomain, Is.EqualTo(this.thermal));
            Assert.That(this.RowFor(viewModel, this.jane).SelectedDomain, Is.EqualTo(this.systems));
            Assert.That(this.RowFor(viewModel, this.personWithoutDefaultDomain).SelectedDomain, Is.Null);

            Assert.That(this.RowFor(viewModel, this.john).PossibleDomain, Is.EquivalentTo(new[] { this.thermal, this.systems }));
        }

        [Test]
        public void VerifyThatOkCanExecuteRequiresRoleAndDomainPerSelectedRow()
        {
            var viewModel = this.CreateDialogViewModel();

            Assert.That(viewModel.OkCanExecute, Is.False);

            viewModel.SelectedRole = this.role;

            // the person without a default domain has no domain selected yet
            Assert.That(viewModel.OkCanExecute, Is.False);

            this.RowFor(viewModel, this.personWithoutDefaultDomain).SelectedDomain = this.thermal;

            Assert.That(viewModel.OkCanExecute, Is.True);

            foreach (var row in viewModel.Participants)
            {
                row.IsSelected = false;
            }

            Assert.That(viewModel.OkCanExecute, Is.False);
        }

        [Test]
        public void VerifyThatDeselectingPersonWithoutDomainAllowsOk()
        {
            var viewModel = this.CreateDialogViewModel();

            viewModel.SelectedRole = this.role;

            // exclude the person without a default domain; the remaining persons resolve to their own default domain
            this.RowFor(viewModel, this.personWithoutDefaultDomain).IsSelected = false;

            Assert.That(viewModel.OkCanExecute, Is.True);
        }

        [Test]
        public async Task VerifyThatOkResultAppliesBatchValuesToSelectedRows()
        {
            var viewModel = this.CreateDialogViewModel();

            viewModel.SelectedRole = this.role;
            viewModel.IsActive = false;
            this.RowFor(viewModel, this.personWithoutDefaultDomain).SelectedDomain = this.thermal;

            await viewModel.OkCommand.Execute();

            var result = viewModel.DialogResult as BulkParticipantCreationResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Result, Is.True);
            Assert.That(result.Participants.Count(), Is.EqualTo(3));
            Assert.That(result.Participants.All(x => x.SelectedRole == this.role), Is.True);
            Assert.That(result.Participants.All(x => !x.IsActive), Is.True);

            Assert.That(result.Participants.Single(x => x.Person == this.john).SelectedDomain, Is.EqualTo(this.thermal));
            Assert.That(result.Participants.Single(x => x.Person == this.jane).SelectedDomain, Is.EqualTo(this.systems));
            Assert.That(result.Participants.Single(x => x.Person == this.personWithoutDefaultDomain).SelectedDomain, Is.EqualTo(this.thermal));
        }

        [Test]
        public async Task VerifyThatOkResultContainsOnlySelectedParticipants()
        {
            var viewModel = this.CreateDialogViewModel();

            viewModel.SelectedRole = this.role;
            this.RowFor(viewModel, this.personWithoutDefaultDomain).IsSelected = false;

            await viewModel.OkCommand.Execute();

            var result = viewModel.DialogResult as BulkParticipantCreationResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Participants.Count(), Is.EqualTo(2));
            Assert.That(result.Participants.Any(x => x.Person == this.personWithoutDefaultDomain), Is.False);
        }

        [Test]
        public async Task VerifyThatCancelSetsNegativeResult()
        {
            var viewModel = this.CreateDialogViewModel();

            await viewModel.CancelCommand.Execute();

            Assert.That(viewModel.DialogResult, Is.Not.Null);
            Assert.That(viewModel.DialogResult.Result, Is.False);
        }

        [Test]
        public void VerifyThatOkCommandCanExecuteIsGatedByOkCanExecute()
        {
            var viewModel = this.CreateDialogViewModel();

            Assert.That(((ICommand)viewModel.OkCommand).CanExecute(null), Is.False);

            viewModel.SelectedRole = this.role;
            this.RowFor(viewModel, this.personWithoutDefaultDomain).SelectedDomain = this.thermal;

            Assert.That(((ICommand)viewModel.OkCommand).CanExecute(null), Is.True);
        }
    }
}
