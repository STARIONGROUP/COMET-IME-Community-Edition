// -------------------------------------------------------------------------------------------------
// <copyright file="ThingSelectorDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.ViewModels
{
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.ViewModels;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Test suiste for the <see cref="ThingSelectorDialogViewModel{T}"/> class
    /// </summary>
    [TestFixture]
    public class ThingSelectorDialogViewModelTestFixture
    {
        private ThingSelectorDialogViewModel<Person> viewModel;

        private Person[] persons;
        
        private string[] fieldNames;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.persons = new[] { new Person(), new Person(), new Person() };
            this.fieldNames = new[] { nameof(Person.Name), nameof(Person.UserFriendlyName) };
            this.viewModel = new ThingSelectorDialogViewModel<Person>(this.persons, this.fieldNames);
        }

        [Test]
        public void VerityThatPropertiesAreSetByConstructor()
        {
            CollectionAssert.AreEqual(this.viewModel.Things, this.persons);
            CollectionAssert.AreEqual(this.viewModel.Columns.Select(x => x.FieldName), this.fieldNames);

            Assert.IsTrue(this.viewModel.Title.Contains(nameof(Person)));
        }

        [Test]
        public async Task VerifyThatOkCommandWorksAsExpected()
        {
            Assert.IsFalse(((ICommand)this.viewModel.OkCommand).CanExecute(null));
            this.viewModel.SelectedThing = this.viewModel.Things.First();
            Assert.IsTrue(((ICommand)this.viewModel.OkCommand).CanExecute(null));

            await this.viewModel.OkCommand.Execute();

            Assert.IsTrue(this.viewModel.DialogResult);
        }

        [Test]
        public async Task VerifyThatCancelCommandWorksAsExpected()
        {
            Assert.IsTrue(((ICommand)this.viewModel.CancelCommand).CanExecute(null));
            await this.viewModel.CancelCommand.Execute();

            Assert.IsFalse(this.viewModel.DialogResult);
        }
    }
}
