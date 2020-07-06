// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Controls;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    
    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Grapher.Behaviors;
    using CDP4Grapher.Utilities;

    using DevExpress.CodeParser;
    using DevExpress.Diagram.Core;

    using ReactiveUI;

    /// <summary>
    /// The view model for the Grapher
    /// </summary>
    public class GrapherViewModel : BrowserViewModelBase<Option>, IPanelViewModel, IGrapherViewModel
    {
        /// <summary>
        /// Gets or sets the collection of <see cref="Thing"/> to display.
        /// </summary>
        public ReactiveList<NestedElement> ThingDiagramItems
        {
            get => this.thingDiagramItems;
            set => this.RaiseAndSetIfChanged(ref this.thingDiagramItems, value);
        }

        /// <summary>
        /// Backing field for <see cref="ThingDiagramItems"/>
        /// </summary>
        private ReactiveList<NestedElement> thingDiagramItems = new ReactiveList<NestedElement>();

        /// <summary>
        /// Gets or sets the attached behavior
        /// </summary>
        public IGrapherOrgChartBehavior Behavior { get; set; }

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
        /// The active <see cref="Participant"/>
        /// </summary>
        public readonly Participant ActiveParticipant;

        /// <summary>
        /// The <see cref="EngineeringModelSetup"/> that is referenced by the <see cref="EngineeringModel"/> that contains the current <see cref="Option"/>
        /// </summary>
        private readonly EngineeringModelSetup modelSetup;

        /// <summary>
        /// The container <see cref="iterationSetup"/> that is referenced by the container <see cref="Iteration"/> of the current <see cref="Option"/>.
        /// </summary>
        private readonly IterationSetup iterationSetup;
        
        /// <summary>
        /// Backing field for <see cref="CanExportDiagram"/>
        /// </summary>
        private bool canExportDiagram;

        /// <summary>
        /// Holds the current <see cref="Option"/>
        /// </summary>
        private readonly Option option;

        /// <summary>
        /// Holds the current <see cref="DomainOfExpertise"/>
        /// </summary>
        private readonly DomainOfExpertise currentDomainOfExpertise;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Grapher";

        /// <summary>
        /// Initializes a new instance of the <see cref="GrapherViewModel"/> class
        /// </summary>
        /// <param name="option">The <see cref="Option"/> of which this browser is of</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public GrapherViewModel(Option option, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(option, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, {this.Thing.Name}";
            this.ToolTip = $"{this.Thing.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.currentDomainOfExpertise = this.Session.QueryCurrentDomainOfExpertise();
            this.option = option;

            this.TopElement = new DisposableReactiveList<ElementDefinition>();
            var model = (EngineeringModel)this.Thing.TopContainer;
            this.modelSetup = model.EngineeringModelSetup;

            var iteration = (Iteration)this.Thing.Container;
            this.iterationSetup = iteration.IterationSetup;

            this.ActiveParticipant = this.modelSetup.Participant.Single(x => x.Person == this.Session.ActivePerson);

            var iterationSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(iteration)
                .Where(
                    objectChange =>
                        (objectChange.EventKind == EventKind.Updated) &&
                        (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.SetTopElement);
            
            this.Disposables.Add(iterationSubscription);
            
            this.AddSubscriptions();

            this.SetTopElement(iteration);

            this.UpdateProperties();
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
        /// Gets the current option caption to be displayed in the browser
        /// </summary>
        public string CurrentOption
        {
            get { return this.currentOption; }
            private set { this.RaiseAndSetIfChanged(ref this.currentOption, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the diagram can be exported
        /// </summary>
        public bool CanExportDiagram
        {
            get { return this.canExportDiagram; }
            private set { this.RaiseAndSetIfChanged(ref this.canExportDiagram, value); }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to export the generated diagram as png
        /// </summary>
        public ReactiveCommand<object> ExportGraphAsPng { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to export the generated diagram as pdf
        /// </summary>
        public ReactiveCommand<object> ExportGraphAsPdf { get; private set; }
        
        /// <summary>
        /// Gets the Top <see cref="ElementDefinition"/> for this <see cref="Option"/>
        /// </summary>
        /// <remarks>
        /// This has to be a list in order to display the tree
        /// </remarks>
        public DisposableReactiveList<ElementDefinition> TopElement { get; private set; }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            
            this.CanExportDiagram = true;
            
            this.ExportGraphAsPng = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExportDiagram));
            this.ExportGraphAsPng.Subscribe(_ => this.ExecuteExportGraphAsPng());
            this.ExportGraphAsPdf = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExportDiagram));
            this.ExportGraphAsPdf.Subscribe(_ => this.ExecuteExportGraphAsPdf());

            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out var tuple);
        }

        /// <summary>
        /// Handles the <see cref="DomainChangedEvent"/>
        /// </summary>
        /// <param name="domainChangeEvent">The <see cref="DomainChangedEvent"/></param>
        protected override void UpdateDomain(DomainChangedEvent domainChangeEvent)
        {
            base.UpdateDomain(domainChangeEvent);
            this.TopElement.ClearAndDispose();
            this.SetTopElement(this.Thing.Container as Iteration);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.CurrentOption = this.Thing.Name;
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
            this.TopElement.SingleOrDefault()?.Dispose();
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.modelSetup)
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber) && (objectChange.ChangedThing.Cache == this.Session.Assembler.Cache))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            
            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber) && (objectChange.ChangedThing.Cache == this.Session.Assembler.Cache))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            
            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.iterationSetup)
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber) && (objectChange.ChangedThing.Cache == this.Session.Assembler.Cache))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            
            this.Disposables.Add(iterationSetupSubscription);
        }

        /// <summary>
        /// Sets the top element for this Grapher
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> associated to this <see cref="Option"/></param>
        private void SetTopElement(Iteration iteration)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            var existingTopElement = this.TopElement.SingleOrDefault();
            var topElement = iteration.TopElement;

            if ((topElement == null) && (existingTopElement != null))
            {
                this.TopElement.ClearAndDispose();
            }
            else if ((topElement != null) && ((existingTopElement == null) || (existingTopElement != topElement)))
            {
                if (existingTopElement != null)
                {
                    this.TopElement.ClearAndDispose();
                }

                this.TopElement.Add(iteration.TopElement);
            }
        }
        
        /// <summary>
        /// Populate the <see cref="ElementUsage"/> property
        /// </summary>
        private void PopulateElementUsages()
        {
            this.ThingDiagramItems.AddRange(new NestedElementTreeGenerator().Generate(this.option, this.currentDomainOfExpertise).OrderBy(e => e.ElementUsage.Count));
        }
        
        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.modelSetup.Name;
            this.CurrentIteration = this.iterationSetup.IterationNumber;
            this.CurrentOption = this.Thing.Name;

            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing.Container);
            
            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                this.DomainOfExpertise = "None";
            }
            else
            {
                this.DomainOfExpertise = iterationDomainPair.Value.Item1 == null
                                        ? "None"
                                        : $"{iterationDomainPair.Value.Item1.Name} [{iterationDomainPair.Value.Item1.ShortName}]";
            }

            this.PopulateElementUsages();
        }

        /// <summary>
        /// Executes the <see cref="ExportGraphAsPng"/> 
        /// </summary>
        private void ExecuteExportGraphAsPdf()
        {
            this.canExportDiagram = false;
            this.Behavior.ApplySpecifiedAutoLayout();
            this.Behavior.ExportGraph(DiagramExportFormat.PDF);
            this.canExportDiagram = true;
        }

        /// <summary>
        /// Executes the <see cref="ExportGraphAsPng"/> 
        /// </summary>
        private void ExecuteExportGraphAsPng()
        {
            this.canExportDiagram = false;
            this.Behavior.ExportGraph(DiagramExportFormat.JPEG);
            this.canExportDiagram = true;
        }
    }
}