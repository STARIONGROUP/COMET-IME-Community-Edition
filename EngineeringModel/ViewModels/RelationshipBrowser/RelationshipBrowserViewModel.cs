// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    
    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="RelationshipBrowserViewModel"/> view
    /// </summary>
    public class RelationshipBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Row folder containing all te binary relationships
        /// </summary>
        private CDP4Composition.FolderRowViewModel binaryRelationshipsFolder;

        /// <summary>
        /// Row folder containing all te multi relationships
        /// </summary>
        private CDP4Composition.FolderRowViewModel multiRelationshipsFolder;

        /// <summary>
        /// Backing field for <see cref="CanCreateRelationship"/>
        /// </summary>
        private bool canCreateRelationship;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Relationships";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="session">The session</param>
        /// <param name="permissionService">the <see cref="IPermissionService"/></param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        public RelationshipBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";
            this.RelationshipCreator = new RelationshipCreatorMainViewModel(this.Session, this.Thing);

            this.UpdateBinaryRelationships();
            this.UpdateMultiRelationships();
            this.ComputeUserDependentPermission();
        }
        
        /// <summary>
        /// Gets the folder rows representing relationship types
        /// </summary>
        public DisposableReactiveList<CDP4Composition.FolderRowViewModel> RelationshipTypes { get; private set; }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup
        {
            get { return this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>(); }
        }

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get { return this.currentModel; }
            private set { this.RaiseAndSetIfChanged(ref this.currentModel, value); }
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get { return this.currentIteration; }
            private set { this.RaiseAndSetIfChanged(ref this.currentIteration, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the create <see cref="BinaryRelationship"/> command can be executed
        /// </summary>
        public bool CanCreateRelationship
        {
            get { return this.canCreateRelationship; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateRelationship, value); }
        }

        /// <summary>
        /// Gets the <see cref="RelationshipCreatorMainViewModel"/>
        /// </summary>
        public RelationshipCreatorMainViewModel RelationshipCreator { get; private set; }
        public string TargetName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Updates all the Binary relationships
        /// </summary>
        private void UpdateBinaryRelationships()
        {            
            var currentRelationship = this.binaryRelationshipsFolder.ContainedRows.Select(x => (Relationship)x.Thing).ToList();
            var updatedRelationship = this.Thing.Relationship.Where(x => x is BinaryRelationship).ToList();

            var newRelationship = updatedRelationship.Except(currentRelationship).ToList();
            var oldRelationship = currentRelationship.Except(updatedRelationship).ToList();


            foreach (var relationship in oldRelationship)
            {
                this.RemoveBinaryRelationshipRowViewModel((BinaryRelationship)relationship);
            }

            foreach (var relationship in newRelationship)
            {
                this.AddBinaryRelationshipRowViewModel((BinaryRelationship)relationship);
            }
        }

        /// <summary>
        /// Updates all the Multi relationships
        /// </summary>
        private void UpdateMultiRelationships()
        {
            var currentRelationships = this.multiRelationshipsFolder.ContainedRows.Select(x => (Relationship)x.Thing).ToList();
            var updatedRelationships = this.Thing.Relationship.Where(x => x is MultiRelationship).ToList();

            var newRelationships = updatedRelationships.Except(currentRelationships).ToList();
            var oldRelationships = currentRelationships.Except(updatedRelationships).ToList();

            foreach (var relationship in oldRelationships)
            {
                this.RemoveMultiRelationshipRowViewModel((MultiRelationship)relationship);
            }

            foreach (var relationship in newRelationships)
            {
                this.AddMultiRelationshipRowViewModel((MultiRelationship)relationship);
            }
        }

        /// <summary>
        /// Adds a <see cref="BinaryRelationship"/> row view model to the tree.
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/> that this row will belong to.</param>
        private void AddBinaryRelationshipRowViewModel(BinaryRelationship relationship)
        {
            var row = new BinaryRelationshipRowViewModel(relationship, this.Session, this);
            this.binaryRelationshipsFolder.ContainedRows.Add(row);
        }

        /// <summary>
        /// Adds a <see cref="MultiRelationship"/> row view model to the tree.
        /// </summary>
        /// <param name="relationship">The <see cref="MultiRelationship"/> that this row will belong to.</param>
        private void AddMultiRelationshipRowViewModel(MultiRelationship relationship)
        {
            var row = new MultiRelationshipRowViewModel(relationship, this.Session,  this);
            this.multiRelationshipsFolder.ContainedRows.Add(row);
        }

        /// <summary>
        /// Removes a binary relationship row view-model.
        /// </summary>
        /// <param name="relationship">The relationship that is removed.</param>
        private void RemoveBinaryRelationshipRowViewModel(BinaryRelationship relationship)
        {
            var row = this.binaryRelationshipsFolder.ContainedRows.FirstOrDefault(pr => pr.Thing == relationship);

            if (row != null)
            {
                this.binaryRelationshipsFolder.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Removes a multi relationship row view-model.
        /// </summary>
        /// <param name="relationship">The relationship that is removed.</param>
        private void RemoveMultiRelationshipRowViewModel(MultiRelationship relationship)
        {
            var row = this.multiRelationshipsFolder.ContainedRows.FirstOrDefault(pr => pr.Thing == relationship);

            if (row != null)
            {
                this.multiRelationshipsFolder.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Compute the user-dependent permissions
        /// </summary>
        private void ComputeUserDependentPermission()
        {
            this.CanCreateRelationship = this.PermissionService.CanWrite(ClassKind.BinaryRelationship, this.Thing);
        }
        
        /// <summary>
        /// Initializes the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";

            this.RelationshipTypes = new DisposableReactiveList<CDP4Composition.FolderRowViewModel>();
            this.binaryRelationshipsFolder = new CDP4Composition.FolderRowViewModel("Binary Relationships", "Binary Relationships", this.Session, this);
            this.multiRelationshipsFolder = new CDP4Composition.FolderRowViewModel("Multi Relationships", "Multi Relationships", this.Session, this);
            this.RelationshipTypes.Add(this.binaryRelationshipsFolder);
            this.RelationshipTypes.Add(this.multiRelationshipsFolder);
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
            this.binaryRelationshipsFolder.Dispose();
            this.multiRelationshipsFolder.Dispose();
            this.RelationshipCreator.Dispose();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler that is called upon the reception of a update message of the <see cref="SiteDirectory"/> represented
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            base.ObjectChangeEventHandler(objectChange);
            this.UpdateBinaryRelationships();
            this.UpdateMultiRelationships();
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Binary Relationship", "", this.CreateCommand, MenuItemKind.Create, ClassKind.BinaryRelationship));
        }

        /// <summary>
        /// Initializes the Commands that can be executed from this view model. The commands are initialized
        /// before the <see cref="PopulateContextMenu"/> is invoked
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRelationship));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<BinaryRelationship>(this.Thing));
        }
    }
}