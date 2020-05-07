// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptingEngineTabViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Tests.ViewModels
{
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Windows.Documents;

    using CDP4Dal;
    
    using CDP4Scripting.Helpers;
    using CDP4Scripting.ViewModels;
    
    using ICSharpCode.AvalonEdit;
    
    using Interfaces;
    
    using Moq;
    
    using NUnit.Framework;
    
    using ReactiveUI;

    /// <summary>
    /// Suite of test for the <see cref="ScriptPanelViewModel"/> class.
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ScriptPanelViewModelTestFixture : DispatcherTestFixture
    {
        private Mock<ScriptPanelViewModel> scriptPanelViewModel;
        private Mock<IScriptingProxy> scriptingProxy;
        private ReactiveList<ISession> openSessions;
        private OutputTerminal outputTerminal;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.scriptingProxy = new Mock<IScriptingProxy>();
            this.openSessions = new ReactiveList<ISession>();
            var avalonEditor = new TextEditor();
            this.outputTerminal = new OutputTerminal();
            this.scriptPanelViewModel = new Mock<ScriptPanelViewModel>("header", this.scriptingProxy.Object, "*.lua", this.openSessions, true);
            this.scriptPanelViewModel.SetupProperty(x => x.AvalonEditor, avalonEditor);
            this.scriptPanelViewModel.SetupProperty(x => x.Caption, "header*");
        }

        [Test]
        public void VerifyThatLoadHighlightingSheetWorks()
        {
            var resourcePath = "CDP4Scripting.Resources.languageNotSupported.xshd";
            this.scriptPanelViewModel.Object.LoadHighlightingSheet(resourcePath);
            Assert.AreEqual(null, this.scriptPanelViewModel.Object.AvalonEditor.SyntaxHighlighting);

            resourcePath = "CDP4Scripting.Resources.SyntaxHighlightingSheets.Lua.xshd";
            this.scriptPanelViewModel.Object.LoadHighlightingSheet(resourcePath);
            Assert.AreEqual("Lua", this.scriptPanelViewModel.Object.AvalonEditor.SyntaxHighlighting.Name);
        }

        [Test]
        public void VerifyThatCommandsWork()
        {
            Assert.DoesNotThrow(() => this.scriptPanelViewModel.Object.ExecuteScriptCommand.Execute(null));
            Assert.DoesNotThrow(() => this.scriptPanelViewModel.Object.StopScriptCommand.Execute(null));
            Assert.DoesNotThrow(() => this.scriptPanelViewModel.Object.SaveScriptCommand.Execute(null));
        }

        [Test]
        public void VerfifyThatClearOutputCommandWorks()
        {
            this.scriptPanelViewModel.Object.OutputTerminal.AppendText("Content of the output");
            Assert.DoesNotThrow(() => this.scriptPanelViewModel.Object.ClearOutputCommand.Execute(null));
            var outputContent = new TextRange(this.scriptPanelViewModel.Object.OutputTerminal.Document.ContentStart, this.scriptPanelViewModel.Object.OutputTerminal.Document.ContentEnd);
            Assert.AreEqual(true, outputContent.IsEmpty);
        }
    }
}