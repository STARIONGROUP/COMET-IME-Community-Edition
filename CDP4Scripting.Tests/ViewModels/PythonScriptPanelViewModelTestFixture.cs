// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PythonScriptPanelViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Documents;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Scripting.Interfaces;
    using CDP4Scripting.ViewModels;

    using IronPython.Hosting;

    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="PythonScriptPanelViewModel"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class PythonScriptPanelViewModelTestFixture : DispatcherTestFixture
    {
        private PythonScriptPanelViewModel pythonScriptPanelViewModel;
        private Mock<IScriptingProxy> scriptingProxy;
        private ReactiveList<ISession> openSessions;
        private ScriptEngine pythonEngine;
        private ScriptScope scope;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.scriptingProxy = new Mock<IScriptingProxy>();
            this.openSessions = new ReactiveList<ISession>();
            this.pythonScriptPanelViewModel = new PythonScriptPanelViewModel("python script", this.scriptingProxy.Object, this.messageBus, this.openSessions);

            this.pythonEngine = Python.CreateEngine();
            this.scope = this.pythonEngine.CreateScope();
        }

        [Test]
        public void VerifyThatInitializationWorked()
        {
            Assert.AreEqual("Python", this.pythonScriptPanelViewModel.AvalonEditor.SyntaxHighlighting.Name);
        }

        [Test]
        public async Task VerifyThatPythonCodeCanBeExecuted()
        {
            await this.pythonScriptPanelViewModel.ClearOutputCommand.Execute();
            this.pythonScriptPanelViewModel.Execute("print(\"Hello, world!\")");
            var outputContent = new TextRange(this.pythonScriptPanelViewModel.OutputTerminal.Document.ContentStart, this.pythonScriptPanelViewModel.OutputTerminal.Document.ContentEnd);

            Assert.That(outputContent.Text, Is.EqualTo("Hello, world!\r\n"));

            var command = $"{ScriptPanelViewModel.Command}.Help()";
            Assert.DoesNotThrow(() => this.pythonScriptPanelViewModel.Execute(command));
            this.scriptingProxy.Verify(x => x.Help(), Times.Once);
        }

        [Test]
        public void VerifyThatParameterWorks()
        {
            this.pythonEngine = Python.CreateEngine();
            this.scope = this.pythonEngine.CreateScope();

            //warning: if you tab or add space to the python code (to put it in line with C# code).The test will fail (unexpected indent)
            var printHello = @"
def PrintHello(name):
  msg = 'Hello ' + name
  return msg
";

            var source = this.pythonEngine.CreateScriptSourceFromString(printHello, SourceCodeKind.Statements);
            source.Execute(this.scope);

            var fPrintHello = this.scope.GetVariable<Func<string, string>>("PrintHello");
            var result = fPrintHello("Leiden");
            Assert.IsTrue(result == "Hello Leiden");
        }

        [Test]
        public void VerifyThatUpdateAndClearScopeVariablesWorks()
        {
            this.pythonScriptPanelViewModel.Execute("a=2");
            this.pythonScriptPanelViewModel.Execute("b=\"hello world\"");

            Assert.That(this.pythonScriptPanelViewModel.ScriptVariables.Count, Is.EqualTo(4));

            var pair = this.pythonScriptPanelViewModel.ScriptVariables.ElementAt(2);
            var key = pair.Key;
            var value = pair.Value;

            Assert.That(key, Is.EqualTo("a"));
            Assert.That(value, Is.EqualTo(2));

            pair = this.pythonScriptPanelViewModel.ScriptVariables.ElementAt(3);
            key = pair.Key;
            value = pair.Value;

            Assert.That(key, Is.EqualTo("b"));
            Assert.That(value, Is.EqualTo("hello world"));

            this.pythonScriptPanelViewModel.ClearScopeVariables();
            Assert.AreEqual(0, this.pythonScriptPanelViewModel.ScriptVariables.Count);
        }
    }
}
