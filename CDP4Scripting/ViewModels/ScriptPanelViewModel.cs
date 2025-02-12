﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptPanelViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Scripting.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Xml;

    using CDP4Composition;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Scripting.Events;
    using CDP4Scripting.Helpers;
    using CDP4Scripting.Interfaces;
    using CDP4Scripting.Views;

    using ICSharpCode.AvalonEdit;
    using ICSharpCode.AvalonEdit.CodeCompletion;
    using ICSharpCode.AvalonEdit.Highlighting;
    using ICSharpCode.AvalonEdit.Highlighting.Xshd;
    using ICSharpCode.AvalonEdit.Search;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="ScriptPanel"/> view
    /// </summary>    
    public abstract class ScriptPanelViewModel : ReactiveObject, IScriptPanelViewModel
    {
        /// <summary>
        /// The string the user has to write to access the instance of the <see cref="CDP4Scripting.Helpers.ScriptingProxy"/> class and use its methods. 
        /// </summary>
        public const string Command = "Command";

        /// <summary>
        /// The path of the xaml file of the search bar of the text editor
        /// </summary>
        public const string EditorSearchPanelPath = "pack://application:,,,/CDP4Scripting;component/Resources/EditorSearchPanel.xaml";

        /// <summary>
        /// The Nlog Logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="Caption"/>.
        /// </summary>
        private string caption;

        /// <summary>
        /// Backing field for <see cref="ScriptVariables"/>.
        /// </summary>
        private IList<KeyValuePair<string, dynamic>> scriptVariables;

        /// <summary>
        /// Backing field for <see cref="IsRunButtonVisible"/>.
        /// </summary>
        private bool isRunButtonVisible = true;

        /// <summary>
        /// Backing field for <see cref="IsSelectSessionVisible"/>.
        /// </summary>
        private bool isSelectSessionVisible = true;

        /// <summary>
        /// Backing field for <see cref="IsClearOutputButtonVisible"/>.
        /// </summary>
        private bool isClearOutputButtonVisible = true;

        /// <summary>
        /// Backing field for <see cref="IsShowWhitespacesButtonVisible"/>.
        /// </summary>
        private bool isShowWhitespacesButtonVisible = true;

        /// <summary>
        /// Backing field for <see cref="ShowWhitespaces"/>.
        /// </summary>
        private bool showWhitespaces = false;

        /// <summary>
        /// Backing field for <see cref="IsStopScriptButtonVisible"/>.
        /// </summary>
        private bool isStopScriptButtonVisible = true;

        /// <summary>
        /// Backing field for <see cref="AreTerminalsVisible"/>.
        /// </summary>
        private bool areTerminalsVisible;

        /// <summary>
        /// Backing field for <see cref="IsScriptVariablesPanelVisible"/>.
        /// </summary>
        private bool isLocalVariablePanelVisible = true;

        /// <summary>
        /// Backing field for <see cref="IsDirty"/>.
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// Backing field for the <see cref="SelectedSession"/> property
        /// </summary>
        private ISession selectedSession;

        /// <summary>
        /// Backing field for the <see cref="IsComboBoxSessionsEnable"/> property.
        /// </summary>
        private bool isComboBoxSessionsEnable;

        /// <summary>
        /// Backing field for the <see cref="CanClearOutput"/> property.
        /// </summary>
        private bool canClearOutput;

        /// <summary>
        /// Backing field for the <see cref="IsScriptExecuted"/>.
        /// </summary>
        private bool isScriptExecuted;

        /// <summary>
        /// Backing field for the <see cref="CanExecuteScript"/>.
        /// </summary>
        private bool canExecuteScript;

        /// <summary>
        /// The thread that executes a script.
        /// </summary>
        private Thread scriptThread;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> used to stop a script during its execution.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// A <see cref="ScriptingProxy"/> object to perform the actions associated to the scripting commands entered by the user.
        /// </summary>
        protected readonly IScriptingProxy ScriptingProxy;

        /// <summary>
        /// The <see cref="CancellationToken"/> generated by the cancellationTokenSource to stop the execution of a script executed.
        /// </summary>
        protected CancellationToken CancellationToken;

        /// <summary>
        /// Backing field for the <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Gets or sets the script engine.
        /// </summary>
        /// <remarks>
        /// Currently the Script engine is only used to execute Python scripts.
        /// </remarks>
        protected Microsoft.Scripting.Hosting.ScriptEngine Engine { get; set; }

        /// <summary>
        /// Gets or sets the ScriptRuntime engine.
        /// </summary>
        /// <remarks>
        /// Currently the ScriptRuntime is only used to execute Python scripts.
        /// </remarks>
        protected Microsoft.Scripting.Hosting.ScriptRuntime Runtime { get; set; }

        /// <summary>
        /// The constructor of the <see cref="ScriptPanelViewModel"/> class.
        /// </summary>
        /// <param name="panelTitle">The title of the panel associated to this view model.</param>
        /// <param name="scriptingProxy">A <see cref="IScriptingProxy"/> object to perform the script commands associated to CDP4.</param>
        /// <param name="messageBus">The <see cref="ICDPMessageBus"/></param>
        /// <param name="fileExtension">The file extension associated to the tab item view-model.</param>
        /// <param name="openSessions">The list of the open <see cref="ISession"/>.</param>
        /// <param name="areTerminalsExistByDefault">Indicates whether the input and output terminals exist by default</param>
        protected ScriptPanelViewModel(string panelTitle, IScriptingProxy scriptingProxy, ICDPMessageBus messageBus, string fileExtension, ReactiveList<ISession> openSessions, bool areTerminalsExistByDefault)
        {
            this.Caption = panelTitle;
            this.ScriptingProxy = scriptingProxy;
            this.OpenSessions = openSessions;
            this.FileExtension = fileExtension;
            this.messageBus = messageBus;

            this.InitAvalonEditor();

            this.Identifier = Guid.NewGuid();

            this.ScriptVariables = new List<KeyValuePair<string, dynamic>>();

            this.SaveScriptCommand = ReactiveCommandCreator.Create(this.SaveScript, this.WhenAnyValue(x => x.IsDirty));

            this.ExecuteScriptCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteScript, this.WhenAnyValue(x => x.CanExecuteScript));

            this.StopScriptCommand = ReactiveCommandCreator.Create(this.StopScript, this.WhenAnyValue(x => x.IsScriptExecuted));

            this.ClearOutputCommand = ReactiveCommandCreator.Create(this.ClearOutput, this.WhenAnyValue(x => x.CanClearOutput));

            this.WhenAnyValue(vm => vm.IsShowWhitespacesButtonVisible)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    if (!x)
                    {
                        this.ShowWhitespaces = false;
                    }
                });

            this.WhenAnyValue(vm => vm.ShowWhitespaces)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ReactOnShowWhiteSpacesChange());
            
            this.WhenAnyValue(vm => vm.OpenSessions.Count)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ReactOnOpenSessionsChange());

            this.WhenAnyValue(vm => vm.IsDirty)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ReactOnIsDirtyChange());

            this.WhenAnyValue(vm => vm.IsScriptExecuted)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.ReactOnIsScriptExecutedChange());

            this.cancellationTokenSource = new CancellationTokenSource();
            this.CancellationToken = this.cancellationTokenSource.Token;

            if (areTerminalsExistByDefault)
            {
                this.InputTerminal = new InputTerminal(this);
                this.OutputTerminal = new OutputTerminal();
                this.OutputTerminal.TextChanged += (s, e) => this.ReactOnOutputContentChange();
                this.AreTerminalsVisible = true;
                this.ReactOnOutputContentChange();
            }
        }

        /// <summary>
        /// Gets or sets the File extension.
        /// </summary>
        public string FileExtension { get; private set; }

        /// <summary>
        /// Gets and sets the AvalonEditor.
        /// </summary>
        public virtual TextEditor AvalonEditor { get; set; }

        /// <summary>
        /// Gets or sets the input terminal.
        /// </summary>
        public InputTerminal InputTerminal { get; private set; }

        /// <summary>
        /// Gets or sets the output terminal.
        /// </summary>
        public OutputTerminal OutputTerminal { get; private set; }

        /// <summary>
        /// Gets or sets the SearchPanel which allows a user to perform searches in the content of the Avalon editor.
        /// </summary>
        public SearchPanel SearchPanel { get; private set; }

        /// <summary>
        /// Gets or sets the CompletionWindow
        /// </summary>
        public CompletionWindow CompletionWindow { get; private set; }

        /// <summary>
        /// The variables of the scope of the script to display in the local variables panel.
        /// </summary>
        public IList<KeyValuePair<string, dynamic>> ScriptVariables
        {
            get => this.scriptVariables;
            protected set => this.RaiseAndSetIfChanged(ref this.scriptVariables, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the run button.
        /// </summary>
        public bool IsRunButtonVisible
        {
            get => this.isRunButtonVisible;
            set => this.RaiseAndSetIfChanged(ref this.isRunButtonVisible, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the combobox which allow a user to select a <see cref="ISession"/>.
        /// </summary>
        public bool IsSelectSessionVisible
        {
            get => this.isSelectSessionVisible;
            set => this.RaiseAndSetIfChanged(ref this.isSelectSessionVisible, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the clear output button.
        /// </summary>
        public bool IsClearOutputButtonVisible
        {
            get => this.isClearOutputButtonVisible;
            set => this.RaiseAndSetIfChanged(ref this.isClearOutputButtonVisible, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the show whitespaces button.
        /// </summary>
        public bool IsShowWhitespacesButtonVisible
        {
            get => this.isShowWhitespacesButtonVisible;
            set => this.RaiseAndSetIfChanged(ref this.isShowWhitespacesButtonVisible, value);
        }

        /// <summary>
        /// Gets or sets state of the show whitespaces button.
        /// </summary>
        public bool ShowWhitespaces
        {
            get => this.showWhitespaces;
            set => this.RaiseAndSetIfChanged(ref this.showWhitespaces, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the stop button.
        /// </summary>
        public bool IsStopScriptButtonVisible
        {
            get => this.isStopScriptButtonVisible;
            set => this.RaiseAndSetIfChanged(ref this.isStopScriptButtonVisible, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the terminal panels.
        /// </summary>
        public bool AreTerminalsVisible
        {
            get => this.areTerminalsVisible;
            set => this.RaiseAndSetIfChanged(ref this.areTerminalsVisible, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the stop button.
        /// </summary>
        public bool IsScriptVariablesPanelVisible
        {
            get => this.isLocalVariablePanelVisible;
            set => this.RaiseAndSetIfChanged(ref this.isLocalVariablePanelVisible, value);
        }

        /// <summary>
        /// Gets or sets the caption of the label.
        /// </summary>
        /// <remarks>
        /// The caption is displayed as a header.
        /// </remarks>
        public virtual string Caption
        {
            get => this.caption;
            set => this.RaiseAndSetIfChanged(ref this.caption, value);
        }

        /// <summary>
        /// Gets the tooltip
        /// </summary>
        public string ToolTip => "Display a script panel";

        /// <summary>
        /// Gets the data-source
        /// </summary>
        public string DataSource => null;

        /// <summary>
        /// Gets the identifier of this panel.
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Disposes the IPanelView
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content of the Panel is dirty or not.
        /// </summary>
        public bool IsDirty
        {
            get => this.isDirty;
            set => this.RaiseAndSetIfChanged(ref this.isDirty, value);
        }

        /// <summary>
        /// Gets a list of open <see cref="ISession"/>s
        /// </summary>
        public ReactiveList<ISession> OpenSessions { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ISession"/> selected by the user to execute the script.
        /// </summary>
        public ISession SelectedSession
        {
            get => this.selectedSession;
            set => this.RaiseAndSetIfChanged(ref this.selectedSession, value);
        }

        /// <summary>
        /// Gets or sets the state of the comboBox used to select the session.
        /// </summary>
        public bool IsComboBoxSessionsEnable
        {
            get => this.isComboBoxSessionsEnable;
            set => this.RaiseAndSetIfChanged(ref this.isComboBoxSessionsEnable, value);
        }

        /// <summary>
        /// Gets or sets the value indicating whether the output can be cleared.
        /// </summary>
        public bool CanClearOutput
        {
            get => this.canClearOutput;
            private set => this.RaiseAndSetIfChanged(ref this.canClearOutput, value);
        }

        /// <summary>
        /// Gets or sets the value indicating whether a script is actually executed.
        /// </summary>
        public bool IsScriptExecuted
        {
            get => this.isScriptExecuted;
            private set => this.RaiseAndSetIfChanged(ref this.isScriptExecuted, value);
        }

        /// <summary>
        /// Gets or sets the value indicating whether the script button can be clicked. If there is no script actually executed in this panel, the method return true;
        /// </summary>
        public bool CanExecuteScript
        {
            get => this.canExecuteScript;
            private set => this.RaiseAndSetIfChanged(ref this.canExecuteScript, value);
        }

        /// <summary>
        /// Saves the data that has been typed in the editor
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveScriptCommand { get; private set; }

        /// <summary>
        /// Gets the code from the texteditor, execute it and show the result
        /// </summary>
        public ReactiveCommand<Unit, Unit> ExecuteScriptCommand { get; private set; }

        /// <summary>
        /// Stops the script executed.
        /// </summary>
        public ReactiveCommand<Unit, Unit> StopScriptCommand { get; private set; }

        /// <summary>
        /// Clears the content of the output.
        /// </summary>
        public ReactiveCommand<Unit, Unit> ClearOutputCommand { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Gets or sets a value indicating if the <see cref="IPanelViewModel"/> is selected
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }

        /// <summary>
        /// Event handler when the text of <see cref="AvalonEditor"/> changes. <see cref="IsDirty"/> is set to true.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The arguments.</param>
        private void AvalonEditorOnTextChanged(object sender, EventArgs eventArgs)
        {
            this.IsDirty = true;
        }

        /// <summary>
        /// Initializes the text editor.
        /// </summary>
        private void InitAvalonEditor()
        {
            this.AvalonEditor = new TextEditor();
            this.AvalonEditor.ShowLineNumbers = true;
            this.AvalonEditor.Options.HighlightCurrentLine = true;
            this.AvalonEditor.TextArea.Opacity = 1.0;

            // Initializes the search panel of the text editor
            this.SearchPanel = SearchPanel.Install(this.AvalonEditor);
            var res = new ResourceDictionary();
            res.Source = new Uri(EditorSearchPanelPath);
            var style = res["AvalonEditorSearchPanelStyle"] as Style;
            this.SearchPanel.Style = style;

            // React when text is entering or entered
            this.AvalonEditor.TextChanged += this.AvalonEditorOnTextChanged;
            this.AvalonEditor.TextArea.TextEntering += this.TextAreaTextEntering;
            this.AvalonEditor.TextArea.TextEntered += this.TextAreaTextEntered;

            // Add context menu when user right click 
            this.AvalonEditor.ContextMenu = new ContextMenu();

            this.AvalonEditor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Undo",
                Command = ApplicationCommands.Undo
            });

            this.AvalonEditor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Redo",
                Command = ApplicationCommands.Redo
            });

            this.AvalonEditor.ContextMenu.Items.Add(new Separator());

            this.AvalonEditor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Cut",
                Command = ApplicationCommands.Cut
            });

            this.AvalonEditor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Copy",
                Command = ApplicationCommands.Copy
            });

            this.AvalonEditor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Paste",
                Command = ApplicationCommands.Paste
            });

            this.AvalonEditor.ContextMenu.Items.Add(new Separator());

            this.AvalonEditor.ContextMenu.Items.Add(new MenuItem
            {
                Header = "Select All",
                Command = ApplicationCommands.SelectAll
            });
        }

        /// <summary>
        /// Clears the variables of the scope of the script.
        /// </summary>
        public virtual void ClearScopeVariables()
        {
        }

        /// <summary>
        /// Executes the supplied script
        /// </summary>
        /// <param name="script">
        /// The string of the script that is to be executed
        /// </param>
        public virtual void Execute(string script)
        {
        }

        /// <summary>
        /// Enable / disable show whitespaces in script editor
        /// </summary>
        public void ReactOnShowWhiteSpacesChange()
        {
            this.AvalonEditor.Options.ShowSpaces = this.ShowWhitespaces;
            this.AvalonEditor.Options.ShowTabs = this.ShowWhitespaces;
        }

        /// <summary>
        /// Enable the comboBox used to select the session if at least one session is opened. If no session is opened the comboBox is disabled.
        /// </summary>
        public void ReactOnOpenSessionsChange()
        {
            if (this.OpenSessions.IsEmpty)
            {
                this.IsComboBoxSessionsEnable = false;
                return;
            }

            this.IsComboBoxSessionsEnable = true;
        }

        /// <summary>
        /// Reacts when <see cref="IsDirty"/> changes, to add or remove '*' at the end of the <see cref="Caption"/>.
        /// </summary>
        public void ReactOnIsDirtyChange()
        {
            if (this.IsDirty && !this.Caption.EndsWith("*"))
            {
                this.Caption += "*";
                return;
            }

            if (!this.IsDirty && this.Caption.EndsWith("*"))
            {
                this.Caption = this.Caption.Remove(this.Caption.Length - 1);
            }
        }

        /// <summary>
        /// Reacts when <see cref="IsScriptExecuted"/> changes to update the <see cref="CanExecuteScript"/> property. 
        /// </summary>
        public void ReactOnIsScriptExecutedChange()
        {
            this.CanExecuteScript = !this.IsScriptExecuted;
        }

        /// <summary>
        /// Reacts when the content of the <see cref="OutputTerminal"/> changes to set the <see cref="CanClearOutput"/> property and scroll down.
        /// </summary>
        public void ReactOnOutputContentChange()
        {
            this.OutputTerminal.ScrollToEnd();
            var outputContent = new TextRange(this.OutputTerminal.Document.ContentStart, this.OutputTerminal.Document.ContentEnd);

            if (outputContent.Text.Length == 0)
            {
                this.CanClearOutput = false;
                return;
            }

            this.CanClearOutput = true;
        }

        /// <summary>
        /// Loads a style sheet to enable the syntax highlighting in the script.
        /// </summary>
        /// <param name="sheetPath">The path of the embedded resource which contains the symbols to highlight.</param>
        public void LoadHighlightingSheet(string sheetPath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (var stream = assembly.GetManifestResourceStream(sheetPath))
            {
                if (stream == null)
                {
                    Logger.Trace("The syntax highlighting sheet {0} could not be loaded", sheetPath);
                    return;
                }

                using (var reader = new XmlTextReader(stream))
                {
                    this.AvalonEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        /// <summary>
        /// Executes the script of this panel in another thread.
        /// </summary>
        private async Task ExecuteScript()
        {
            var script = this.AvalonEditor.Text;
            this.IsScriptExecuted = true;

            try
            {
                await Task.Run(() =>
                {
                    this.CancellationToken.ThrowIfCancellationRequested();
                    this.scriptThread = Thread.CurrentThread;
                    this.Execute(script);
                }, this.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                Logger.Trace("The execution of the script has been cancelled.");
            }
            catch (Exception ex)
            {
                this.OutputTerminal.AppendText($"\nAn error occured during the execution of the script !\nError: {ex.Message}\n");
            }
            finally
            {
                this.IsScriptExecuted = false;
                this.cancellationTokenSource = new CancellationTokenSource();
                this.CancellationToken = this.cancellationTokenSource.Token;
                this.scriptThread = null;
            }
        }

        /// <summary>
        /// Stops the script that is being executed.
        /// </summary>
        private void StopScript()
        {
            // Asks for a cancellation of the token
            this.cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Send a message using the <see cref="CDPMessageBus"/> to tell that the content of the editor has to be saved.
        /// </summary>
        private void SaveScript()
        {
            var scriptSaved = new ScriptPanelEvent(this, ScriptPanelStatus.Saved);
            this.messageBus.SendMessage(scriptSaved);
        }

        /// <summary>
        /// Clears the content of the output.
        /// </summary>
        private void ClearOutput()
        {
            this.OutputTerminal.Document.Blocks.Clear();
        }

        /// <summary>
        /// Reacts when a text is entered in the text editor, and enable autocompletion if possible.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event that contains the text entered</param>
        public void TextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            try
            {
                // if text is a dot checks for properties to suggest. If not, returns.
                if (e.Text != ".")
                {
                    return;
                }

                // Initializes the completion window
                this.CompletionWindow = new CompletionWindow(this.AvalonEditor.TextArea);
                this.CompletionWindow.Width = 300;
                var data = this.CompletionWindow.CompletionList.CompletionData;

                var word = this.AvalonEditor.GetWordsBeforeDot();

                if (word == null)
                {
                    return;
                }

                // Suggestions for the proxy command object
                if (word == "Command")
                {
                    foreach (var completionData in this.ScriptingProxy.CommandCompletionData)
                    {
                        data.Add(completionData);
                    }

                    this.CompletionWindow.Show();
                    this.CompletionWindow.Closed += delegate { this.CompletionWindow = null; };
                }
                else
                {
                    var words = word.Split('.');

                    // Checks whether the string corresponds to a key of the script variables 
                    var scriptVariable = this.ScriptVariables.SingleOrDefault(x => x.Key == words[0]);

                    if (scriptVariable.Equals(default(KeyValuePair<string, dynamic>)))
                    {
                        return;
                    }

                    var scriptVariableType = scriptVariable.Value.GetType();
                    List<PropertyInfo> properties;

                    // Browse the expression entered to suggest properties of the last part (the one after the last dot) 
                    for (var i = 0; i < words.Length - 1; i++)
                    {
                        properties = new List<PropertyInfo>(scriptVariableType.GetProperties());

                        var property = properties.SingleOrDefault(x => x.Name == words[i + 1]);

                        if (property == null)
                        {
                            return;
                        }

                        scriptVariableType = property.PropertyType;
                    }

                    // Iterate the properties to display as suggestions
                    properties = new List<PropertyInfo>(scriptVariableType.GetProperties());

                    foreach (var property in properties)
                    {
                        data.Add(new EditorCompletionData(property.Name, ""));
                    }

                    // Iterate the methods to display
                    var methods = new List<MethodInfo>(scriptVariableType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                        .Where(x => !x.Name.StartsWith("get_") && !x.Name.StartsWith("set_"))
                        .ToList();

                    foreach (var method in methods)
                    {
                        data.Add(new EditorCompletionData(string.Format("{0}()", method.Name), ""));
                    }

                    this.CompletionWindow.Show();
                    this.CompletionWindow.Closed += delegate { this.CompletionWindow = null; };
                }
            }
            catch (Exception exception)
            {
                Logger.Trace(exception, "Autocompletion failed");
            }
        }

        /// <summary>
        /// Whenever a non-letter is typed while the completion window is open, insert the currently selected element.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event</param>
        public void TextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && this.CompletionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    this.CompletionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }
    }
}
