// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4BuiltInRules.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows;

    using CDP4Common;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Errors.ViewModels;

    using ReactiveUI;

    /// <summary>
    /// The view-model that allows the user to browse the available <see cref="BuiltInRule"/>s
    /// </summary>
    public class ErrorBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Errors";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated <see cref="ISession"/></param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        public ErrorBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";
            this.Errors = new List<ErrorRowViewModel>();
            this.PopulateErrors();
        }
        
        /// <summary>
        /// Gets the list of <see cref="ErrorRowViewModel"/>.
        /// </summary>
        public List<ErrorRowViewModel> Errors { get; private set; }

        /// <summary>
        /// Gets or sets the Highlight Command
        /// </summary>
        public ReactiveCommand<object> HighlightCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Copy Command
        /// </summary>
        public ReactiveCommand<object> CopyErrorCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.RightGroup;

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            
            this.ContextMenu.Add(new ContextMenuItemViewModel("Highlight", "", this.HighlightCommand, MenuItemKind.Highlight));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Copy Error", "", this.CopyErrorCommand, MenuItemKind.Copy));
        }

        /// <summary>
        /// Initializes the Commands that can be executed from this view model. The commands are initialized
        /// before the <see cref="BrowserViewModelBase{T}.PopulateContextMenu"/> is invoked
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canSelectableCommandsExecute =
                this.WhenAnyValue(x => x.SelectedThing).Select(x => x != null && !(x.Thing is NotThing));

            this.HighlightCommand = ReactiveCommand.Create(canSelectableCommandsExecute);
            this.HighlightCommand.Subscribe(_ => this.ExecuteHighlightCommand());

            this.CopyErrorCommand = ReactiveCommand.Create(canSelectableCommandsExecute);
            this.CopyErrorCommand.Subscribe(_ => this.ExecuteCopyErrorCommand());
        }

        /// <summary>
        /// Handles the <see cref="SessionEvent"/> message
        /// </summary>
        /// <param name="sessionEvent">
        /// The <see cref="SessionEvent"/>
        /// </param>
        protected override void OnAssemblerUpdate(SessionEvent sessionEvent)
        {
            if (sessionEvent.Status == SessionStatus.EndUpdate)
            {
                this.PopulateErrors();
            }

            this.HasUpdateStarted = sessionEvent.Status == SessionStatus.BeginUpdate;
        }

        /// <summary>
        /// Execute the <see cref="BrowserViewModelBase{T}.RefreshCommand"/>
        /// </summary>
        protected override void ExecuteRefreshCommand()
        {
            this.PopulateErrors();
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var error in this.Errors)
            {
                error.Dispose();
            }
        }
        
        /// <summary>
        /// populates the <see cref="ErrorBrowser"/> with <see cref="ErrorRowViewModel"/>s.
        /// </summary>
        private void PopulateErrors()
        {
            foreach (var errorRowViewModel in this.Errors)
            {
                errorRowViewModel.Dispose();
            }

            this.Errors.Clear();

            foreach (var thing in this.Session.Assembler.Cache.Select(item => item.Value.Value).Where(t => t.ValidationErrors.Any()))
            {
                foreach (var error in thing.ValidationErrors)
                {
                    var row = new ErrorRowViewModel(thing, error, this.Session, this);
                    this.Errors.Add(row);
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="CopyErrorCommand"/>
        /// </summary>
        private void ExecuteCopyErrorCommand()
        {
            var selectedErrorRow = this.SelectedThing as ErrorRowViewModel;

            if (selectedErrorRow != null)
            {
                Clipboard.SetDataObject(string.Format("{1} of type {0} has an error: {2}", selectedErrorRow.ContainerThingClassKind, selectedErrorRow.Path, selectedErrorRow.Content));
            }
        }

        /// <summary>
        /// Executes the <see cref="HighlightCommand"/>
        /// </summary>
        private void ExecuteHighlightCommand()
        {
            // clear all highlights
            CDPMessageBus.Current.SendMessage(new CancelHighlightEvent());

            // highlight the selected thing
            CDPMessageBus.Current.SendMessage(new HighlightEvent(this.SelectedThing.Thing), this.SelectedThing.Thing);
            CDPMessageBus.Current.SendMessage(new HighlightEvent(this.SelectedThing.Thing), null);
        }
    }
}