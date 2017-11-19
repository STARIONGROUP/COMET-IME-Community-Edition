// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuaScriptPanelViewModelTestFIxture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Documents;
    using CDP4Dal;
    using CDP4Scripting.ViewModels;
    using Interfaces;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="LuaScriptPanelViewModel"/> class.
    /// </summary>
    [TestFixture, RequiresSTA]
    public class LuaScriptPanelViewModelTestFIxture
    {
        private Mock<IScriptingProxy> scriptingProxy;
        private ReactiveList<ISession> openSessions;
        private LuaScriptPanelViewModel luaScriptPanelViewModel;

        [SetUp]
        public void SetUp()
        {
            this.scriptingProxy = new Mock<IScriptingProxy>();
            this.openSessions = new ReactiveList<ISession>();

            this.luaScriptPanelViewModel = new LuaScriptPanelViewModel("lua script", this.scriptingProxy.Object, this.openSessions);
        }

        [Test]
        public void VerifyThatInitializationWorked()
        {
            Assert.AreEqual("Lua", this.luaScriptPanelViewModel.AvalonEditor.SyntaxHighlighting.Name);
        }

        [Test]
        [Ignore("Application.Current.Dispatcher is called to add text to the output but not created in the test environement.")]
        public void VerifyThatLuaCodeCanBeExecuted()
        {
            this.luaScriptPanelViewModel.Execute("print(\"Hello world\")");
            var outputContent = new TextRange(this.luaScriptPanelViewModel.OutputTerminal.Document.ContentStart, this.luaScriptPanelViewModel.OutputTerminal.Document.ContentEnd);
            Assert.AreEqual("Hello world \n", outputContent);

            this.luaScriptPanelViewModel.OutputTerminal.Document.Blocks.Clear();
            this.luaScriptPanelViewModel.Execute("return \"RHEA group\"");
            outputContent = new TextRange(this.luaScriptPanelViewModel.OutputTerminal.Document.ContentStart, this.luaScriptPanelViewModel.OutputTerminal.Document.ContentEnd);
            Assert.AreEqual("\"RHEA group\"", outputContent);

            var command = String.Format("{0}.Help()", ScriptPanelViewModel.Command);
            Assert.DoesNotThrow(() => this.luaScriptPanelViewModel.Execute(command));
            this.scriptingProxy.Verify(x => x.Help(), Times.Once);

            command = String.Format("{0}.help()", ScriptPanelViewModel.Command);
            Assert.DoesNotThrow(() => this.luaScriptPanelViewModel.Execute(command));
            this.scriptingProxy.Verify(x => x.Help(), Times.Exactly(2));
        }

        [Test]
        public void VerifyThatUpdateAndClearScopeVariablesWorks()
        {
            this.luaScriptPanelViewModel.Execute("a=2");
            this.luaScriptPanelViewModel.Execute("b=\"hello world\"");

            Assert.AreEqual(2, this.luaScriptPanelViewModel.ScriptVariables.Count);
            var pair = this.luaScriptPanelViewModel.ScriptVariables.ElementAt(0);
            var key = pair.Key;
            var value = pair.Value;
            Assert.AreEqual("\"a\"", key);
            Assert.AreEqual(2, value);
            pair = this.luaScriptPanelViewModel.ScriptVariables.ElementAt(1);
            key = pair.Key;
            value = pair.Value;
            Assert.AreEqual("\"b\"", key);
            Assert.AreEqual("hello world", value);

            this.luaScriptPanelViewModel.ClearScopeVariables();
            Assert.AreEqual(0, this.luaScriptPanelViewModel.ScriptVariables.Count);
        }
    }
}