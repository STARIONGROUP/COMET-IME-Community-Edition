// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuaScriptPanelViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4Scripting.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using Interfaces;

    using Microsoft.Scripting;

    using MoonSharp.Interpreter;
    using MoonSharp.Interpreter.Loaders;
    using MoonSharp.Interpreter.REPL;

    using ReactiveUI;

    using Views;

    /// <summary>
    /// The view-model for the <see cref="ScriptPanel"/> when the selected language is Lua.
    /// </summary>
    public class LuaScriptPanelViewModel : ScriptPanelViewModel
    {
        /// <summary>
        /// The path to the embedded resource which contains the information for the Lua syntax highlighting.
        /// </summary>
        private const string LuaHighlighting = "CDP4Scripting.Resources.SyntaxHighlightingSheets.Lua.xshd";

        /// <summary>
        /// The scope variables to not show in the local variables panel.
        /// </summary>
        private readonly string[] ignoredVariables;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaScriptPanelViewModel"/> class.
        /// </summary>
        /// <param name="panelTitle">The title of the panel associated to this view model.</param>
        /// <param name="scriptingProxy">A <see cref="IScriptingProxy"/> object to perform the script commands associated to CDP4.</param>
        /// <param name="openSessions">The list of the open <see cref="ISession"/>.</param>
        public LuaScriptPanelViewModel(string panelTitle, IScriptingProxy scriptingProxy, ReactiveList<ISession> openSessions) : base(panelTitle, scriptingProxy, "*.lua", openSessions, true)
        {
            this.LoadHighlightingSheet(LuaHighlighting);

            UserData.RegisterType<IScriptingProxy>();
            this.CommandObject = UserData.Create(this.ScriptingProxy);

            // We create an object to interprete the Lua script and we register IScriptingProxy to use its methods in the script
            this.Interpreter = new Script();
            Interpreter.Globals.Set(Command, this.CommandObject);
            Interpreter.Options.DebugPrint = s =>
            {
                Application.Current.Dispatcher.Invoke(
                       DispatcherPriority.Input,
                       new Action(() => this.OutputTerminal.AppendText(string.Format("{0} \n", s))));
            };

            this.ignoredVariables = Interpreter.Globals.Keys.Select(x => x.ToString()).ToArray();
        }

        /// <summary>
        /// The <see cref="IScriptingProxy"/> object.
        /// </summary>
        private DynValue CommandObject { get; set; }

        /// <summary>
        /// The MoonSharp scripting session that interprets the lua scripts.
        /// </summary>
        private Script Interpreter { get; set; }

        /// <summary>
        /// Executes the supplied Lua script.
        /// </summary>
        /// <param name="script">The string of the Lua script that is to be executed.</param>
        public override void Execute(string script)
        {
            try
            {
                // Check if the script should be stopped
                this.CancellationToken.ThrowIfCancellationRequested();

                this.ScriptingProxy.ScriptingPanelViewModel = this;

                // Allow the user to load scripts located in pathLuaScriptsDirectory
                var pathLuaScriptsDirectory = @"C:\Users\Mathieu\Documents\COMET scripts\?";
                var pathLuaFiles = pathLuaScriptsDirectory + ".lua";
                Script.DefaultOptions.ScriptLoader = new ReplInterpreterScriptLoader();
                ((ScriptLoaderBase)Script.DefaultOptions.ScriptLoader).ModulePaths = new[] { pathLuaScriptsDirectory, pathLuaFiles };

                // Execute the script
                var resultScript = Interpreter.DoString(script);

                if (resultScript.ToString() != "void")
                {
                    Application.Current.Dispatcher.Invoke(
                       DispatcherPriority.Input,
                       new Action(() => this.OutputTerminal.AppendText(resultScript.ToString())));
                }
            }
            catch (ThreadAbortException tae)
            {
                // If a ThreadAbortException occured but the user didn't choose to stop the script
                if (!(tae.ExceptionState is KeyboardInterruptException))
                {
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Input,
                        new Action(() => this.OutputTerminal.AppendText(string.Format("\nAn error occured during the execution of the script !\nError: {0}\n", tae.Message))));
                }

                // The abortion of the thread is cancelled to avoid the loss of data, the cancelletation token is checked and will throw an exception to cancel the task properly if necessary
                Thread.ResetAbort();
                this.CancellationToken.ThrowIfCancellationRequested();
            }
            // If CancellationRequested
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException(this.CancellationToken);
            }
            // Other kinds of exceptions
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Input,
                    new Action(() => this.OutputTerminal.AppendText(string.Format("\nAn error occured during the execution of the script !\nError: {0}\n", ex.Message))));
            }
            finally
            {
                this.UpdateScopeVariables();
            }
        }

        /// <summary>
        /// Updates the local variables list that is displayed in a panel of the scripting engine.
        /// </summary>
        public void UpdateScopeVariables()
        {
            var pairs = this.Interpreter.Globals.Pairs
                .Where(x => !this.ignoredVariables.Contains(x.Key.ToString()))
                .OrderBy(pair => pair.Key.ToString())
                .ToList();

            var tempList = new List<KeyValuePair<string, dynamic>>();
            foreach (var pair in pairs)
            {
                tempList.Add(new KeyValuePair<string, dynamic>(pair.Key.ToString(), pair.Value.ToDynamic()));
            }
            this.ScriptVariables = tempList;
        }

        /// <summary>
        /// Clears the variables of the scope of the script.
        /// </summary>
        public override void ClearScopeVariables()
        {
            // We create an object to interprete the Lua script and we register IScriptingProxy to use its methods in the script
            this.Interpreter = new Script();
            Interpreter.Globals.Set(Command, this.CommandObject);
            Interpreter.Options.DebugPrint = s =>
            {
                Application.Current.Dispatcher.Invoke(
                       DispatcherPriority.Input,
                       new Action(() => this.OutputTerminal.AppendText(string.Format("{0} \n", s))));
            };

            this.UpdateScopeVariables();
        }
    }
}