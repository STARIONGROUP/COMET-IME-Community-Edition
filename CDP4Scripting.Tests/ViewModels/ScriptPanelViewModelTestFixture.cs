// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptingEngineTabViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
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

namespace CDP4Scripting.Tests.ViewModels
{
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Documents;

    using CDP4Composition.Mvvm;

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
            Assert.DoesNotThrowAsync(async () => await this.scriptPanelViewModel.Object.ExecuteScriptCommand.Execute());
            Assert.DoesNotThrowAsync(async () => await  this.scriptPanelViewModel.Object.StopScriptCommand.Execute());
            Assert.DoesNotThrowAsync(async () => await  this.scriptPanelViewModel.Object.SaveScriptCommand.Execute());
        }

        [Test]
        public void VerfifyThatClearOutputCommandWorks()
        {
            this.scriptPanelViewModel.Object.OutputTerminal.AppendText("Content of the output");
            Assert.DoesNotThrowAsync(async () => await this.scriptPanelViewModel.Object.ClearOutputCommand.Execute());
            var outputContent = new TextRange(this.scriptPanelViewModel.Object.OutputTerminal.Document.ContentStart, this.scriptPanelViewModel.Object.OutputTerminal.Document.ContentEnd);
            Assert.AreEqual(true, outputContent.IsEmpty);
        }
    }
}