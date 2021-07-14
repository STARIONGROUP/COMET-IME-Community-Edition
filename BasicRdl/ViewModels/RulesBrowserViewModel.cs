// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RulesBrowserViewModel.cs" company="RHEA System S.A.">
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

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="RulesBrowserViewModel"/> is to represent the view-model for <see cref="Rule"/>s
    /// </summary>
    public class RulesBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel, IDeprecatableBrowserViewModel
    {
        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Rules";

        /// <summary>
        /// Backing field for the <see cref="Rules"/> property.
        /// </summary>
        private readonly DisposableReactiveList<RuleRowViewModel> rules = new DisposableReactiveList<RuleRowViewModel>();

        /// <summary>
        /// Backing field for <see cref="CanCreateRdlElement"/>
        /// </summary>
        private bool canCreateRdlElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="RulesBrowserViewModel"/> class.
        /// </summary>
        /// <param name="session">the associated session</param>
        /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/> that is used to navigate to a panel</param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public RulesBrowserViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.rules.ChangeTrackingEnabled = true;

            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the <see cref="RuleRowViewModel"/> that are contained by this view-model
        /// </summary>
        public DisposableReactiveList<RuleRowViewModel> Rules => this.rules;

        /// <summary>
        /// Gets a value indicating whether a RDL element may be created
        /// </summary>
        public bool CanCreateRdlElement
        {
            get => this.canCreateRdlElement;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateRdlElement, value);
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="BinaryRelationshipRule"/>
        /// </summary>
        public ReactiveCommand<object> CreateBinaryRelationshipRule { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="DecompositionRule"/>
        /// </summary>
        public ReactiveCommand<object> CreateDecompositionRule { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="MultiRelationshipRule"/>
        /// </summary>
        public ReactiveCommand<object> CreateMultiRelationshipRule { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ParameterizedCategoryRule"/>
        /// </summary>
        public ReactiveCommand<object> CreateParameterizedCategoryRule { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ReferencerRule"/>
        /// </summary>
        public ReactiveCommand<object> CreateReferencerRule { get; private set; }
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Rule))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Rule)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddRuleRowViewModel);
            this.Disposables.Add(addListener);

            var removeListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Rule))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Rule)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveRuleRowViewModel);
            this.Disposables.Add(removeListener);

            var rdlUpdateListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ReferenceDataLibrary))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as ReferenceDataLibrary)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RefreshContainerName);
            this.Disposables.Add(rdlUpdateListener);
        }

        /// <summary>
        /// Adds a <see cref="RuleRowViewModel"/>
        /// </summary>
        /// <param name="rule">
        /// The associated <see cref="Rule"/> for which the row is to be added.
        /// </param>
        private void AddRuleRowViewModel(Rule rule)
        {
            var row = new RuleRowViewModel(rule, this.Session, this);
            this.Rules.Add(row);
        }

        /// <summary>
        /// Removes a <see cref="RuleRowViewModel"/> from the view model
        /// </summary>
        /// <param name="rule">
        /// The <see cref="Rule"/> for which the row view model has to be removed
        /// </param>
        private void RemoveRuleRowViewModel(Rule rule)
        {
            var row = this.Rules.SingleOrDefault(rowViewModel => rowViewModel.Thing == rule);

            if (row != null)
            {
                this.Rules.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Refresh the displayed container name for the category rows
        /// </summary>
        /// <param name="rdl">
        /// The updated <see cref="ReferenceDataLibrary"/>.
        /// </param>
        private void RefreshContainerName(ReferenceDataLibrary rdl)
        {
            foreach (var rule in this.Rules)
            {
                if (rule.Thing.Container != rdl)
                {
                    continue;
                }

                if (rule.ContainerRdl != rdl.ShortName)
                {
                    rule.ContainerRdl = rdl.ShortName;
                }
            }
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateRdlElement = this.Session.OpenReferenceDataLibraries.Any();
        }

        /// <summary>
        /// Populate the <see cref="ContextMenu"/> of the current browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create Binary Relationship Rule", "", this.CreateBinaryRelationshipRule, MenuItemKind.Create, ClassKind.BinaryRelationshipRule));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create Decomposition Rule", "", this.CreateDecompositionRule, MenuItemKind.Create, ClassKind.DecompositionRule));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create Multi Relationship Rule", "", this.CreateMultiRelationshipRule, MenuItemKind.Create, ClassKind.MultiRelationshipRule));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create Parameterized Category Rule", "", this.CreateParameterizedCategoryRule, MenuItemKind.Create, ClassKind.ParameterizedCategoryRule));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create Referencer Rule", "", this.CreateReferencerRule, MenuItemKind.Create, ClassKind.ReferencerRule));
        }

        /// <summary>
        /// Initializes the create <see cref="ReactiveCommand"/> that allow a user to create the different kinds of <see cref="Rule"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateBinaryRelationshipRule = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateBinaryRelationshipRule.Subscribe(_ => this.ExecuteCreateCommand<BinaryRelationshipRule>());

            this.CreateDecompositionRule = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateDecompositionRule.Subscribe(_ => this.ExecuteCreateCommand<DecompositionRule>());

            this.CreateMultiRelationshipRule = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateMultiRelationshipRule.Subscribe(_ => this.ExecuteCreateCommand<MultiRelationshipRule>());

            this.CreateParameterizedCategoryRule = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateParameterizedCategoryRule.Subscribe(_ => this.ExecuteCreateCommand<ParameterizedCategoryRule>());

            this.CreateReferencerRule = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanCreateRdlElement));
            this.CreateReferencerRule.Subscribe(_ => this.ExecuteCreateCommand<ReferencerRule>());
        }

        /// <summary>
        /// Loads the <see cref="Thing"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid);

            foreach (var referenceDataLibrary in this.Thing.AvailableReferenceDataLibraries().Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var rule in referenceDataLibrary.Rule)
                {
                    this.AddRuleRowViewModel(rule);
                }
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

            foreach (var rule in this.Rules)
            {
                rule.Dispose();
            }
        }
    }
}