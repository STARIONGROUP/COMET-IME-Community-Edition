// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingDiagramConnectorViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Controls;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.Types;

    using CDP4CommonView.Diagram;

    using CDP4Composition.DragDrop;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using DevExpress.Xpf.Diagram;

    using NLog;

    using ReactiveUI;

    using Point = System.Windows.Point;
    using PropertyChangingEventArgs = ReactiveUI.PropertyChangingEventArgs;
    using PropertyChangingEventHandler = ReactiveUI.PropertyChangingEventHandler;

    /// <summary>
    /// Represents a diagram connector control class that can store a <see cref="Thing" />.
    /// </summary>
    public abstract class ThingDiagramConnectorViewModel : IDiagramConnectorViewModel, IReactiveObject, IIDropTarget
    {
        /// <summary>
        /// The <see cref="ISession"/> to be used when creating other view models
        /// </summary>
        protected ISession session;

        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger Logger;

        /// <summary>
        /// The <see cref="IDiagramEditorViewModel" /> container
        /// </summary>
        private readonly IDiagramEditorViewModel containerViewModel;

        /// <summary>
        /// Backing field for <see cref="IsDirty" />
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Backing field for <see cref="RevisionNumber" />
        /// </summary>
        private int revisionNumber;

        /// <summary>
        /// Backing field for <see cref="ConnectingPoints" />
        /// </summary>
        private List<Point> connectingPoints;

        /// <summary>
        /// Backing field for <see cref="IsFiltered"/>
        /// </summary>
        private bool isFiltered;

        /// <summary>
        /// Backing field for <see cref="DisplayedText" />
        /// </summary>
        private string displayedText;

        /// <summary>
        /// Backing field for <see cref="BeginItemPointIndex" />
        /// </summary>
        private int beginItemPointIndex;

        /// <summary>
        /// Backing field for <see cref="EndItemPointIndex" />
        /// </summary>
        private int endItemPointIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramConnectorViewModel" /> class.
        /// </summary>
        public ThingDiagramConnectorViewModel(DiagramEdge diagramThing, ISession session, IDiagramEditorViewModel containerViewModel)
        {
            if (diagramThing == null || containerViewModel == null)
            {
                return;
            }

            this.session = session;

            this.DiagramThing = diagramThing;
            this.containerViewModel = containerViewModel;
            this.Thing = diagramThing.DepictedThing;

            this.BeginItemPointIndex = -1;
            this.EndItemPointIndex = -1;

            this.SetSource(((DiagramEdge)this.DiagramThing).Source);
            this.SetTarget(((DiagramEdge)this.DiagramThing).Target);

            if (this.Source == this.Target)
            {
                this.BeginItemPointIndex = 0;
                this.EndItemPointIndex = 1;
            }

            this.InitializeSubscriptions();
        }

        /// <summary>
        /// Gets the target of the <see cref="DiagramEdge" />
        /// </summary>
        public DiagramElementThing Target { get; set; }

        /// <summary>
        /// Gets or sets the displayed text
        /// </summary>
        public string DisplayedText
        {
            get { return this.displayedText; }
            set { this.RaiseAndSetIfChanged(ref this.displayedText, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the node is filtered out
        /// </summary>
        public bool IsFiltered
        {
            get => this.isFiltered;
            set => this.RaiseAndSetIfChanged(ref this.isFiltered, value);
        }

        /// <summary>
        /// Gets or sets the collection of connecting <see cref="Point" />
        /// </summary>
        public List<Point> ConnectingPoints
        {
            get { return this.connectingPoints; }
            set { this.RaiseAndSetIfChanged(ref this.connectingPoints, value); }
        }

        /// <summary>
        /// Gets the source of the <see cref="DiagramEdge" />
        /// </summary>
        public DiagramElementThing Source { get; set; }

        /// <summary>
        /// Gets the source <see cref="IThingDiagramItemViewModel"/>
        /// </summary>
        public IThingDiagramItemViewModel BeginItem { get; set; }

        /// <summary>
        /// Gets the target <see cref="IThingDiagramItemViewModel" />
        /// </summary>
        public IThingDiagramItemViewModel EndItem { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CDP4Common.CommonData.Thing" /> representing the diagram with all of its diagram elements
        /// </summary>
        public DiagramElementThing DiagramThing { get; set; }

        /// <summary>
        /// Gets the list of <see cref="IDisposable" /> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; } = new();

        /// <summary>
        /// Gets the revision number of the <see cref="Thing" /> that is represented by the view-model when
        /// it was last updated
        /// </summary>
        public int RevisionNumber
        {
            get { return this.revisionNumber; }
            private set { this.RaiseAndSetIfChanged(ref this.revisionNumber, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return this.isDirty; }
            private set { this.RaiseAndSetIfChanged(ref this.isDirty, value); }
        }

        /// <summary>
        /// Gets or sets the begin point index
        /// </summary>
        public int BeginItemPointIndex
        {
            get { return this.beginItemPointIndex; }
            set { this.RaiseAndSetIfChanged(ref this.beginItemPointIndex, value); }
        }

        /// <summary>
        /// Gets or sets the end point index
        /// </summary>
        public int EndItemPointIndex
        {
            get { return this.endItemPointIndex; }
            set { this.RaiseAndSetIfChanged(ref this.endItemPointIndex, value); }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// A specific class that implements <see cref="IDropTarget" /> that can handle the <see cref="IDropTarget" />
        /// functionality
        /// for the class that this interface is applied to.
        /// </summary>
        public IDropTarget DropTarget { get; protected set; }

        /// <summary>
        /// <see cref="ReactiveUI.PropertyChangingEventHandler" /> event
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary>
        /// <see cref="System.ComponentModel.PropertyChangedEventHandler" /> event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Executes the PropertyChanging event
        /// </summary>
        /// <param name="args">The <see cref="ReactiveUI.PropertyChangingEventArgs" /></param>
        void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args)
        {
            var propertyChanging = this.PropertyChanging;

            if (propertyChanging == null)
            {
                return;
            }

            propertyChanging(this, args);
        }

        /// <summary>
        /// Executes the PropertyChanged event
        /// </summary>
        /// <param name="args">The <see cref="PropertyChangedEventArgs" /></param>
        void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            var propertyChanged = this.PropertyChanged;

            if (propertyChanged == null)
            {
                return;
            }

            propertyChanged(this, args);
        }

        /// <summary>
        /// Gets or sets the <see cref="IThingDiagramItemViewModel.Thing" />.
        /// </summary>
        public Thing Thing { get; set; }

        /// <summary>
        /// Initialize subscriptions
        /// </summary>
        private void InitializeSubscriptions()
        {
            var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.ObjectChangeEventHandler);

            this.Disposables.Add(thingSubscription);

            var thingRemoveSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing)
                .Where(objectChange => objectChange.EventKind == EventKind.Removed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RemoveEventHandler);

            this.Disposables.Add(thingRemoveSubscription);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing" /> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected virtual void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            this.RevisionNumber = objectChange.ChangedThing.RevisionNumber;
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for removes
        /// on the <see cref="Thing" /> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected virtual void RemoveEventHandler(ObjectChangedEvent objectChange)
        {
            this.containerViewModel.RemoveDiagramThingItemByThing(objectChange.ChangedThing);
        }

        /// <summary>
        /// Set the <see cref="BeginItem"/> from the view-model
        /// </summary>
        /// <param name="source">The source <see cref="IDiagramObjectViewModel"/></param>
        private void SetSource(object source)
        {
            this.BeginItem = this.GetDiagramContentItemToConnectTo(source);
        }

        /// <summary>
        /// Set the <see cref="EndItem"/> from the view-model
        /// </summary>
        /// <param name="target">The target <see cref="IDiagramObjectViewModel"/></param>
        private void SetTarget(object target)
        {
            this.EndItem = this.GetDiagramContentItemToConnectTo(target);
        }

        /// <summary>
        /// Get the the target <see cref="DiagramContentItem"/>
        /// to be set either as <see cref="BeginItem"/> or <see cref="EndItem"/>
        /// </summary>
        /// <param name="diagramThing">The <see cref="DiagramContentItem"/> to find</param>
        /// <returns>The <see cref="DiagramContentItem"/></returns>
        private IThingDiagramItemViewModel GetDiagramContentItemToConnectTo(object diagramThing)
        {
            var diagramObject = this.containerViewModel.ThingDiagramItemViewModels.SingleOrDefault(x => x.DiagramThing == diagramThing);

            if (diagramObject == null)
            {
                throw new InvalidOperationException("DiagramContentItem could not be found.");
            }

            return diagramObject;
        }

        /// <summary>
        /// Update the transaction with the data contained in this view-model
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="container">The container</param>
        public virtual void UpdateTransaction(IThingTransaction transaction, DiagramElementContainer container)
        {
            var clone = this.DiagramThing.Clone(true);

            container.DiagramElement.Add(clone);
            transaction.CreateOrUpdate(clone);
        }

        /// <summary>
        /// Reinitialize the view model with a new Thing from the cache
        /// </summary>
        public virtual void Reinitialize()
        {
            var cachedThingExists = this.containerViewModel.Thing.Cache.TryGetValue(new CacheKey(this.DiagramThing.Iid, this.containerViewModel.Thing.Container.Iid), out var cachedThing);

            if (cachedThingExists)
            {
                var newThing = cachedThing.Value as DiagramEdge;

                if (newThing is null)
                {
                    return;
                }

                this.DiagramThing = newThing;

                this.SetSource(((DiagramEdge)this.DiagramThing).Source);
                this.SetTarget(((DiagramEdge)this.DiagramThing).Target);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing) //Free any other managed objects here
            {
                // Clear all property values that maybe have been set
                // when the class was instantiated
                this.RevisionNumber = 0;
                this.Thing = null;

                if (this.Disposables != null)
                {
                    foreach (var disposable in this.Disposables)
                    {
                        disposable.Dispose();
                    }
                }
                else
                {
                    Logger.Trace("The Disposables collection of the {0} is null", this.GetType().Name);
                }
            }

            // Indicate that the instance has been disposed.
            this.isDisposed = true;
        }
    }
}
