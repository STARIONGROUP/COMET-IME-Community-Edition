// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptingEngineRibbonPageGroupViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
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
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Scripting.Events;
    using CDP4Scripting.Helpers;
    using CDP4Scripting.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The view-model of the <see cref="ScriptingEngineRibbonPageGroup"/>
    /// </summary>
    [Export(typeof(ScriptingEngineRibbonPageGroupViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ScriptingEngineRibbonPageGroupViewModel : ReactiveObject
    {
        /// <summary>
        /// A <see cref="IScriptingProxy"/> object which perfoms the commands called in the scripts.
        /// </summary>
        private readonly IScriptingProxy scriptingProxy;

        /// <summary>
        /// The initial path when a dialog is opened.
        /// </summary>
        private readonly string initialDialogPath;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private IOpenSaveFileDialogService fileDialogService;

        /// <summary>
        /// Backing field for the <see cref="CollectionScriptPanelViewModels"/>
        /// </summary>
        private ObservableCollection<IScriptPanelViewModel> collectionScriptPanelViewModels;

        /// <summary>
        /// The file filters to use when a dialog is opened.
        /// </summary>
        public const string DialogFilters = "Python Files (*.py)|*.py|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private ObservableAsPropertyHelper<bool> hasSession;

        /// <summary name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingEngineRibbonPageGroupViewModel"/> class
        /// </summary>
        /// <param name="panelNavigationService">
        /// The <see cref="IPanelNavigationService"/>
        /// </param>
        /// <param name="fileDialogService">
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </param>
        /// <param name="scriptingProxy">
        /// The <see cref="IScriptingProxy"/>
        /// </param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public ScriptingEngineRibbonPageGroupViewModel(IPanelNavigationService panelNavigationService, IOpenSaveFileDialogService fileDialogService, IScriptingProxy scriptingProxy, ICDPMessageBus messageBus)
        {
            this.PanelNavigationService = panelNavigationService ?? throw new ArgumentNullException(nameof(panelNavigationService));
            this.fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
            this.scriptingProxy = scriptingProxy ?? throw new ArgumentNullException(nameof(scriptingProxy));
            this.messageBus = messageBus;

            messageBus.Listen<NavigationPanelEvent>()
                .Where(x => x.ViewModel.ToString().Contains("ScriptPanelViewModel") && x.PanelStatus == PanelStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.HandleClosedPanel);

            this.OpenSessions = new ReactiveList<ISession>();
            this.OpenSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession, scheduler: RxApp.MainThreadScheduler);
            messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.initialDialogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "file.txt");
            this.PathScriptingFiles = new Dictionary<string, string>();
            this.CollectionScriptPanelViewModels = new ObservableCollection<IScriptPanelViewModel>();
            messageBus.Listen<ScriptPanelEvent>().Subscribe(this.ScriptPanelEventHandler);

            this.NewPythonScriptCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateNewScript("python", ScriptingLaguageKindSupported.Python));

            this.NewTextScriptCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateNewScript("text", ScriptingLaguageKindSupported.Text));

            this.OpenScriptCommand = ReactiveCommandCreator.Create(this.OpenScriptFile);

            this.SaveAllCommand = ReactiveCommandCreator.Create(this.SaveAllScripts);
        }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> used to navigate to Panels.
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets and sets the tabitems
        /// </summary>
        public ObservableCollection<IScriptPanelViewModel> CollectionScriptPanelViewModels
        {
            get => this.collectionScriptPanelViewModels;
            set => this.RaiseAndSetIfChanged(ref this.collectionScriptPanelViewModels, value);
        }

        /// <summary>
        /// Creates a new python tab.
        /// </summary>
        public ReactiveCommand<Unit, Unit> NewPythonScriptCommand { get; private set; }

        /// <summary>
        /// Creates a new text tab.
        /// </summary>
        public ReactiveCommand<Unit, Unit> NewTextScriptCommand { get; private set; }

        /// <summary>
        /// Shows a dialog window to select a python file and import it into the texteditor
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenScriptCommand { get; private set; }

        /// <summary>
        /// Saves all the scripts currently open.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveAllCommand { get; private set; }

        /// <summary>
        /// Gets or sets the list of the paths that correspond to the files open in the scripting engine.
        /// The key is the header of the tab item and the value is the path of the file associated to this tab item.
        /// </summary>
        public Dictionary<string, string> PathScriptingFiles { get; set; }

        /// <summary>
        /// Gets a list of open <see cref="ISession"/>s
        /// </summary>
        public ReactiveList<ISession> OpenSessions { get; private set; }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSession => this.hasSession.Value;

        /// <summary>
        /// Calls the <see cref="CreateNewScript"/> method and pass as title agrument, the language used and the number of <see cref="IScriptPanelViewModel"/> open.
        /// </summary>
        /// <param name="panelTitle">The title of the panel associated to this view model.</param>
        /// <param name="scriptingLanguage">The language of the new script.</param>
        private void ExecuteCreateNewScript(string panelTitle, ScriptingLaguageKindSupported scriptingLanguage)
        {
            var panelCounter = this.CollectionScriptPanelViewModels.Count - 1;
            var captionExists = true;

            while (captionExists)
            {
                panelCounter++;
                captionExists = this.CollectionScriptPanelViewModels.Any(x => x.Caption == panelTitle + panelCounter);
            }

            this.CreateNewScript(panelTitle + panelCounter, scriptingLanguage);
        }

        /// <summary>
        /// Creates a new panel which contains a script.
        /// </summary>
        /// <param name="panelTitle">The title of the panel associated to this view model.</param>
        /// <param name="scriptingLanguage">The language of the new script.</param>
        private IScriptPanelViewModel CreateNewScript(string panelTitle, ScriptingLaguageKindSupported scriptingLanguage)
        {
            IScriptPanelViewModel scriptPanelViewModel;

            switch (scriptingLanguage)
            {
                case ScriptingLaguageKindSupported.Python:
                    scriptPanelViewModel = new PythonScriptPanelViewModel(panelTitle, this.scriptingProxy, this.messageBus, this.OpenSessions);
                    break;
                    break;
                case ScriptingLaguageKindSupported.Text:
                    scriptPanelViewModel = new TextScriptPanelViewModel(panelTitle, this.scriptingProxy, this.messageBus, this.OpenSessions);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The {0} is not supported", scriptingLanguage));
            }

            this.PanelNavigationService.OpenInDock(scriptPanelViewModel as IPanelViewModel);
            this.CollectionScriptPanelViewModels.Add(scriptPanelViewModel);
            return scriptPanelViewModel;
        }

        /// <summary>
        /// Open a new panel with the selected script file.
        /// </summary>
        private void OpenScriptFile()
        {
            // Open the dialog to open a file
            var filePaths = this.fileDialogService.GetOpenFileDialog(false, false, true, DialogFilters, "*.*", this.initialDialogPath, 4);

            if (filePaths == null || !filePaths.Any())
            {
                return;
            }

            foreach (var filePath in filePaths)
            {
                // if the path is invalid got to the next one
                // if the file is already open, it cannot be open a second time 
                if (string.IsNullOrEmpty(filePath) || this.PathScriptingFiles.ContainsValue(filePath))
                {
                    continue;
                }

                var fileName = Path.GetFileName(filePath);
                var fileExtension = Path.GetExtension(filePath);

                // Check if the extension is supported and create the panel associated
                IScriptPanelViewModel scriptPanelViewModel;

                switch (fileExtension)
                {
                    case ".py":
                    {
                        scriptPanelViewModel = this.CreateNewScript(fileName, ScriptingLaguageKindSupported.Python);
                        break;
                    }
                    case ".txt":
                    {
                        scriptPanelViewModel = this.CreateNewScript(fileName, ScriptingLaguageKindSupported.Text);
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException(string.Format("The filextension ({0}) is not supported", fileExtension));
                    }
                }

                scriptPanelViewModel.AvalonEditor.Text = File.ReadAllText(filePath);
                scriptPanelViewModel.IsDirty = false;
                this.PathScriptingFiles.Add(scriptPanelViewModel.Caption, filePath);
            }
        }

        /// <summary>
        /// Save the script contained in <see cref="scriptPanelViewModel"/>. 
        /// The first time it will show a dialog to save the script and the next times it will overwrite the data.
        /// </summary>
        /// <param name="scriptPanelViewModel">The <see cref="IScriptPanelViewModel"/> that contains the script.</param>
        private void SaveScript(IScriptPanelViewModel scriptPanelViewModel)
        {
            var contentScript = scriptPanelViewModel.AvalonEditor.Text;

            string header;

            // Check if the content of the Panel is dirty or not to not include the * in the name of the file saved 
            if (scriptPanelViewModel.Caption.EndsWith("*"))
            {
                header = scriptPanelViewModel.Caption.Remove(scriptPanelViewModel.Caption.Length - 1);
            }
            else
            {
                header = scriptPanelViewModel.Caption;
            }

            string filePath;

            if (this.PathScriptingFiles.TryGetValue(header, out filePath) && File.Exists(filePath))
            {
                File.WriteAllText(filePath, contentScript);
                scriptPanelViewModel.IsDirty = false;
                return;
            }

            // Open the dialog to save the file
            var extension = scriptPanelViewModel.FileExtension;
            var filterIndex = this.FindFilterIndex(extension);
            filePath = this.fileDialogService.GetSaveFileDialog(header, extension, DialogFilters, this.initialDialogPath, filterIndex);
            var fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(string.Format("An error occured with the path or the name of the file, the script has not been saved. " +
                                                              "Please verify that the path '{0}' amd the name '{1}' are not empty.", filePath, fileName));
            }

            File.WriteAllText(filePath, contentScript);

            // Update the dictionnary which contains the paths of the open tabs 
            if (this.PathScriptingFiles.ContainsKey(header))
            {
                this.PathScriptingFiles.Remove(header);
            }

            scriptPanelViewModel.Caption = fileName;
            this.PathScriptingFiles.Add(fileName, filePath);
            scriptPanelViewModel.IsDirty = false;
        }

        /// <summary>
        /// Saves all the scripts currently open.
        /// </summary>
        private void SaveAllScripts()
        {
            foreach (var scriptPanelViewModel in this.CollectionScriptPanelViewModels)
            {
                this.SaveScript(scriptPanelViewModel);
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Session"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (sessionChange.Status == SessionStatus.Open)
            {
                this.OpenSessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.OpenSessions.Remove(sessionChange.Session);
            }
        }

        /// <summary>
        /// Removes a <see cref="IScriptPanelViewModel"/> from the <see cref="CollectionScriptPanelViewModels"/> and its information stored in the <see cref="PathScriptingFiles"/>.
        /// </summary>
        /// <param name="navigationPanelEvent"> The payload of the event that is being handled.</param>
        private void HandleClosedPanel(NavigationPanelEvent navigationPanelEvent)
        {
            var scriptPanelViewModel = (IScriptPanelViewModel)navigationPanelEvent.ViewModel;
            var header = scriptPanelViewModel.Caption;

            if (this.PathScriptingFiles.ContainsKey(header))
            {
                this.PathScriptingFiles.Remove(header);
            }

            this.CollectionScriptPanelViewModels.Remove(scriptPanelViewModel);
        }

        /// <summary>
        /// The event handler that listens for updates on the <see cref="IScriptPanelViewModel"/>. 
        /// </summary>
        /// <param name="scriptPanelEvent">The payload of the event that is being handled.</param>
        private void ScriptPanelEventHandler(ScriptPanelEvent scriptPanelEvent)
        {
            if (scriptPanelEvent.Status == ScriptPanelStatus.Saved)
            {
                this.SaveScript(scriptPanelEvent.ScriptPanelViewModel);
            }
        }

        /// <summary>
        /// Finds the filter index associated to the extension in the <see cref="DialogFilters"/>.
        /// </summary>
        /// <param name="extension">The extension for which we want the index.</param>
        /// <returns>The filter index.</returns>
        public int FindFilterIndex(string extension)
        {
            var filterIndex = 1;
            extension = string.Concat("(", extension, ")");
            var filters = DialogFilters.Split('|');

            for (var i = 0; i < filters.Length; i++)
            {
                if (filters[i].Contains(extension))
                {
                    filterIndex = i / 2 + 1;
                    return filterIndex;
                }
            }

            return filterIndex;
        }
    }
}
