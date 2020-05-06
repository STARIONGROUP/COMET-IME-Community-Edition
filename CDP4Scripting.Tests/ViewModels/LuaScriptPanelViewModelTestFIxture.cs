// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuaScriptPanelViewModelTestFIxture.cs" company="RHEA System S.A.">
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
    using System;
    using System.Linq;
    using System.Threading;
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
    [TestFixture, Apartment(ApartmentState.STA)]
    public class LuaScriptPanelViewModelTestFIxture : DispatcherTestFixture
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
        public void VerifyThatLuaCodeCanBeExecuted()
        {
            this.luaScriptPanelViewModel.OutputTerminal.Document.Blocks.Clear();
            this.luaScriptPanelViewModel.Execute("print(\"Hello world\")");
            var outputContent = new TextRange(this.luaScriptPanelViewModel.OutputTerminal.Document.ContentStart, this.luaScriptPanelViewModel.OutputTerminal.Document.ContentEnd);
            Assert.AreEqual("Hello world \r\n", outputContent.Text);

            this.luaScriptPanelViewModel.OutputTerminal.Document.Blocks.Clear();
            this.luaScriptPanelViewModel.Execute("return \"RHEA group\"");
            outputContent = new TextRange(this.luaScriptPanelViewModel.OutputTerminal.Document.ContentStart, this.luaScriptPanelViewModel.OutputTerminal.Document.ContentEnd);
            Assert.AreEqual("\"RHEA group\"\r\n", outputContent.Text);

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