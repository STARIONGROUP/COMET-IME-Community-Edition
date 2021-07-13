// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipEditorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal.Operations;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Diagram;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Behaviours;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Controls;

    using DevExpress.Xpf.Diagram;

    using Helpers;

    using ReactiveUI;

    using IDiagramContainer = CDP4Composition.Diagram.IDiagramContainer;

    /// <summary>
    /// The view-model for the Relationship Editor that lets users edit Relationships between any 2 objects.
    /// </summary>
    public class RelationshipEditorViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel, IDropTarget, IDiagramContainer
    {
        /// <summary>
        /// Backing field for <see cref="ThingDiagramItems"/>
        /// </summary>
        private ReactiveList<object> thingDiagramItems;

        /// <summary>
        /// Backing field for <see cref="SelectedItems"/>
        /// </summary>
        private ReactiveList<DiagramItem> selectedItems;

        /// <summary>
        /// Backing field for <see cref="RelationshipRules"/>
        /// </summary>
        private DisposableReactiveList<RuleNavBarItemViewModel> relationshipRules;

        /// <summary>
        /// Backing field for <see cref="SelectedItem"/>
        /// </summary>
        private DiagramItem selectedItem;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="DomainOfExpertise"/>
        /// </summary>
        private string domainOfExpertise;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Relationship Editor";

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipEditorViewModel"/> class
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> to display
        /// </param>
        /// <param name="participant">The <see cref="Participant"/> that this open <see cref="Iteration"/> is tied to.</param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="thingDialogNavigationService"></param>
        /// <param name="panelNavigationService">The panel navigation service.</param>
        public RelationshipEditorViewModel(Iteration thing, Participant participant, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(thing, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel) this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.AddSubscriptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canCreateMultiRelationship =
                this.SelectedItems.Changed.Select(_ => this.CanCreateMultiRelationship());

            this.CreateBinaryRelationshipCommand = ReactiveCommand.Create();
            this.CreateBinaryRelationshipCommand.Subscribe(_ => this.CreateBinaryRelationshipCommandExecute());

            // creation of multi relationship requires selected nodes
            this.CreateMultiRelationshipCommand = ReactiveCommand.Create(canCreateMultiRelationship);
            this.CreateMultiRelationshipCommand.Subscribe(_ => this.CreateMultiRelationshipCommandExecute());
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var addBinaryListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(BinaryRelationshipRule))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Rule)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddRuleNavItemViewModel);

            this.Disposables.Add(addBinaryListener);

            var removeBinaryListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(BinaryRelationshipRule))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Rule)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveNavItemViewModel);

            this.Disposables.Add(removeBinaryListener);

            var addMultiListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(MultiRelationshipRule))
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Rule)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.AddRuleNavItemViewModel);

            this.Disposables.Add(addMultiListener);

            var removeMultiListener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(MultiRelationshipRule))
                    .Where(objectChange => objectChange.EventKind == EventKind.Removed && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .Select(x => x.ChangedThing as Rule)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.RemoveNavItemViewModel);

            this.Disposables.Add(removeMultiListener);
        }

        /// <summary>
        /// Loads the <see cref="Thing"/>s from the cache when the browser is instantiated.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this.RelationshipRules = new DisposableReactiveList<RuleNavBarItemViewModel> { ChangeTrackingEnabled = true };
            this.ThingDiagramItems = new ReactiveList<object> { ChangeTrackingEnabled = true };
            this.SelectedItems = new ReactiveList<DiagramItem> { ChangeTrackingEnabled = true };

            var openDataLibrariesIids = this.Session.OpenReferenceDataLibraries.Select(y => y.Iid);

            foreach (var referenceDataLibrary in this.Session.RetrieveSiteDirectory().AvailableReferenceDataLibraries().Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var rule in referenceDataLibrary.Rule)
                {
                    if (rule is BinaryRelationshipRule || rule is MultiRelationshipRule)
                    {
                        this.AddRuleNavItemViewModel(rule);
                    }
                }
            }

            this.WhenAnyValue(vm => vm.SelectedItem).Subscribe(_ =>
            {
                this.OnSelectedThingChanged();

                // TODO: Populate the context items, to be designed. phabricator T2251
                //this.PopulateItemContextMenu();
            });
        }

        /// <summary>
        /// Adds a <see cref="RuleNavBarItemViewModel"/>
        /// </summary>
        /// <param name="rule">
        /// The associated <see cref="Rule"/> for which the row is to be added.
        /// </param>
        private void AddRuleNavItemViewModel(Rule rule)
        {
            var row = new RuleNavBarItemViewModel(rule, this.Session, this);
            this.RelationshipRules.Add(row);

            this.ResortRuleNavBarItems();
        }

        /// <summary>
        /// Removes a <see cref="RuleNavBarItemViewModel"/> from the view model
        /// </summary>
        /// <param name="rule">
        /// The <see cref="Rule"/> for which the row view model has to be removed
        /// </param>
        private void RemoveNavItemViewModel(Rule rule)
        {
            var row = this.RelationshipRules.SingleOrDefault(rowViewModel => rowViewModel.Thing == rule);

            if (row != null)
            {
                this.RelationshipRules.RemoveAndDispose(row);
            }

            this.ResortRuleNavBarItems();
        }

        /// <summary>
        /// Resorts the nav bar items.
        /// </summary>
        private void ResortRuleNavBarItems()
        {
            this.RelationshipRules.Sort(new RuleNavBarItemComparerAscending());
        }

        /// <summary>
        /// Compute whether a <see cref="MultiRelationship"/> can be created.
        /// This is true only when two <see cref="NamedThingDiagramContentItem"/> with <see cref="Thing"/>s are selected.
        /// </summary>
        /// <returns>True when two <see cref="NamedThingDiagramContentItem"/> with <see cref="Thing"/>s are selected. </returns>
        private bool CanCreateMultiRelationship()
        {
            if (this.SelectedItems.Count <= 1)
            {
                return false;
            }

            return this.SelectedItems.All(x => x is NamedThingDiagramContentItem);
        }

        /// <summary>
        /// Gets or sets the Create Binary Relationship Command
        /// </summary>
        public ReactiveCommand<object> CreateBinaryRelationshipCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Create Multi Relationship Command
        /// </summary>
        public ReactiveCommand<object> CreateMultiRelationshipCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the active <see cref="Participant"/> of this editor.
        /// </summary>
        public Participant ActiveParticipant { get; set; }

        /// <summary>
        /// Gets or sets the collection of <see cref="DiagramItem"/> items to display.
        /// </summary>
        public ReactiveList<object> ThingDiagramItems
        {
            get { return this.thingDiagramItems; }
            set { this.RaiseAndSetIfChanged(ref this.thingDiagramItems, value); }
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="RuleNavBarItemViewModel"/> items to display.
        /// </summary>
        public DisposableReactiveList<RuleNavBarItemViewModel> RelationshipRules
        {
            get { return this.relationshipRules; }
            set { this.RaiseAndSetIfChanged(ref this.relationshipRules, value); }
        }

        /// <summary>
        /// Gets or sets the behaviour.
        /// </summary>
        public IExtendedDiagramOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DiagramItem"/> item that is selected.
        /// </summary>
        public DiagramItem SelectedItem
        {
            get { return this.selectedItem; }
            set { this.RaiseAndSetIfChanged(ref this.selectedItem, value); }
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="DiagramItem"/> items that are selected.
        /// </summary>
        public ReactiveList<DiagramItem> SelectedItems
        {
            get { return this.selectedItems; }
            set { this.RaiseAndSetIfChanged(ref this.selectedItems, value); }
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
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup
        {
            get { return this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>(); }
        }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise"/> name
        /// </summary>
        public string DomainOfExpertise
        {
            get { return this.domainOfExpertise; }
            private set { this.RaiseAndSetIfChanged(ref this.domainOfExpertise, value); }
        }

        /// <summary>
        /// The pending rule holds a rule in memory while creating connections, to apply to relationsships in case
        /// they are created from this rule.
        /// </summary>
        public BinaryRelationshipRule PendingBinaryRelationshipRule { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TargetName { get; set; } = "DocumentContainer";

        /// <summary>
        /// Creates a <see cref="BinaryRelationship"/>
        /// </summary>
        public void CreateBinaryRelationshipCommandExecute()
        {
            this.Behavior.ActivateConnectorTool();
        }

        /// <summary>
        /// Creates a <see cref="MultiRelationship"/>
        /// </summary>
        /// <param name="rule">
        /// The rule this relationship is based on.
        /// </param>
        public void CreateMultiRelationshipCommandExecute(MultiRelationshipRule rule = null)
        {
            var relatableThings = this.SelectedItems.Select(i => ((NamedThingDiagramContentItem) i).Thing);
            this.CreateMultiRelationship(relatableThings, rule);
        }

        /// <summary>
        /// Creates a <see cref="MultiRelationship"/>
        /// </summary>
        /// <param name="relatableThings">The list of <see cref="Thing"/> that this relationship will apply to.</param>
        /// <param name="rule">The <see cref="MultiRelationshipRule"/> that defines this relationship.</param>
        private async void CreateMultiRelationship(IEnumerable<Thing> relatableThings, MultiRelationshipRule rule)
        {
            // send off the relationship
            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing, out tuple);
            var multiRelationship = new MultiRelationship(Guid.NewGuid(), null, null) { Owner = tuple.Item1 };

            if (rule != null)
            {
                multiRelationship.Category.Add(rule.RelationshipCategory);
            }

            var iteration = this.Thing.Clone(false);

            iteration.Relationship.Add(multiRelationship);

            multiRelationship.Container = iteration;

            multiRelationship.RelatedThing = relatableThings.ToList();

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);

            var containerTransaction = new ThingTransaction(transactionContext, iteration);
            containerTransaction.CreateOrUpdate(multiRelationship);

            try
            {
                var operationContainer = containerTransaction.FinalizeTransaction();
                await this.Session.Write(operationContainer);

                // at this point relationship has gone through.
                var returedRelationship =
                    this.Thing.Relationship.FirstOrDefault(r => r.Iid == multiRelationship.Iid) as MultiRelationship;

                if (returedRelationship != null)
                {
                    this.CreateMultiRelationshipDiagramConnector(returedRelationship);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Creation of Binary Relationship failed: {0}", ex.Message),
                    "Binary Relationship Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Creates a <see cref="BinaryRelationship"/>
        /// </summary>
        /// <param name="connector">The drawn <see cref="DiagramConnector"/> that is used as a template.</param>
        private async void CreateBinaryRelationship(DiagramConnector connector)
        {
            var beginItemContent = ((DiagramContentItem) connector?.BeginItem)?.Content as NamedThingDiagramContentItem;
            var endItemContent = ((DiagramContentItem)connector?.EndItem)?.Content as NamedThingDiagramContentItem;

            if (beginItemContent == null || endItemContent == null)
            {
                // connector was drawn with either the source or target missing
                // remove the dummy connector
                this.Behavior.RemoveItem(connector);
                this.Behavior.ResetTool();
                return;
            }

            // send off the relationship
            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing, out tuple);

            var binaryRelationship = new BinaryRelationship(Guid.NewGuid(), null, null) { Owner = tuple.Item1 };

            if (this.PendingBinaryRelationshipRule != null)
            {
                binaryRelationship.Category.Add(this.PendingBinaryRelationshipRule.RelationshipCategory);
            }

            var iteration = this.Thing.Clone(false);

            iteration.Relationship.Add(binaryRelationship);

            binaryRelationship.Container = iteration;

            binaryRelationship.Source = beginItemContent.Thing;
            binaryRelationship.Target = endItemContent.Thing;

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var containerTransaction = new ThingTransaction(transactionContext, iteration);

            containerTransaction.CreateOrUpdate(binaryRelationship);

            try
            {
                var operationContainer = containerTransaction.FinalizeTransaction();
                await this.Session.Write(operationContainer);

                // at this point relationship has gone through.

                if (this.Thing.Relationship.FirstOrDefault(r => r.Iid == binaryRelationship.Iid) is BinaryRelationship returedRelationship)
                {
                    this.CreateBinaryRelationshipDiagramConnector(returedRelationship, connector);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Creation of Binary Relationship failed: {ex.Message}",
                    "Creating Binary Relationship Failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // remove the dummy connector
                this.Behavior.RemoveItem(connector);
                this.Behavior.ResetTool();
                this.PendingBinaryRelationshipRule = null;
            }
        }

        /// <summary>
        /// Creates a <see cref="MultiRelationshipDiagramConnector"/> from a given <see cref="MultiRelationship"/>.
        /// </summary>
        /// <param name="relationship">The <see cref="MultiRelationship"/> that defines this connector.</param>
        private void CreateMultiRelationshipDiagramConnector(MultiRelationship relationship)
        {
            foreach (var thing in relationship.RelatedThing)
            {
                var otherThings = relationship.RelatedThing.Except(new List<Thing> { thing }).ToList();

                var sourceItem =
                    this.ThingDiagramItems.OfType<NamedThingDiagramContentItem>().First(
                        x => x.Thing == thing);

                foreach (var item in otherThings)
                {
                    var targetItem =
                        this.ThingDiagramItems.OfType<NamedThingDiagramContentItem>().First(
                            x => x.Thing == item);

                    var newConnector = new MultiRelationshipDiagramConnector(relationship,
                        sourceItem, targetItem);

                    this.Behavior.AddConnector(newConnector);
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="BinaryRelationshipDiagramConnector"/> from a given <see cref="BinaryRelationship"/>.
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/> that defines this connector.</param>
        /// <param name="connector">The dummy <see cref="DiagramConnector"/> that was used as defenition for this connector.</param>
        private void CreateBinaryRelationshipDiagramConnector(BinaryRelationship relationship, DiagramConnector connector)
        {
            var newConnector = new BinaryRelationshipDiagramConnector(relationship, connector);

            this.Behavior.AddConnector(newConnector);
            this.SelectedItem = newConnector;
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            var rowPayload = dropInfo.Payload as Thing;

            if (rowPayload != null)
            {
                if (!this.ThingDiagramItems.OfType<NamedThingDiagramContentItem>().Select(x => x.Thing).Contains(rowPayload))
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                    return;
                }
            }

            if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> tuplePayload)
            {
                dropInfo.Effects = DragDropEffects.Copy;
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var rowPayload = dropInfo.Payload as Thing;

            var convertedDropPosition = this.Behavior.GetDiagramPositionFromMousePosition(dropInfo.DropPosition);

            if (rowPayload != null)
            {
                if (this.ThingDiagramItems.OfType<NamedThingDiagramContentItem>().Select(x => x.Thing).Contains(rowPayload))
                {
                    return;
                }

                var diagramItem = new NamedThingDiagramContentItem(rowPayload);
                this.Behavior.ItemPositions.Add(diagramItem, convertedDropPosition);
                this.ThingDiagramItems.Add(diagramItem);

                return;
            }

            if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> tuplePayload)
            {
                var diagramItem = new NamedThingDiagramContentItem(tuplePayload.Item1);
                this.Behavior.ItemPositions.Add(diagramItem, convertedDropPosition);
                this.ThingDiagramItems.Add(diagramItem);
            }
        }

        /// <summary>
        /// Removes items provided by the behavior.
        /// </summary>
        /// <param name="oldItems">The list of items to be removed.</param>
        public void RemoveItems(IList oldItems)
        {
            // wipes all selected items from the collection.
            foreach (var oldItem in oldItems)
            {
                var item = oldItem as DiagramItem;

                if (item != null)
                {
                    this.ThingDiagramItems.Remove(item);
                }
            }
        }

        /// <summary>
        /// Handles the action of a user selecting different row in the browser
        /// </summary>
        protected void OnSelectedThingChanged()
        {
            if (this.SelectedItem == null)
            {
                return;
            }

            var thingDiagramItem = this.SelectedItem as IThingDiagramItem;

            if (thingDiagramItem == null)
            {
                // logic to handle drawing of new connections
                var newDiagramConnector = this.SelectedItem as DiagramConnector;

                if (newDiagramConnector != null)
                {
                    // a new connection was drawn
                    this.CreateBinaryRelationship(newDiagramConnector);
                }

                return;
            }

            var thing = thingDiagramItem.Thing;

            if (thing == null)
            {
                return;
            }

            CDPMessageBus.Current.SendMessage(new SelectedThingChangedEvent(thing, this.Session));
        }

        /// <summary>
        /// Updates the properties of this viewmodel.
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing);

            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                this.DomainOfExpertise = "None";
            }
            else
            {
                this.DomainOfExpertise = (iterationDomainPair.Value == null || iterationDomainPair.Value.Item1 == null)
                    ? "None"
                    : string.Format("{0} [{1}]", iterationDomainPair.Value.Item1.Name, iterationDomainPair.Value.Item1.ShortName);
            }
        }
    }
}
