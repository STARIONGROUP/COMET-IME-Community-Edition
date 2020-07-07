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
    using DevExpress.Mvvm.Native;

    using ReactiveUI;

    /// <summary>
    /// The view model for the Grapher
    /// </summary>
    public class GrapherViewModel : BrowserViewModelBase<Option>, IPanelViewModel, IGrapherViewModel
    {
        /// <summary>
        /// Gets or sets the collection of <see cref="Thing"/> to display.
        /// </summary>
        public ReactiveList<NestedElement> NestedElements
        {
            get => this.nestedElements;
            set => this.RaiseAndSetIfChanged(ref this.nestedElements, value);
        }

        /// <summary>
        /// Backing field for <see cref="NestedElements"/>
        /// </summary>
        private ReactiveList<NestedElement> nestedElements = new ReactiveList<NestedElement>();

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

        private Dictionary<NestedElement, IDisposable> elementSubscription = new Dictionary<NestedElement, IDisposable>();

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

            this.modelSetup = ((EngineeringModel)this.Thing.TopContainer).EngineeringModelSetup;

            this.iterationSetup = ((Iteration)this.Thing.Container).IterationSetup;

            this.ActiveParticipant = this.modelSetup.Participant.Single(x => x.Person == this.Session.ActivePerson);
            
            this.AddSubscriptions();

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

            var iterationSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>((Iteration)this.Thing.Container)
                .Where(
                    objectChange =>
                        (objectChange.EventKind == EventKind.Updated) &&
                        (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSubscription);
            
            var optionSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing)
                .Where(
                    objectChange =>
                        (objectChange.EventKind == EventKind.Updated) &&
                        (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(optionSubscription);
        }
        
        /// <summary>
        /// Populate the <see cref="ElementUsage"/> property
        /// </summary>
        private void PopulateElementUsages()
        {
            var elements = new NestedElementTreeGenerator().Generate(this.option, this.currentDomainOfExpertise).OrderBy(e => e.ElementUsage.Count).ToList();

            foreach (var nestedElement in elements)
            {
                IDisposable listener = null;
                if (nestedElement.IsRootElement)
                {
                    listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(nestedElement.Container)
                        .Where(
                            objectChange =>
                                objectChange.EventKind == EventKind.Updated &&
                                objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => this.UpdateProperties());
                }
                else
                {
                    var elementUsage = nestedElement.GetElementUsage();
                    listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(elementUsage)
                        .Where(
                            objectChange =>
                                objectChange.EventKind == EventKind.Updated &&
                                objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(this.UpdateElementUsage);
                }

                this.elementSubscription[nestedElement] = listener;
            }

            this.NestedElements.AddRange(elements);
        }

        /// <summary>
        /// Use to update only one Element usage properties
        /// </summary>
        /// <param name="objectChangedEvent"></param>
        private void UpdateElementUsage(ObjectChangedEvent objectChangedEvent)
        {
            var elementUsage = this.NestedElements.Select(e => e.GetElementUsage());
            var elementToUpdate = elementUsage.FirstOrDefault(e => e == objectChangedEvent.ChangedThing);
            
            //var elementToUpdate = this.NestedElements.FirstOrDefault(e => e.GetElementUsage() == objectChangedEvent.ChangedThing).GetElementUsage();
            elementToUpdate = objectChangedEvent.ChangedThing as ElementUsage;
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

            this.ClearNestedElementsAndDisposeSubscriptions();

            this.PopulateElementUsages();
        }

        /// <summary>
        /// Dispose of the subscriptions and clears up collections
        /// </summary>
        private void ClearNestedElementsAndDisposeSubscriptions()
        {
            this.elementSubscription.Values.ForEach(x => x.Dispose());
            this.elementSubscription.Clear();
            this.NestedElements.Clear();
            this.Behavior?.ClearConnectors();
        }

        /// <summary>
        /// Executes the <see cref="ExportGraphAsPng"/> 
        /// </summary>
        private void ExecuteExportGraphAsPdf()
        {
            this.canExportDiagram = false;
            //this.Behavior.ApplySpecifiedAutoLayout();
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