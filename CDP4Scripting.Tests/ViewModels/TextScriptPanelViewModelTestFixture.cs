// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextScriptPanelViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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

namespace CDP4Scripting.Tests.ViewModels
{
    using System.Threading;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Scripting.Interfaces;
    using CDP4Scripting.ViewModels;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="TextScriptPanelViewModel"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class TextScriptPanelViewModelTestFixture : DispatcherTestFixture
    {
        private TextScriptPanelViewModel textScriptPanelViewModel;
        private Mock<IScriptingProxy> scriptingProxy;
        private ReactiveList<ISession> openSessions;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.scriptingProxy = new Mock<IScriptingProxy>();
            this.openSessions = new ReactiveList<ISession>();
            this.textScriptPanelViewModel = new TextScriptPanelViewModel("text script", this.scriptingProxy.Object, this.messageBus, this.openSessions);
        }

        [Test]
        public void VerfifyThatShowWhitespacesWorks()
        {
            Assert.That(this.textScriptPanelViewModel.IsShowWhitespacesButtonVisible, Is.True);
            Assert.That(this.textScriptPanelViewModel.ShowWhitespaces, Is.False);

            Assert.That(this.textScriptPanelViewModel.AvalonEditor.Options.ShowTabs, Is.False);
            Assert.That(this.textScriptPanelViewModel.AvalonEditor.Options.ShowSpaces, Is.False);

            this.textScriptPanelViewModel.ShowWhitespaces = true;

            Assert.That(this.textScriptPanelViewModel.AvalonEditor.Options.ShowTabs, Is.True);
            Assert.That(this.textScriptPanelViewModel.AvalonEditor.Options.ShowSpaces, Is.True);

            this.textScriptPanelViewModel.IsShowWhitespacesButtonVisible = false;

            Assert.That(this.textScriptPanelViewModel.ShowWhitespaces, Is.False);

            Assert.That(this.textScriptPanelViewModel.AvalonEditor.Options.ShowTabs, Is.False);
            Assert.That(this.textScriptPanelViewModel.AvalonEditor.Options.ShowSpaces, Is.False);
        }
    }
}
