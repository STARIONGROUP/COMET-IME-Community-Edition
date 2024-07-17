﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ModellingThingBrowserViewModelBase.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Linq;
    using System.Reactive;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    
    using CDP4Dal;
    using CDP4Dal.Operations;
    
    using ReactiveUI;

    /// <summary>
    /// The View-Model-base that shall be used by a view-model representing a Browser that displays modelling data
    /// </summary>
    public abstract class ModellingThingBrowserViewModelBase : BrowserViewModelBase<Iteration>
    {
        /// <summary>
        /// Backing field for <see cref="CanCreateChangeRequest"/>
        /// </summary>
        private bool canCreateChangeRequest;

        /// <summary>
        /// Backing field for <see cref="CanCreateRequestForWaiver"/>
        /// </summary>
        private bool canCreateRequestForWaiver;

        /// <summary>
        /// Backing field for <see cref="CanCreateRequestForDeviation"/>
        /// </summary>
        private bool canCreateRequestForDeviation;

        /// <summary>
        /// Backing field for <see cref="CanCreateReviewItemDiscrepancy"/>
        /// </summary>
        private bool canCreateReviewItemDiscrepancy;

        /// <summary>
        /// Backing field for <see cref="CanCreateEngineeringModelDataNote"/>
        /// </summary>
        private bool canCreateEngineeringModelDataNote;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModellingThingBrowserViewModelBase"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> that contains the data to browse</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        protected ModellingThingBrowserViewModelBase(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
        }

        /// <summary>
        /// The applied annotations menu group
        /// </summary>
        public ContextMenuItemViewModel AnnotationMenuGroup { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the a <see cref="ChangeRequest"/> can be created
        /// </summary>
        public bool CanCreateChangeRequest
        {
            get { return this.canCreateChangeRequest; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateChangeRequest, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the a <see cref="RequestForWaiver"/> can be created
        /// </summary>
        public bool CanCreateRequestForWaiver
        {
            get { return this.canCreateRequestForWaiver; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateRequestForWaiver, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the a <see cref="RequestForDeviation"/> can be created
        /// </summary>
        public bool CanCreateRequestForDeviation
        {
            get { return this.canCreateRequestForDeviation; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateRequestForDeviation, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="CreateReviewItemDiscrepancyCommand"/> can be executed
        /// </summary>
        public bool CanCreateReviewItemDiscrepancy
        {
            get { return this.canCreateReviewItemDiscrepancy; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateReviewItemDiscrepancy, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the current user can create a <see cref="EngineeringModelSetup"/>
        /// </summary>
        public bool CanCreateEngineeringModelDataNote
        {
            get { return this.canCreateEngineeringModelDataNote; }
            private set { this.RaiseAndSetIfChanged(ref this.canCreateEngineeringModelDataNote, value); }
        }

        /// <summary>
        /// Gets the Command to create a Change Request
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateChangeRequestCommand { get; private set; }

        /// <summary>
        /// Gets the Command to create a Change Request
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateRequestForWaiverCommand { get; private set; }

        /// <summary>
        /// Gets the Command to create a Change Request
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateRequestForDeviationCommand { get; private set; }

        /// <summary>
        /// Gets the Command to create a RID
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateReviewItemDiscrepancyCommand { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a new <see cref="EngineeringModelDataNote"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateEngineeringModelDataNoteCommand { get; private set; }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var currentParticipantAndDomain = this.Session.OpenIterations[this.Thing];

            this.CreateChangeRequestCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateModellingAnnotation(new ChangeRequest(), currentParticipantAndDomain.Item2, currentParticipantAndDomain.Item1), 
                this.WhenAnyValue(x => x.CanCreateChangeRequest));

            this.CreateReviewItemDiscrepancyCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateModellingAnnotation(new ReviewItemDiscrepancy(), currentParticipantAndDomain.Item2, currentParticipantAndDomain.Item1), 
                this.WhenAnyValue(x => x.CanCreateReviewItemDiscrepancy));

            this.CreateRequestForWaiverCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateModellingAnnotation(new RequestForWaiver(), currentParticipantAndDomain.Item2, currentParticipantAndDomain.Item1), 
                this.WhenAnyValue(x => x.CanCreateRequestForWaiver));

            this.CreateRequestForDeviationCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateModellingAnnotation(new RequestForDeviation(), currentParticipantAndDomain.Item2, currentParticipantAndDomain.Item1), 
                this.WhenAnyValue(x => x.CanCreateRequestForDeviation));

            this.CreateEngineeringModelDataNoteCommand = ReactiveCommandCreator.Create(
                () => this.ExecuteCreateEngineeringModelDataNote(new EngineeringModelDataNote(), currentParticipantAndDomain.Item2), 
                this.WhenAnyValue(x => x.canCreateEngineeringModelDataNote));
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            if (this.SelectedThing == null)
            {
                return;
            }

            this.CanCreateChangeRequest = this.PermissionService.CanWrite(ClassKind.ChangeRequest, this.Thing.TopContainer);
            this.CanCreateRequestForDeviation = this.PermissionService.CanWrite(ClassKind.RequestForDeviation, this.Thing.TopContainer);
            this.CanCreateRequestForWaiver = this.PermissionService.CanWrite(ClassKind.RequestForWaiver, this.Thing.TopContainer);
            this.CanCreateReviewItemDiscrepancy = this.PermissionService.CanWrite(ClassKind.ReviewItemDiscrepancy, this.Thing.TopContainer);
            this.CanCreateEngineeringModelDataNote = this.PermissionService.CanWrite(ClassKind.EngineeringModelDataNote, this.Thing.TopContainer);
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.AnnotationMenuGroup == null)
            {
                this.AnnotationMenuGroup = new ContextMenuItemViewModel("Check Annotations", "", null, MenuItemKind.Navigate, ClassKind.ModellingAnnotationItem);
            }

            this.ContextMenu.Add(this.AnnotationMenuGroup);
            this.PopulateAnnotationMenuItemGroup();
        }

        /// <summary>
        /// Open the floating annotation window
        /// </summary>
        protected abstract void ExecuteOpenAnnotationWindow(ModellingAnnotationItem annotation);

        /// <summary>
        /// Execute the creation of a <see cref="ModellingAnnotationItem"/>
        /// </summary>
        protected void ExecuteCreateEngineeringModelDataNote(EngineeringModelDataNote engineeringModelDataNote, Participant participant)
        {
            if (this.SelectedThing == null)
            {
                return;
            }
            
            engineeringModelDataNote.Author = participant;
            
            var annotatedThing = new ModellingThingReference(this.SelectedThing.Thing);
            engineeringModelDataNote.PrimaryAnnotatedThing = annotatedThing;
            engineeringModelDataNote.RelatedThing.Add(annotatedThing);
            
            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var model = this.Thing.TopContainer as EngineeringModel;
            if (model == null)
            {
                throw new InvalidOperationException("A EngineeringModelDataNote item can only be created in the context of a Engineering Model.");
            }

            var containerClone = model.Clone(false);
            var transaction = new ThingTransaction(transactionContext, containerClone);
            this.ThingDialogNavigationService.Navigate(engineeringModelDataNote, transaction, this.Session, true, ThingDialogKind.Create, this.ThingDialogNavigationService, containerClone);
        }

        /// <summary>
        /// Execute the creation of a <see cref="ModellingAnnotationItem"/>
        /// </summary>
        protected void ExecuteCreateModellingAnnotation(ModellingAnnotationItem annotation, Participant participant, DomainOfExpertise owner)
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            annotation.Owner = owner;
            annotation.Author = participant;
            annotation.Status = AnnotationStatusKind.OPEN;

            var annotatedThing = new ModellingThingReference(this.SelectedThing.Thing);
            annotation.RelatedThing.Add(annotatedThing);
            annotation.PrimaryAnnotatedThing = annotatedThing;

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var model = this.Thing.TopContainer as EngineeringModel;
            if (model == null)
            {
                throw new InvalidOperationException("A modelling annotation item can only be created in the context of a Engineering Model.");
            }

            var containerClone = model.Clone(false);
            var transaction = new ThingTransaction(transactionContext, containerClone);
            this.ThingDialogNavigationService.Navigate(annotation, transaction, this.Session, true, ThingDialogKind.Create, this.ThingDialogNavigationService, containerClone);
        }

        /// <summary>
        /// Populate the <see cref="AnnotationMenuGroup"/> property
        /// </summary>
        protected virtual void PopulateAnnotationMenuItemGroup()
        {
            this.AnnotationMenuGroup.SubMenu.Clear();
            if (this.SelectedThing == null)
            {
                return;
            }

            var model = (EngineeringModel)this.Thing.Container;
            var annotations = model.ModellingAnnotation.Where(x => x.RelatedThing.Any(rt => rt.ReferencedThing == this.SelectedThing.Thing));
            var menugroup = new ReactiveList<ContextMenuItemViewModel>();
            foreach (var modellingAnnotationItem in annotations)
            {
                var menuitem = new ContextMenuItemViewModel(modellingAnnotationItem.ShortName, "", x => this.ExecuteOpenAnnotationWindow((ModellingAnnotationItem)x), modellingAnnotationItem, true, MenuItemKind.Navigate);
                menugroup.Add(menuitem);
            }

            this.AnnotationMenuGroup.SubMenu.AddRange(menugroup.OrderBy(x => x.Header));
        }
    }
}