// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Grapher.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Grapher.Behaviors;

    using DevExpress.Mvvm.Native;

    using ReactiveUI;

    /// <summary>
    /// The view model for the Grapher
    /// </summary>
    public class GrapherViewModel : BrowserViewModelBase<Option>, IPanelViewModel, IGrapherViewModel
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
        /// Backing field for <see cref="CurrentOption"/>
        /// </summary>
        private string currentOption;

        /// <summary>
        /// Backing field for <see cref="SelectedElement"/>
        /// </summary>
        private ElementParameterRowControlViewModel selectedElement;

        /// <summary>
        /// The <see cref="EngineeringModelSetup"/> that is referenced by the <see cref="EngineeringModel"/> that contains the current <see cref="Option"/>
        /// </summary>
        private readonly EngineeringModelSetup modelSetup;

        /// <summary>
        /// The container <see cref="iterationSetup"/> that is referenced by the container <see cref="Iteration"/> of the current <see cref="Option"/>.
        /// </summary>
        private readonly IterationSetup iterationSetup;

        /// <summary>
        /// Holds the current <see cref="Option"/>
        /// </summary>
        private readonly Option option;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Grapher";

        /// <summary>
        /// Backing field for the <see cref="SelectedElementModelCode"/> Property
        /// </summary>
        private string selectedElementModelCode;

        /// <summary>
        /// Gets or sets the attached behavior
        /// </summary>
        public IGrapherOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
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
        /// Gets the current option caption to be displayed in the browser
        /// </summary>
        public string CurrentOption
        {
            get => this.currentOption;
            private set => this.RaiseAndSetIfChanged(ref this.currentOption, value);
        }

        /// <summary>
        /// Gets or sets the selected element
        /// </summary>
        public ElementParameterRowControlViewModel SelectedElement
        {
            get => this.selectedElement;
            set => this.RaiseAndSetIfChanged(ref this.selectedElement, value);
        }

        /// <summary>
        /// Gets or sets the selected element model code
        /// </summary>
        public string SelectedElementModelCode
        {
            get => this.selectedElementModelCode;
            set => this.RaiseAndSetIfChanged(ref this.selectedElementModelCode, value);
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="GraphElementViewModel"/> to display.
        /// </summary>
        public ReactiveList<GraphElementViewModel> GraphElements { get; } = new ReactiveList<GraphElementViewModel>();

        /// <summary>
        /// Gets or sets the custom context menu
        /// </summary>
        public IHaveContextMenu DiagramContextMenuViewModel { get; set; } = new DiagramControlContextMenuViewModel();

        /// <summary>
        /// Initializes a new instance of the <see cref="GrapherViewModel"/> class
        /// </summary>
        /// <param name="option">The <see cref="Option"/> of which this browser is of</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService"> The <see cref="IPluginSettingsService"/> used to read and write plugin setting files. </param>
        public GrapherViewModel(Option option, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(option, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";
            this.option = option;

            this.modelSetup = ((EngineeringModel)this.Thing.TopContainer).EngineeringModelSetup;

            this.iterationSetup = ((Iteration)this.Thing.Container).IterationSetup;

            this.AddSubscriptions();

            this.UpdateProperties();
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.modelSetup)
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.Cache == this.Session.Assembler.Cache))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.Cache == this.Session.Assembler.Cache))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.iterationSetup)
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.Cache == this.Session.Assembler.Cache))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);

            var iterationSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>((Iteration)this.Thing.Container)
                .Where(objectChange => (objectChange.EventKind == EventKind.Updated))
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSubscription);

            var optionSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing)
                .Where(
                    objectChange =>
                        (objectChange.EventKind == EventKind.Updated) &&
                        (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                .Select(x => x.ChangedThing as Option)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(optionSubscription);

            var elementUsageSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ElementUsage))
                .Where(objectChange => objectChange.EventKind != EventKind.Updated)
                .Select(x => x.ChangedThing as ElementUsage)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(elementUsageSubscription);
        }

        /// <summary>
        /// Populate the <see cref="ElementUsage"/> property
        /// </summary>
        private void PopulateElementUsages()
        {
            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise((Iteration)this.Thing.Container);

            var elements = new NestedElementTreeGenerator().Generate(this.option, currentDomainOfExpertise).OrderBy(e => e.ElementUsage.Count).ThenBy(e => e.Name);
            this.GraphElements.AddRange(elements.Select(e => new GraphElementViewModel(e)));
        }

        /// <summary>
        /// Calculate and update the element of the tree under the <see cref="graphElement"/>
        /// </summary>
        /// <param name="graphElement">The Graph Element</param>
        public void Isolate(GraphElementViewModel graphElement)
        {
            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise((Iteration)this.Thing.Container);

            var newTree = new NestedElementTreeGenerator().GenerateNestedElements(this.option, currentDomainOfExpertise, graphElement.Thing.ElementUsage.Last().ElementDefinition)
                .OrderBy(e => e.ElementUsage.Count).ThenBy(e => e.Name);
            
            this.GraphElements.Clear();
            this.GraphElements.AddRange(newTree.Select(e => new GraphElementViewModel(e)));
        }

        /// <summary>
        /// Exits the isolation
        /// </summary>
        public void ExitIsolation()
        {
            this.GraphElements.Clear();
            this.PopulateElementUsages();
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.modelSetup.Name;
            this.CurrentIteration = this.iterationSetup.IterationNumber;
            this.CurrentOption = this.Thing.Name;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise((Iteration)this.Thing.Container);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";

            this.ClearNestedElementsAndDisposeSubscriptions();
            this.PopulateElementUsages();
            this.Behavior?.ApplyPreviousLayout();
        }

        /// <summary>
        /// Dispose of the subscriptions and clears up collections
        /// </summary>
        private void ClearNestedElementsAndDisposeSubscriptions()
        {
            this.GraphElements.ForEach(x => x.NestedElementElementListener.Dispose());
            this.GraphElements.Clear();
        }
        
        /// <summary>
        /// Sets the selected element and the selected element model code
        /// </summary>
        /// <param name="element">The selected element</param>
        public void SetsSelectedElementAndSelectedElementPath(GraphElementViewModel element)
        {
            this.SelectedElement = new ElementParameterRowControlViewModel(element.NestedElementElement, this.option);
            this.SelectedElementModelCode = element.ModelCode;
        }
    }
}
