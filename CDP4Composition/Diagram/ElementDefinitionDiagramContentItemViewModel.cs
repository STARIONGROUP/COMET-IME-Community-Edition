// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDiagramContentItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4Composition.Diagram
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.ViewModels;

    using CDP4Composition.DragDrop;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    using Point = System.Windows.Point;

    /// <summary>
    /// Represents an <see cref="ElementDefinition"/> to be used in a Diagram
    /// </summary>
    public class ElementDefinitionDiagramContentItemViewModel : PortContainerDiagramContentItemViewModel, IDiagramContentItemChildren
    {
        /// <summary>
        /// Backing fied for <see cref="IsTopDiagramElement"/>
        /// </summary>
        private bool isTopDiagramElement;

        /// <summary>
        /// Backing field for <see cref="IsParameterGroupCollapsed"/>
        /// </summary>
        private bool isParameterGroupCollapsed;

        /// <summary>
        /// Gets or sets the Children of the <see cref="ElementDefinitionDiagramContentItemViewModel"/>
        /// </summary>
        public ReactiveList<IDiagramContentItemChild> DiagramContentItemChildren { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedThingDiagramContentItemViewModel"/> class.
        /// </summary>
        /// <param name="diagramThing">
        /// The diagramThing contained</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="container">
        /// The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        public ElementDefinitionDiagramContentItemViewModel(ArchitectureElement diagramThing, ISession session, IDiagramEditorViewModel container)
            : base(diagramThing, session, container)
        {
            if (diagramThing.DepictedThing is ElementDefinition elementDefinition)
            {
                this.DropTarget = new ElementDefinitionDropTarget(elementDefinition, this.session);
            }

            var collapseObservable = this.WhenAnyValue(vm => vm.IsParameterGroupCollapsed).Subscribe(_ => this.UpdatePortLayout());
            this.Disposables.Add(collapseObservable);

            this.SetUpElementUsageListeners();

            this.UpdateProperties();

            this.IsParameterGroupCollapsed = true;
        }

        /// <summary>
        /// Setup the listeners for ElementUsages in case they are getting changed to ports manually
        /// </summary>
        private void SetUpElementUsageListeners()
        {
            var containedElement = (this.Thing as ElementDefinition)?.ContainedElement;

            if (containedElement != null)
            {
                foreach (var eu in containedElement.ToList())
                {
                    if (this.UsageSubscriptions.ContainsKey(eu))
                    {
                        continue;
                    }

                    var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(eu)
                        .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(this.ObjectChangeEventHandler);

                    this.Disposables.Add(thingSubscription);
                    this.UsageSubscriptions.Add(eu, thingSubscription);

                    var thingRemoveSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(eu)
                        .Where(objectChange => objectChange.EventKind == EventKind.Removed)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(this.CleanupElementUsageSubscriptions);

                    this.Disposables.Add(thingRemoveSubscription);
                }
            }
        }

        /// <summary>
        /// Gets the dictionary of <see cref="ElementUsage"/> subscriptions.
        /// </summary>
        public Dictionary<ElementUsage, IDisposable> UsageSubscriptions { get; } = new Dictionary<ElementUsage, IDisposable>();

        /// <summary>
        /// Gets or sets the value indicating whether this is a top element of the <see cref="ArchitectureDiagram"/>
        /// </summary>
        public bool IsTopDiagramElement
        {
            get => this.isTopDiagramElement;
            set => this.RaiseAndSetIfChanged(ref this.isTopDiagramElement, value);
        }

        /// <summary>
        /// Gets or sets whether the parameter group is collapsed
        /// </summary>
        public bool IsParameterGroupCollapsed
        {
            get => this.isParameterGroupCollapsed;
            set => this.RaiseAndSetIfChanged(ref this.isParameterGroupCollapsed, value);
        }

        /// <summary>
        /// Cleanup subscriptions related to <see cref="ElementUsage"/>s
        /// </summary>
        /// <param name="objectChange">The event holding the <see cref="ElementUsage"/></param>
        private void CleanupElementUsageSubscriptions(ObjectChangedEvent objectChange)
        {
            if (objectChange.ChangedThing is not ElementUsage eu)
            {
                return;
            }

            var subscription = this.UsageSubscriptions.TryGetValue(eu, out var disposable);

            if (!subscription)
            {
                return;
            }

            this.Disposables.Remove(disposable);
            this.UsageSubscriptions.Remove(eu);

            disposable.Dispose();
        }

        /// <summary>
        /// Sets <see cref="ElementDefinitionDiagramContentItemViewModel.Thing"/> related properties
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Thing is ElementDefinition elementDefinition)
            {
                this.DiagramContentItemChildren.Clear();

                foreach (var parameter in elementDefinition.Parameter.OrderBy(x => x.ParameterType.Name))
                {
                    var parameterRowViewModel = new DiagramContentItemParameterRowViewModel(parameter, this.session, null);
                    this.DiagramContentItemChildren.Add(parameterRowViewModel);
                }
            }
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
            this.UpdateProperties();
            this.SetUpElementUsageListeners();
            this.UpdatePorts();
        }

        /// <summary>
        /// Gets the height of the height of the representing <see cref="DiagramItem"/>
        /// </summary>
        /// <returns>The actual height</returns>
        public double GetDiagramContentItemHeight()
        {
            return this.DiagramRepresentation.ActualHeight;
        }

        /// <summary>
        /// Gets the width of the height of the representing <see cref="DiagramItem"/>
        /// </summary>
        /// <returns>The actual width</returns>
        public double GetDiagramContentItemWidth()
        {
            return this.DiagramRepresentation.ActualWidth;
        }

        /// <summary>
        /// Creates a new <see cref="ElementDefinitionDiagramContentItemViewModel"/>
        /// </summary>
        /// <param name="session">The session</param>
        /// <param name="definition">The <see cref="ElementDefinition"/></param>
        /// <param name="editor">The containing <see cref="IDiagramEditorViewModel"/></param>
        /// <param name="position">The placement position</param>
        /// <returns>The new <see cref="ElementDefinitionDiagramContentItemViewModel"/></returns>
        public static ElementDefinitionDiagramContentItemViewModel CreatElementDefinitionDiagramContentItemViewModel(ISession session, ElementDefinition definition, IDiagramEditorViewModel editor, Point position)
        {
            var bounds = new Bounds(Guid.NewGuid(), session.Assembler.Cache, new Uri(session.DataSourceUri))
            {
                X = (float)position.X,
                Y = (float)position.Y,
                Name = definition.UserFriendlyName
            };

            var architectureBlock = new ArchitectureElement(Guid.NewGuid(), session.Assembler.Cache, new Uri(session.DataSourceUri))
            {
                DepictedThing = definition,
                Name = definition.UserFriendlyName,
                Documentation = definition.UserFriendlyName,
                Resolution = Cdp4DiagramHelper.DefaultResolution
            };

            architectureBlock.Bounds.Add(bounds);

            return new ElementDefinitionDiagramContentItemViewModel(architectureBlock, session, editor);
        }
    }
}
