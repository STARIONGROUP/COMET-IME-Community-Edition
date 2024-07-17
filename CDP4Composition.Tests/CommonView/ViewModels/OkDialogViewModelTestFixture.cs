// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyConfirmationDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4CommonView.Tests.ViewModels
{
    using CDP4CommonView.ViewModels;
    using NUnit.Framework;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    /// <summary>
    /// Suite of tests for the <see cref="OkDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class OkDialogViewModelTestFixture
    {
        private OkDialogViewModel viewModel;

        [Test]
        public void VerityThatPropertiesAreSetByConstructor()
        {
            this.viewModel = new OkDialogViewModel("the title", "the message");

            Assert.AreEqual("the title", viewModel.Title);
            Assert.AreEqual("the message", viewModel.Message);
        }

        [Test]
        public async Task VerifyThatYesCommandWorksAsExpected()
        {
            this.viewModel = new OkDialogViewModel("", "");
            Assert.IsTrue(((ICommand)this.viewModel.OkCommand).CanExecute(null));

            await this.viewModel.OkCommand.Execute();

            Assert.IsTrue((bool)this.viewModel.DialogResult.Result.Value);
        }
    }
}
