// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChangeOwnershipSelectionDialogViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.SiteDirectoryData;

    using CDP4EngineeringModel.ViewModels.Dialogs;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ChangeOwnershipSelectionDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class ChangeOwnershipSelectionDialogViewModelTestFixture
    {
        private IEnumerable<DomainOfExpertise> domainOfExpertises;
        private DomainOfExpertise systemEngineering;
        private DomainOfExpertise powerEngineering;
        private ChangeOwnershipSelectionDialogViewModel changeOwnershipSelectionDialogViewModel;

        [SetUp]
        public void SetUp()
        {
            this.systemEngineering = new DomainOfExpertise { Name = "System Engineering", ShortName = "SYS" };
            this.powerEngineering = new DomainOfExpertise { Name = "Power Engineering", ShortName = "PWR" };

            this.domainOfExpertises = new List<DomainOfExpertise> {this.systemEngineering, this.powerEngineering};

            this.changeOwnershipSelectionDialogViewModel = new ChangeOwnershipSelectionDialogViewModel(this.domainOfExpertises);
        }

        [Test]
        public void Verify_that_ArgumentNullException_is_thrown_when_domain_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeOwnershipSelectionDialogViewModel(null));
        }

        [Test]
        public async Task Verify_that_when_OKCommand_is_executed_Result_is_as_Expected()
        {
            this.changeOwnershipSelectionDialogViewModel.SelectedOwner = this.powerEngineering;

            await this.changeOwnershipSelectionDialogViewModel.OkCommand.Execute();

            var dialogResult = this.changeOwnershipSelectionDialogViewModel.DialogResult;

            Assert.That(dialogResult.Result.HasValue, Is.True);

            var changeOwnershipSelectionResult = dialogResult as ChangeOwnershipSelectionResult;

            Assert.That(changeOwnershipSelectionResult.DomainOfExpertise, Is.EqualTo(this.powerEngineering));
        }

        [Test]
        public async Task Verify_that_when_CancelCommand_is_executed_Result_is_as_Expected()
        {
            Assert.That(((ICommand)this.changeOwnershipSelectionDialogViewModel.CancelCommand).CanExecute(false), Is.True);

            await this.changeOwnershipSelectionDialogViewModel.CancelCommand.Execute();

            var dialogResult = this.changeOwnershipSelectionDialogViewModel.DialogResult;

            Assert.That(dialogResult.Result.HasValue, Is.True);

            Assert.That(dialogResult.Result, Is.False);
        }
    }
}
