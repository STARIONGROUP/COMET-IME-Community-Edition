﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationBrowserViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.Comparers;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="PublicationBrowser" /> view
    /// </summary>
    public class PublicationBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Publications";

        /// <summary>
        /// The <see cref="PublicationChildRowComparer" /> used to compare the sort-order of rows
        /// </summary>
        private static readonly PublicationChildRowComparer rowComparer = new PublicationChildRowComparer();

        /// <summary>
        /// Backing field for <see cref="CanCreatePublication" />
        /// </summary>
        private bool canCreatePublication;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration" />
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="CurrentModel" />
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="FilterString" />
        /// </summary>
        private string filterString;

        /// <summary>
        /// Backing field for <see cref="IsHidingEmptyDomains" />
        /// </summary>
        private bool isHidingEmptyDomains;

        /// <summary>
        /// Backing field for <see cref="SelectAll" />
        /// </summary>
        private bool? selectAll = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationBrowserViewModel" /> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration" /></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService" /></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService" /></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService" /></param>
        public PublicationBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);
            this.FilterString = string.Empty;

            this.AddSubscriptions();
            this.UpdatePublications();
            this.UpdateDomains();

            this.IsHidingEmptyDomains = true;

            this.UpdateProperties();
            this.ComputePermission();
        }

        /// <summary>
        /// Gets or sets the Publish Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> PublishCommand { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to highlight the parameters or overrides
        /// </summary>
        public ReactiveCommand<Unit, Unit> HighlightCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to highlight the elements
        /// </summary>
        public ReactiveCommand<Unit, Unit> HighlightElementCommand { get; private set; }

        /// <summary>
        /// Gets or sets the select all command
        /// </summary>
        public ReactiveCommand<bool, Unit> SelectAllCommand { get; private set; }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup" />
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup => this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>();

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
        }

        /// <summary>
        /// Gets or sets the filter string for the browser
        /// </summary>
        public string FilterString
        {
            get => this.filterString;
            set => this.RaiseAndSetIfChanged(ref this.filterString, value);
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get => this.currentIteration;
            private set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Gets a value indicating whether the create override command shall be enabled
        /// </summary>
        public bool CanCreatePublication
        {
            get => this.canCreatePublication;
            private set => this.RaiseAndSetIfChanged(ref this.canCreatePublication, value);
        }

        /// <summary>
        /// Gets a value indicating whether the browser should hide empty domain rows
        /// </summary>
        public bool IsHidingEmptyDomains
        {
            get => this.isHidingEmptyDomains;
            set => this.RaiseAndSetIfChanged(ref this.isHidingEmptyDomains, value);
        }

        /// <summary>
        /// Gets the rows representing <see cref="Publication" />s
        /// </summary>
        public DisposableReactiveList<PublicationRowViewModel> Publications { get; private set; }

        /// <summary>
        /// Gets the rows representing <see cref="DomainOfExpertise" />s
        /// </summary>
        public DisposableReactiveList<PublicationDomainOfExpertiseRowViewModel> Domains { get; private set; }

        /// <summary>
        /// Gets or sets the select all property
        /// </summary>
        public bool? SelectAll
        {
            get => this.selectAll;
            private set => this.RaiseAndSetIfChanged(ref this.selectAll, value);
        }

        /// <summary>
        /// Gets all parameter rows.
        /// </summary>
        public IEnumerable<PublicationParameterOrOverrideRowViewModel> Parameters
        {
            get { return this.Domains.SelectMany(d => d.ContainedRows).OfType<PublicationParameterOrOverrideRowViewModel>(); }
        }

        /// <summary>
        /// Gets or sets the PublicationRowCheckedCommand
        /// </summary>
        public ReactiveCommand<Unit, Unit> PublicationRowCheckedCommand { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Initializes the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Publications = new DisposableReactiveList<PublicationRowViewModel>();
            this.Domains = new DisposableReactiveList<PublicationDomainOfExpertiseRowViewModel>();
        }

        /// <summary>
        /// Initializes the <see cref="ICommand" />s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.PublishCommand = ReactiveCommandCreator.Create(this.ExecutePublishCommand);
            this.SelectAllCommand = ReactiveCommandCreator.Create<bool>(x => this.ExecuteSelectAllCommand(x));
            this.PublicationRowCheckedCommand = ReactiveCommandCreator.Create(this.ExecutePublicationRowCheckedCommand);
            this.HighlightCommand = ReactiveCommandCreator.Create(this.ExecuteHighlightCommand);
            this.HighlightElementCommand = ReactiveCommandCreator.Create(this.ExecuteHighlightElementCommand);
        }

        /// <summary>
        /// Executes the <see cref="HighlightElementCommand"/>.
        /// </summary>
        protected virtual void ExecuteHighlightElementCommand()
        {
            // clear all highlights
            this.CDPMessageBus.SendMessage(new CancelHighlightEvent());

            if (this.SelectedThing?.Thing == null || !(this.SelectedThing is PublicationParameterOrOverrideRowViewModel))
            {
                return;
            }

            var thing = this.SelectedThing.Thing.Container;

            // highlight the selected thing
            this.CDPMessageBus.SendMessage(new HighlightEvent(thing), thing);
            this.CDPMessageBus.SendMessage(new HighlightEvent(thing), null);
        }

        /// <summary>
        /// Executes the <see cref="HighlightCommand"/>.
        /// </summary>
        protected virtual void ExecuteHighlightCommand()
        {
            // clear all highlights
            this.CDPMessageBus.SendMessage(new CancelHighlightEvent());

            if (this.SelectedThing?.Thing == null)
            {
                return;
            }

            // highlight the selected thing
            this.CDPMessageBus.SendMessage(new HighlightEvent(this.SelectedThing.Thing), this.SelectedThing.Thing);
            this.CDPMessageBus.SendMessage(new HighlightEvent(this.SelectedThing.Thing), null);
        }

        /// <summary>
        /// Executes the toggling of visibility of empty domain rows
        /// </summary>
        private void ToggleEmptyDomains()
        {
            var filterstring = "[IsEmpty] = false";
            var booleanedFilterstring = $" AND {filterstring}";

            if (this.IsHidingEmptyDomains)
            {
                if (this.FilterString.Contains(filterstring))
                {
                    return;
                }

                var boolean = string.Empty;

                if (!string.IsNullOrEmpty(this.FilterString))
                {
                    this.FilterString += $"{booleanedFilterstring}";
                    return;
                }

                this.FilterString += $"{boolean}{filterstring}";
            }
            else
            {
                if (this.FilterString.Contains(filterstring))
                {
                    if (this.FilterString.Contains(booleanedFilterstring))
                    {
                        this.FilterString = this.FilterString.Replace(booleanedFilterstring, string.Empty);
                    }
                    else
                    {
                        this.FilterString = this.FilterString.Replace(filterstring, string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the select all status when a row checked status is changed
        /// </summary>
        private void ExecutePublicationRowCheckedCommand()
        {
            var totalCount = this.Domains.Count() + this.Parameters.Count();
            var toBePublishedCount = this.Domains.Count(d => d.ToBePublished) + this.Parameters.Count(p => p.ToBePublished);

            if (toBePublishedCount == totalCount)
            {
                this.SelectAll = true;
                return;
            }

            if (toBePublishedCount == 0)
            {
                this.SelectAll = false;
                return;
            }

            this.SelectAll = null;
        }

        /// <summary>
        /// Updates selection status of domain rows in response to select all toggle
        /// </summary>
        /// <param name="isChecked"></param>
        private void ExecuteSelectAllCommand(bool? isChecked)
        {
            this.SelectAll = isChecked;

            if (isChecked.HasValue)
            {
                foreach (var domain in this.Domains)
                {
                    domain.ToBePublished = isChecked.Value;
                }
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            this.Disposables.Add(this.WhenAnyValue(vm => vm.IsHidingEmptyDomains).Subscribe(_ => this.ToggleEmptyDomains()));

            var updatePublishableParameterListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterValueSetBase))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated
                                    && objectChange.ChangedThing.CacheKey.Iteration == this.Thing.Iid
                                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(x => x.ChangedThing.Container as ParameterOrOverrideBase)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdatePublishableParameter);

            this.Disposables.Add(updatePublishableParameterListener);

            var addParameterListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterOrOverrideBase))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Added
                                    && objectChange.ChangedThing.CacheKey.Iteration == this.Thing.Iid
                                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as ParameterOrOverrideBase)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdatePublishableParameter);

            this.Disposables.Add(addParameterListener);

            var updateParameterListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterOrOverrideBase))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated
                                    && objectChange.ChangedThing.CacheKey.Iteration == this.Thing.Iid
                                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as ParameterOrOverrideBase)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RelocateParameterRowViewModel);

            this.Disposables.Add(updateParameterListener);

            var removeParameterListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterOrOverrideBase))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Removed
                                    && objectChange.ChangedThing.CacheKey.Iteration == this.Thing.Iid
                                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as ParameterOrOverrideBase)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RemovePublishableParameterRowViewModel);

            this.Disposables.Add(removeParameterListener);

            var addPublicationListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Publication))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Added
                                    && objectChange.ChangedThing.CacheKey.Iteration == this.Thing.Iid
                                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as Publication)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.AddPublicationRowViewModel);

            this.Disposables.Add(addPublicationListener);

            var updatedEngineeringModelListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.UpdateDomains());

            this.Disposables.Add(updatedEngineeringModelListener);

            var updateDomainOfExpretiseListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(x => x.ChangedThing as DomainOfExpertise)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.UpdateDomainOfExpretiseRowViewModel);

            this.Disposables.Add(updateDomainOfExpretiseListener);

            var engineeringModelSetupSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated
                                    && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated
                                    && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber
                                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                .Where(
                    objectChange => objectChange.EventKind == EventKind.Updated
                                    && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);
        }

        /// <summary>
        /// Update the rows representing the publishable <see cref="ParameterOrOverrideBase" />
        /// </summary>
        /// <param name="parameter">The updated <see cref="ParameterOrOverrideBase" /></param>
        private void UpdatePublishableParameter(ParameterOrOverrideBase parameter)
        {
            if (parameter.CanBePublished)
            {
                this.AddPublishableParameterRowViewModel(parameter);
            }
            else
            {
                this.RemovePublishableParameterRowViewModel(parameter);
            }
        }

        /// <summary>
        /// Adds the publishable <see cref="PublicationParameterOrOverrideRowViewModel" /> row view model to the tree.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase" /> that this row will belong to.</param>
        private void AddPublishableParameterRowViewModel(ParameterOrOverrideBase parameter)
        {
            var domainRow = this.Domains.SingleOrDefault(vm => vm.Thing == parameter.Owner);

            if (domainRow == null)
            {
                return;
            }

            // if the row already exists then simply update it
            var parameterRow = domainRow.ContainedRows.FirstOrDefault(pr => pr.Thing == parameter);

            if (parameterRow != null)
            {
                ((PublicationParameterOrOverrideRowViewModel)parameterRow).SetProperties();
                return;
            }

            // create the row only if it is publishable
            parameterRow = new PublicationParameterOrOverrideRowViewModel(parameter, this.Session, domainRow);

            // if the domain row has a checkbox, select this one as well.
            ((PublicationParameterOrOverrideRowViewModel)parameterRow).ToBePublished = domainRow.ToBePublished;

            domainRow.ContainedRows.SortedInsert(parameterRow, rowComparer);
        }

        /// <summary>
        /// Removes a publishable parameter row view-model if the <see cref="ParameterOrOverrideBase" /> is no longer publishable.
        /// </summary>
        /// <param name="parameter">The parameter that is no longer publishable.</param>
        private void RemovePublishableParameterRowViewModel(ParameterOrOverrideBase parameter)
        {
            var domainRow = this.Domains.SingleOrDefault(vm => vm.Thing == parameter.Owner);

            if (domainRow == null)
            {
                return;
            }

            var parameterRow = domainRow.ContainedRows.FirstOrDefault(pr => pr.Thing == parameter);

            if (parameterRow != null)
            {
                domainRow.ContainedRows.RemoveAndDispose(parameterRow);
            }
        }

        /// <summary>
        /// Updates the <see cref="Domains" /> of the parameter in case the <see cref="ParameterOrOverrideBase.Owner" /> that
        /// contains Parameter changes.
        /// </summary>
        /// <param name="parameterOrOverrideBase">The <see cref="ParameterOrOverrideBase" /> to relocate or remove</param>
        private void RelocateParameterRowViewModel(ParameterOrOverrideBase parameterOrOverrideBase)
        {
            // In case the owner has changed check if there are other domains that contain that parameter.
            var oldOwners = this.Domains.Where(d => d.ContainedRows.Any(p => p.Thing == parameterOrOverrideBase) && d.Thing != parameterOrOverrideBase.Owner);

            foreach (var owner in oldOwners)
            {
                var row = owner.ContainedRows.FirstOrDefault(pr => pr.Thing == parameterOrOverrideBase);

                if (row != null)
                {
                    owner.ContainedRows.RemoveAndDispose(row);
                }
            }

            if (!parameterOrOverrideBase.CanBePublished)
            {
                return;
            }

            var domainRow = this.Domains.SingleOrDefault(vm => vm.Thing == parameterOrOverrideBase.Owner);

            if (domainRow == null)
            {
                return;
            }

            var parameterRow = domainRow.ContainedRows.FirstOrDefault(pr => pr.Thing == parameterOrOverrideBase);

            if (parameterRow == null)
            {
                parameterRow = new PublicationParameterOrOverrideRowViewModel(parameterOrOverrideBase, this.Session, domainRow);

                // if the domain row has a checkbox, select this one as well.
                ((PublicationParameterOrOverrideRowViewModel)parameterRow).ToBePublished = domainRow.ToBePublished;

                domainRow.ContainedRows.SortedInsert(parameterRow, rowComparer);
            }
        }

        /// <summary>
        /// Adds a <see cref="PublicationRowViewModel" />
        /// </summary>
        /// <param name="publication">The associated <see cref="Publication" /></param>
        private void AddPublicationRowViewModel(Publication publication)
        {
            if (this.Publications.Any(x => x.Thing == publication))
            {
                return;
            }

            var row = new PublicationRowViewModel(publication, this.Session, this);
            row.Index = this.Thing.Publication.IndexOf(publication);

            var listOfParams = new List<PublicationParameterOrOverrideRowViewModel>();

            foreach (var parameterOrOverrideBase in publication.PublishedParameter)
            {
                var parameterRow = new PublicationParameterOrOverrideRowViewModel(parameterOrOverrideBase, this.Session, row);

                // turn off the checkbox for this row
                parameterRow.IsCheckable = false;
                listOfParams.Add(parameterRow);
            }

            this.Publications.Add(row);
            row.ContainedRows.AddRange(listOfParams.OrderBy(r => r.Thing.ParameterType.Name));
        }

        /// <summary>
        /// Gets the list of <see cref="ParameterOrOverrideBase" /> to be published.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}" /> of to be published <see cref="ParameterOrOverrideBase" />s</returns>
        private IEnumerable<ParameterOrOverrideBase> GetListOfParametersOrOverridesToPublish()
        {
            return this.Parameters.Select(r => r.Thing).Where(p => p.ToBePublished);
        }

        /// <summary>
        /// Execute the publication.
        /// </summary>
        public async void ExecutePublishCommand()
        {
            // get the list of parameters or overrides to publish
            var parametersOrOverrides = this.GetListOfParametersOrOverridesToPublish().ToList();

            // there must be some parameters selected. An empty publication is not possible.
            if (parametersOrOverrides.Count == 0)
            {
                MessageBox.Show(
                    "Please select at least one Parameter or Parameter Override to be published.",
                    "Publication",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // fire off the publication
            var publication = new Publication(Guid.NewGuid(), null, null);
            var iteration = this.Thing.Clone(false);

            iteration.Publication.Add(publication);

            publication.Container = iteration;

            publication.PublishedParameter = parametersOrOverrides;

            this.IsBusy = true;

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var containerTransaction = new ThingTransaction(transactionContext, iteration);
            containerTransaction.CreateOrUpdate(publication);

            try
            {
                var operationContainer = containerTransaction.FinalizeTransaction();
                await this.Session.Write(operationContainer);

                // Unselecect the domain rows
                foreach (var domain in this.Domains)
                {
                    domain.ToBePublished = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Publication failed: {0}", ex.Message),
                    "Publication Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent" /> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent" /></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdatePublications();
            this.UpdateDomains();
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";
        }

        /// <summary>
        /// Update the <see cref="Publications" /> List
        /// </summary>
        private void UpdatePublications()
        {
            var newPublications = this.Thing.Publication.Except(this.Publications.Select(x => x.Thing)).ToList();
            var oldPublications = this.Publications.Select(x => x.Thing).Except(this.Thing.Publication).ToList();

            foreach (var publication in oldPublications)
            {
                var row = this.Publications.SingleOrDefault(x => x.Thing == publication);

                if (row != null)
                {
                    this.Publications.RemoveAndDispose(row);
                }
            }

            foreach (var publication in newPublications)
            {
                this.AddPublicationRowViewModel(publication);
            }

            this.Publications.Sort((o1, o2) => o2.PublicationDate.CompareTo(o1.PublicationDate));
        }

        /// <summary>
        /// Update the <see cref="Domains" /> List
        /// </summary>
        private void UpdateDomains()
        {
            var newDomains = this.CurrentEngineeringModelSetup.ActiveDomain.Except(this.Domains.Select(x => x.Thing)).ToList();
            var oldDomains = this.Domains.Select(x => x.Thing).Except(this.CurrentEngineeringModelSetup.ActiveDomain).ToList();

            foreach (var domain in oldDomains)
            {
                var row = this.Domains.SingleOrDefault(x => x.Thing == domain);

                if (row != null)
                {
                    this.Domains.RemoveAndDispose(row);
                }
            }

            foreach (var domain in newDomains)
            {
                this.AddDomainOfExpretiseRowViewModel(domain);
            }

            this.SortDomains();
        }

        /// <summary>
        /// Sort the <see cref="Domains" /> List
        /// </summary>
        private void SortDomains()
        {
            this.Domains.Sort(
                (o1, o2) =>
                {
                    var nameO1 = o1.Name ?? string.Empty;
                    var nameO2 = o2.Name ?? string.Empty;

                    if (string.IsNullOrEmpty(nameO1))
                    {
                        return -1;
                    }

                    return string.IsNullOrEmpty(nameO2) ? 1 : string.CompareOrdinal(nameO1, nameO2);
                });
        }

        /// <summary>
        /// Add a domain to the <see cref="Domains" /> List
        /// </summary>
        /// <param name="domain">Domain to be added</param>
        private void AddDomainOfExpretiseRowViewModel(DomainOfExpertise domain)
        {
            var row = new PublicationDomainOfExpertiseRowViewModel(domain, this.Session, this);

            var listOfParams = new List<PublicationParameterOrOverrideRowViewModel>();

            foreach (var parameter in domain.OwnedParametersThatCanBePublished(this.Thing))
            {
                var parameterRow = new PublicationParameterOrOverrideRowViewModel(parameter, this.Session, row);
                listOfParams.Add(parameterRow);
            }

            this.Domains.Add(row);
            row.ContainedRows.AddRange(listOfParams.OrderBy(r => r.Name));
        }

        /// <summary>
        /// Remove a domain to the <see cref="Domains" /> List
        /// </summary>
        /// <param name="domain">Domain to be removed</param>
        private void RemoveDomainOfExpretiseRowViewModel(DomainOfExpertise domain)
        {
            var domainRow = this.Domains.SingleOrDefault(vm => vm.Thing == domain);

            if (domainRow != null)
            {
                this.Domains.RemoveAndDispose(domainRow);
            }
        }

        /// <summary>
        /// Adds or removes a domain to the <see cref="Domains" /> list depending if it is or not deprecated
        /// </summary>
        /// <param name="domain">Domain to be updated</param>
        private void UpdateDomainOfExpretiseRowViewModel(DomainOfExpertise domain)
        {
            if (domain.IsDeprecated)
            {
                this.RemoveDomainOfExpretiseRowViewModel(domain);
            }
            else if (this.Domains.SingleOrDefault(vm => vm.Thing == domain) == null)
            {
                this.AddDomainOfExpretiseRowViewModel(domain);
                this.SortDomains();
            }
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreatePublication = this.PermissionService.CanWrite(ClassKind.Publication, this.Thing);
        }

        /// <summary>
        /// Populates the <see cref="PublicationBrowserViewModel.ContextMenu" />
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            this.ContextMenu.Clear();

            if (this.SelectedThing == null)
            {
                return;
            }

            if (this.SelectedThing.ContainedRows.Count > 0)
            {
                this.ContextMenu.Add(this.SelectedThing.IsExpanded ? new ContextMenuItemViewModel("Collapse Rows", "", this.CollpaseRowsCommand, MenuItemKind.None, ClassKind.NotThing) : new ContextMenuItemViewModel("Expand Rows", "", this.ExpandRowsCommand, MenuItemKind.None, ClassKind.NotThing));
            }

            if (this.SelectedThing is PublicationParameterOrOverrideRowViewModel)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Inspect", "CTRL+I", this.InspectCommand, MenuItemKind.Inspect));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Highlight", "", this.HighlightCommand, MenuItemKind.Highlight));
                this.ContextMenu.Add(new ContextMenuItemViewModel("Highlight Container Element", "", this.HighlightElementCommand, MenuItemKind.Highlight));
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

            foreach (var publication in this.Publications)
            {
                publication.Dispose();
            }

            foreach (var domain in this.Domains)
            {
                domain.Dispose();
            }
        }
    }
}
