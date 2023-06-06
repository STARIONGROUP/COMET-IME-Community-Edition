// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PythonScriptPanelViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
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
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows;
    using System.Windows.Threading;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Scripting.Events;
    using CDP4Scripting.Interfaces;
    using CDP4Scripting.Views;

    using IronPython.Hosting;

    using Microsoft.Scripting;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="ScriptPanel"/> view
    /// </summary>
    public class PythonScriptPanelViewModel : ScriptPanelViewModel
    {
        /// <summary>
        /// The path to the embedded resource which contains the information for the Python syntax highlighting.
        /// </summary>
        private const string PythonHighlighting = "CDP4Scripting.Resources.SyntaxHighlightingSheets.Python.xshd";

        /// <summary>
        /// The scope variables to not show in the local variables panel.
        /// </summary>
        private static readonly string[] IgnoredVariables = { "Command", "__builtins__", "__file__", "__name__", "__doc__" };

        /// <summary>
        /// Constructor of the <see cref="PythonScriptPanelViewModel"/>
        /// </summary>
        /// <param name="panelTitle">The title of the panel associated to this view model.</param>
        /// <param name="scriptingProxy">A <see cref="IScriptingProxy"/> object to perform the script commands associated to CDP4.</param>
        /// <param name="openSessions">The list of the open <see cref="ISession"/>.</param>
        public PythonScriptPanelViewModel(string panelTitle, IScriptingProxy scriptingProxy, ReactiveList<ISession> openSessions) : base(panelTitle, scriptingProxy, "*.py", openSessions, true)
        {
            this.LoadHighlightingSheet(PythonHighlighting);

            this.Runtime = IronPython.Hosting.Python.CreateRuntime();
            this.Runtime.LoadAssembly(typeof(string).Assembly);
            this.Runtime.LoadAssembly(typeof(Uri).Assembly);

            var streamWriter = new EventingMemoryStream();
            streamWriter.OnNewText += this.StreamWriter_OnNewText;

            this.Runtime.IO.SetOutput(streamWriter, Encoding.ASCII);

            this.Engine = Python.GetEngine(this.Runtime);

            var searchPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var paths = this.Engine.GetSearchPaths();
            paths.Add(searchPath);
            this.Engine.SetSearchPaths(paths);

            this.Scope = this.Engine.CreateScope();
            this.Scope.SetVariable(Command, this.ScriptingProxy);
        }

        /// <summary>
        /// Gets or sets the scope of the script.
        /// </summary>
        private Microsoft.Scripting.Hosting.ScriptScope Scope { get; set; }

        /// <summary>
        /// Executes the supplied Python script
        /// </summary>
        /// <param name="script">
        /// The string of the Python script that is to be executed
        /// </param>
        public override void Execute(string script)
        {
            try
            {
                this.CancellationToken.ThrowIfCancellationRequested();

                this.ScriptingProxy.ScriptingPanelViewModel = this;

                var source = this.Engine.CreateScriptSourceFromString(script, SourceCodeKind.AutoDetect);

                source.Execute(this.Scope);
            }
            catch (ThreadAbortException tae)
            {
                // If a ThreadAbortException occured but the user didn't choose to stop the script
                if (!(tae.ExceptionState is KeyboardInterruptException))
                {
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Input,
                        new Action(() =>this.OutputTerminal.AppendText($"\nAn error occured during the execution of the script !\nError: {tae.Message}\n")));
                }

                // The abortion of the thread is cancelled to avoid the loss of data, the cancelletation token is checked and will throw an exception to cancel the task properly if necessary
                Thread.ResetAbort();
                this.Engine.Runtime.Shutdown();
                this.CancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException(this.CancellationToken);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Input,
                    new Action(() =>this.OutputTerminal.AppendText($"\nAn error occured during the execution of the script !\nError: {ex.Message}\n")));
            }
            finally
            {
                this.UpdateScopeVariables();
            }
        }

        /// <summary>
        /// Event handler used to update the output window
        /// </summary>
        /// <param name="sender">
        /// the sender of the event
        /// </param>
        /// <param name="e">
        /// the string that is used to update the output window content with
        /// </param>
        private void StreamWriter_OnNewText(object sender, string e)
        {
            try
            {
                if (this.CancellationToken.IsCancellationRequested)
                {
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Input,
                        new Action(() => this.OutputTerminal.AppendText("Script Execution Cancelled")));

                    throw new OperationCanceledException(this.CancellationToken);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Input,
                        new Action(() => this.OutputTerminal.AppendText(e)));
                }
            }
            catch (ThreadAbortException tae)
            {
                // If a ThreadAbortException occured but the user didn't choose to stop the script
                if (!(tae.ExceptionState is KeyboardInterruptException))
                {
                    Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Input,
                        new Action(() => this.OutputTerminal.AppendText($"\nAn error occured during the execution of the script !\nError: {tae.Message}\n")));
                }

                // The abortion of the thread is cancelled to avoid the loss of data, the cancelletation token is checked and will throw an exception to cancel the task properly if necessary
                Thread.ResetAbort();
                this.Engine.Runtime.Shutdown();
                this.CancellationToken.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                throw new OperationCanceledException(this.CancellationToken);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Input,
                    new Action(() => this.OutputTerminal.AppendText($"\nAn error occured during the execution of the script !\nError: {ex.Message}\n")));
            }
        }

        /// <summary>
        /// Updates the local variables list that is displayed in a panel of the scripting engine.
        /// </summary>
        public void UpdateScopeVariables()
        {
            this.ScriptVariables = this.Scope.GetItems()
                .Where(x => !IgnoredVariables.Contains(x.Key))
                .OrderBy(pair => pair.Key)
                .ToList();
        }

        /// <summary>
        /// Clears the variables of the scope of the script.
        /// </summary>
        public override void ClearScopeVariables()
        {
            this.Scope = this.Engine.CreateScope();
            this.Scope.SetVariable(Command, this.ScriptingProxy);
            this.UpdateScopeVariables();
        }
    }
}
