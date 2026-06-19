// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipBrowserViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4EngineeringModel.ViewModels
{
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
    /// The abstract base view-model for the dedicated relationship browsers. It holds the rows representing the
    /// <typeparamref name="TRelationship"/> instances of an <see cref="Iteration"/> and the shared creator and command logic.
    /// </summary>
    /// <typeparam name="TRelationship">The type of <see cref="Relationship"/> that is displayed by the browser</typeparam>
    /// <typeparam name="TRow">The concrete type of row view-model used to represent a <typeparamref name="TRelationship"/></typeparam>
    public abstract class RelationshipBrowserViewModel<TRelationship, TRow> : BrowserViewModelBase<Iteration>, IPanelViewModel
        where TRelationship : Relationship, new()
        where TRow : class, IRowViewModelBase<Thing>
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
        /// Backing field for <see cref="CanCreateRelationship"/>
        /// </summary>
        private bool canCreateRelationship;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipBrowserViewModel{TRelationship,TRow}"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/></param>
        protected RelationshipBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{this.PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";
            this.RelationshipCreator = new RelationshipCreatorMainViewModel(this.Session, this.Thing);

            this.UpdateRelationships();
            this.ComputeUserDependentPermission();
        }

        /// <summary>
        /// Gets the rows representing the <typeparamref name="TRelationship"/> instances of the <see cref="Iteration"/>
        /// </summary>
        public DisposableReactiveList<TRow> Relationships { get; private set; }

        /// <summary>
        /// Gets the caption of the panel
        /// </summary>
        protected abstract string PanelCaption { get; }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <typeparamref name="TRelationship"/> used for permission and context-menu purposes
        /// </summary>
        protected abstract ClassKind RelationshipClassKind { get; }

        /// <summary>
        /// Gets the label of the create context-menu item
        /// </summary>
        protected abstract string CreateMenuItemLabel { get; }

        /// <summary>
        /// Creates the row view-model that represents the provided <paramref name="relationship"/>
        /// </summary>
        /// <param name="relationship">The <typeparamref name="TRelationship"/> that the row will represent</param>
        /// <returns>The <typeparamref name="TRow"/> representing the <paramref name="relationship"/></returns>
        protected abstract TRow CreateRow(TRelationship relationship);

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
        /// Gets a value indicating whether the create <typeparamref name="TRelationship"/> command can be executed
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

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Updates the rows representing the <typeparamref name="TRelationship"/> instances of the <see cref="Iteration"/>
        /// </summary>
        private void UpdateRelationships()
        {
            var currentRelationships = this.Relationships.Select(x => (TRelationship)x.Thing).ToList();
            var updatedRelationships = this.Thing.Relationship.OfType<TRelationship>().ToList();

            var newRelationships = updatedRelationships.Except(currentRelationships).ToList();
            var oldRelationships = currentRelationships.Except(updatedRelationships).ToList();

            foreach (var relationship in oldRelationships)
            {
                this.RemoveRelationshipRowViewModel(relationship);
            }

            foreach (var relationship in newRelationships)
            {
                this.Relationships.Add(this.CreateRow(relationship));
            }
        }

        /// <summary>
        /// Removes the row view-model that represents the provided <paramref name="relationship"/>
        /// </summary>
        /// <param name="relationship">The <typeparamref name="TRelationship"/> that is removed</param>
        private void RemoveRelationshipRowViewModel(TRelationship relationship)
        {
            var row = this.Relationships.FirstOrDefault(r => r.Thing == relationship);

            if (row != null)
            {
                this.Relationships.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Compute the user-dependent permissions
        /// </summary>
        private void ComputeUserDependentPermission()
        {
            this.CanCreateRelationship = this.PermissionService.CanWrite(this.RelationshipClassKind, this.Thing);
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

            this.Relationships = new DisposableReactiveList<TRow>();
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

            foreach (var relationship in this.Relationships)
            {
                relationship.Dispose();
            }

            this.RelationshipCreator.Dispose();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler that is called upon the reception of a update message of the <see cref="SiteDirectory"/> represented
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            this.Caption = $"{this.PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            base.ObjectChangeEventHandler(objectChange);
            this.UpdateRelationships();
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel(this.CreateMenuItemLabel, "", this.CreateCommand, MenuItemKind.Create, this.RelationshipClassKind));
        }

        /// <summary>
        /// Initializes the Commands that can be executed from this view model. The commands are initialized
        /// before the <see cref="PopulateContextMenu"/> is invoked
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<TRelationship>(this.Thing), this.WhenAnyValue(x => x.CanCreateRelationship));
        }
    }
}
