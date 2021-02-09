// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4SiteDirectory.Views;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="ModelBrowser" /> view
    /// </summary>
    [PanelViewModelExport("Model Browser", "The browser which displays the engineering models.")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ModelBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Engineering Models";

        /// <summary>
        /// Backing field for <see cref="CanCreateEngineeringModelSetup" />
        /// </summary>
        private bool canCreateEngineeringModelSetup;

        /// <summary>
        /// Backing field for <see cref="CanCreateIterationSetup" />
        /// </summary>
        private bool canCreateIterationSetup;

        /// <summary>
        /// Backing field for <see cref="CanCreateParticipant" />
        /// </summary>
        private bool canCreateParticipant;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBrowserViewModel" /> class.
        /// Used by MEF.
        /// </summary>
        public ModelBrowserViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelBrowserViewModel" /> class
        /// </summary>
        /// <param name="session">The <see cref="ISession" /> that the model browser is connected to</param>
        /// <param name="siteDir">The <see cref="SiteDirectory" /> containing the data</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService" /></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService" /></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService" /></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService" /> used to read and write plugin setting files.
        /// </param>
        public ModelBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";
        }

        /// <summary>
        /// Gets the <see cref="EngineeringModelSetupRowViewModel" />
        /// </summary>
        public DisposableReactiveList<EngineeringModelSetupRowViewModel> ModelSetup { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand" /> to create a new <see cref="Participant" />
        /// </summary>
        public ReactiveCommand<object> CreateParticipantCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand" /> to create a new <see cref="IterationSetup" />
        /// </summary>
        public ReactiveCommand<object> CreateIterationSetupCommand { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the current user can create a <see cref="IterationSetup" />
        /// </summary>
        public bool CanCreateIterationSetup
        {
            get { return this.canCreateIterationSetup; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateIterationSetup, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the current user can create a <see cref="Participant" />
        /// </summary>
        public bool CanCreateParticipant
        {
            get { return this.canCreateParticipant; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateParticipant, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the current user can create a <see cref="EngineeringModelSetup" />
        /// </summary>
        public bool CanCreateEngineeringModelSetup
        {
            get { return this.canCreateEngineeringModelSetup; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateEngineeringModelSetup, value); }
        }

        /// <summary>
        /// Initialize the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.ModelSetup = new DisposableReactiveList<EngineeringModelSetupRowViewModel>();
            this.UpdateModels();
        }

        /// <summary>
        /// Initialize the <see cref="ICommand" />s of the current view-model
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.ComputeStaticPermission();
            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateEngineeringModelSetup));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<EngineeringModelSetup>(this.Thing));

            this.CreateParticipantCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateParticipant));

            this.CreateParticipantCommand.Subscribe(
                _ =>
                {
                    var container = this.GetSelectedModelSetupContainer();

                    if (container == null)
                    {
                        return;
                    }

                    this.ExecuteCreateCommand<Participant>(container);
                });

            this.CreateIterationSetupCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateIterationSetup));

            this.CreateIterationSetupCommand.Subscribe(
                _ =>
                {
                    var container = this.GetSelectedModelSetupContainer();

                    if (container == null)
                    {
                        return;
                    }

                    this.ExecuteCreateCommand<IterationSetup>(container);
                });
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent" /> handler that is called upon the reception of a update message of the
        /// <see cref="SiteDirectory" /> represented
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateModels();
        }

        /// <summary>
        /// Populate the context menu depending on what is selected
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing == null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Engineering Model Setup", "", this.CreateCommand, MenuItemKind.Create, ClassKind.EngineeringModelSetup));
                return;
            }

            if (this.SelectedThing is EngineeringModelSetupRowViewModel)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Engineering Model Setup", "", this.CreateCommand, MenuItemKind.Create, ClassKind.EngineeringModelSetup));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Participant", "", this.CreateParticipantCommand, MenuItemKind.Create, ClassKind.Participant));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Iteration Setup", "", this.CreateIterationSetupCommand, MenuItemKind.Create, ClassKind.IterationSetup));
            }
            else if (this.SelectedThing is ModelParticipantRowViewModel)
            {
                var participantRowViewModel = (ModelParticipantRowViewModel)this.SelectedThing;
                var engineeringModelSetup = (EngineeringModelSetup)participantRowViewModel.Thing.Container;

                if (engineeringModelSetup.Participant.Count == 1)
                {
                    var deleteContextMenu = this.ContextMenu.SingleOrDefault(x => x.Header == "Delete this Participant");

                    if (deleteContextMenu != null)
                    {
                        this.ContextMenu.Remove(deleteContextMenu);
                    }
                }

                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Participant", "", this.CreateParticipantCommand, MenuItemKind.Create, ClassKind.Participant));
            }
            else if (this.SelectedThing is IterationSetupRowViewModel)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Iteration Setup", "", this.CreateIterationSetupCommand, MenuItemKind.Create, ClassKind.IterationSetup));
            }
            else
            {
                var folderRowViewModel = this.SelectedThing as FolderRowViewModel;

                if (folderRowViewModel != null)
                {
                    if (folderRowViewModel.ShortName == "Iterations")
                    {
                        this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Iteration Setup", "", this.CreateIterationSetupCommand, MenuItemKind.Create, ClassKind.IterationSetup));
                    }
                    else if (folderRowViewModel.ShortName == "Participants")
                    {
                        this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Participant", "", this.CreateParticipantCommand, MenuItemKind.Create, ClassKind.Participant));
                    }
                }
            }
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            if (this.SelectedThing == null)
            {
                this.CanCreateIterationSetup = false;
                this.CanCreateParticipant = false;
                return;
            }

            var folderRow = this.SelectedThing as FolderRowViewModel;

            if (folderRow == null)
            {
                var modelSetup = this.SelectedThing.Thing as EngineeringModelSetup ?? this.SelectedThing.Thing.GetContainerOfType<EngineeringModelSetup>();

                this.CanCreateIterationSetup = modelSetup != null
                                               && modelSetup.StudyPhase != StudyPhaseKind.PREPARATION_PHASE
                                               && this.PermissionService.CanWrite(ClassKind.IterationSetup, modelSetup);

                this.canCreateParticipant = modelSetup != null &&
                                            this.PermissionService.CanWrite(ClassKind.Participant, modelSetup);
            }
            else
            {
                var modelSetup = folderRow.ContainerViewModel.Thing as EngineeringModelSetup;

                this.CanCreateIterationSetup = modelSetup != null
                                               && modelSetup.StudyPhase != StudyPhaseKind.PREPARATION_PHASE
                                               && this.PermissionService.CanWrite(ClassKind.IterationSetup, modelSetup);

                this.canCreateParticipant = modelSetup != null &&
                                            this.PermissionService.CanWrite(ClassKind.Participant, modelSetup);
            }
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

            foreach (var model in this.ModelSetup)
            {
                model.Dispose();
            }
        }

        /// <summary>
        /// Update the list of <see cref="EngineeringModelSetupRowViewModel" />
        /// </summary>
        private void UpdateModels()
        {
            var newmodels = this.Thing.Model.Except(this.ModelSetup.Select(x => x.Thing)).ToList();
            var oldmodels = this.ModelSetup.Select(x => x.Thing).Except(this.Thing.Model).ToList();

            foreach (var engineeringModelSetup in newmodels.OrderBy(m => m.Name))
            {
                var row = new EngineeringModelSetupRowViewModel(engineeringModelSetup, this.Session, this);
                this.ModelSetup.Add(row);
            }

            foreach (var engineeringModelSetup in oldmodels)
            {
                var row = this.ModelSetup.SingleOrDefault(x => x.Thing == engineeringModelSetup);

                if (row == null)
                {
                    continue;
                }

                this.ModelSetup.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Gets the <see cref="EngineeringModelSetup" /> container for the selected row
        /// </summary>
        /// <returns>The <see cref="EngineeringModelSetup" /></returns>
        private EngineeringModelSetup GetSelectedModelSetupContainer()
        {
            var folderRow = this.SelectedThing as FolderRowViewModel;

            if (folderRow != null)
            {
                return (EngineeringModelSetup)this.SelectedThing.ContainerViewModel.Thing;
            }

            if (this.SelectedThing != null)
            {
                return this.SelectedThing.Thing as EngineeringModelSetup ?? this.SelectedThing.Thing.GetContainerOfType<EngineeringModelSetup>();
            }

            return null;
        }

        /// <summary>
        /// Compute the static permissions (only user dependent)
        /// </summary>
        private void ComputeStaticPermission()
        {
            this.CanCreateEngineeringModelSetup = this.PermissionService.CanWrite(ClassKind.EngineeringModelSetup, this.Thing);
        }
    }
}
