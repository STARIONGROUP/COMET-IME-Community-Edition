// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericConfirmationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Composition.ViewModels;

    using NUnit.Framework;
    
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    [TestFixture]
    public class GenericConfirmationDialogViewModelTestFixture
    {
        private GenericConfirmationDialogViewModel viewModel;

        [Test]
        public void VerityThatPropertiesAreSetByConstructor()
        {
            this.viewModel = new GenericConfirmationDialogViewModel("the title", "the message");

            Assert.AreEqual("the title", viewModel.Title);
            Assert.AreEqual("the message", viewModel.Message);
        }

        [Test]
        public async Task VerifyThatYesCommandWorksAsExpected()
        {
            this.viewModel = new GenericConfirmationDialogViewModel("", "");
            Assert.IsTrue(((ICommand)this.viewModel.YesCommand).CanExecute(null));

            await this.viewModel.YesCommand.Execute();

            Assert.IsTrue((bool)this.viewModel.DialogResult.Result.Value);
        }

        [Test]
        public async Task VerifyThatNOCommandWorksAsExpected()
        {
            this.viewModel = new GenericConfirmationDialogViewModel("", "");
            Assert.IsTrue(((ICommand)this.viewModel.NoCommand).CanExecute(null));

            await this.viewModel.NoCommand.Execute();

            Assert.IsFalse((bool)this.viewModel.DialogResult.Result.Value);
        }
    }
}
