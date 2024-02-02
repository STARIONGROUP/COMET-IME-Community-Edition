// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingDiagramContentItem.cs" company="RHEA System S.A.">
// //    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Diagram
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using DevExpress.Xpf.Diagram;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Represents a diagram content control class that can store a <see cref="Thing"/>.
    /// </summary>
    public abstract class ThingDiagramContentItemViewModel : IThingDiagramItemViewModel, IReactiveObject, IIDropTarget
    {
        /// <summary>
        /// The <see cref="ISession"/> to be used when creating other view models
        /// </summary>
        protected ISession session;

        /// <summary> 
        /// <see cref="ReactiveUI.PropertyChangingEventHandler"/> event
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging;

        /// <summary> 
        /// <see cref="System.ComponentModel.PropertyChangedEventHandler"/> event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger Logger;

        /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Backing field for <see cref="RevisionNumber"/>
        /// </summary>
        private int revisionNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramContentItemViewModel"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing represented.</param>
        /// <param name="session">The <see cref="ISession"/></param>
        protected ThingDiagramContentItemViewModel(Thing thing, ISession session)
        {
            this.session = session;
            this.Thing = thing;
            this.InitializeSubscriptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramContentItemViewModel"/> class.
        /// </summary>
        /// <param name="diagramThing">
        /// The diagramThing contained</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">
        /// The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        protected ThingDiagramContentItemViewModel(DiagramElementThing diagramThing, ISession session, IDiagramEditorViewModel containerViewModel)
        {
            this.session = session;

            this.containerViewModel = containerViewModel;
            this.Thing = diagramThing.DepictedThing;
            this.DiagramThing = diagramThing;
            this.InitializeSubscriptions();
        }

        /// <summary>
        /// Gets the list of <see cref="IDisposable"/> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; } = new();

        /// <summary>
        /// Gets the revision number of the <see cref="Thing"/> that is represented by the view-model when
        /// it was last updated
        /// </summary>
        public int RevisionNumber
        {
            get => this.revisionNumber;
            private set => this.RaiseAndSetIfChanged(ref this.revisionNumber, value);
        }

        /// <summary>
        /// The <see cref="IDiagramEditorViewModel"/> container
        /// </summary>
        protected readonly IDiagramEditorViewModel containerViewModel;

        /// <summary>
        /// Backing field for <see cref="IsDirty"/>
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// Backing field for <see cref="DisplayText"/>
        /// </summary>
        private string displayText;

        /// <summary>
        /// Backing field for <see cref="IsFiltered"/>
        /// </summary>
        private bool isFiltered;

        /// <summary>
        /// Gets or sets the <see cref="IThingDiagramItemViewModel.Thing"/>.
        /// </summary>
        public Thing Thing { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CDP4Common.CommonData.Thing"/> representing the diagram with all of its diagram elements
        /// </summary>
        public DiagramElementThing DiagramThing { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DiagramItem"/> that represents this viewmodel on the convas
        /// </summary>
        public DiagramItem DiagramRepresentation { get; set; }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        public bool IsDirty
        {
            get => this.isDirty;
            protected set => this.RaiseAndSetIfChanged(ref this.isDirty, value);
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
        /// Gets or sets the text
        /// </summary>
        public string DisplayText
        {
            get => this.displayText;
            set => this.RaiseAndSetIfChanged(ref this.displayText, value);
        }

        /// <summary>
        /// The observable for when the position of the view object changed
        /// </summary>
        public IDisposable PositionObservable { get; set; }

        /// <summary>
        /// A specific class that handles the <see cref="IDropTarget"/> functionality
        /// </summary>
        public IDropTarget DropTarget { get; protected set; }

        /// <summary>
        /// Initialize subscriptions
        /// </summary>
        private void InitializeSubscriptions()
        {
            var thingSubscription = this.session.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.ObjectChangeEventHandler);

            this.Disposables.Add(thingSubscription);

            var thingRemoveSubscription = this.session.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing)
                .Where(objectChange => objectChange.EventKind == EventKind.Removed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RemoveEventHandler);

            this.Disposables.Add(thingRemoveSubscription);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
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
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected virtual void RemoveEventHandler(ObjectChangedEvent objectChange)
        {
            this.containerViewModel.RemoveDiagramThingItemByThing(objectChange.ChangedThing);
        }

        /// <summary>
        /// Sets the <see cref="IsDirty"/> property
        /// </summary>
        public virtual void SetDirty()
        {
            var bound = this.DiagramThing.Bounds.Single();

            this.IsDirty = this.DiagramRepresentation is DiagramContentItem parent
                           && parent.Position != default
                           && this.Thing != null
                           && (this.Thing.Iid == Guid.Empty
                               || (float)parent.Position.X != bound.X
                               || (float)parent.Position.Y != bound.Y);

            this.containerViewModel?.UpdateIsDirty();
        }

        /// <summary>
        /// Update the transaction with the data contained in this view-model
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="container">The container</param>
        public virtual void UpdateTransaction(IThingTransaction transaction, DiagramElementContainer container)
        {
            if (this.Thing.Iid == Guid.Empty)
            {
                if (!(this.DiagramThing is DiagramEdge))
                {
                    var bound = this.DiagramThing.Bounds.Single();
                    this.UpdateBound(bound);
                    transaction.Create(bound);
                }

                container.DiagramElement.Add(this.DiagramThing);
                transaction.Create(this.Thing);
            }
            else
            {
                var clone = this.DiagramThing.Clone(true);

                if (!(this.DiagramThing is DiagramEdge))
                {
                    var bound = clone.Bounds.SingleOrDefault();
                    if (bound != null)
                    {
                        this.UpdateBound(bound);
                    }

                    transaction.CreateOrUpdate(bound);
                }

                container.DiagramElement.Add(clone);
                transaction.CreateOrUpdate(clone);
            }
        }

        /// <summary>
        /// Reinitializes the viewmodel with the Thing from cache
        /// </summary>
        public virtual void Reinitialize()
        {
            var cachedThingExists = this.containerViewModel.Thing.Cache.TryGetValue(new CacheKey(this.DiagramThing.Iid, this.containerViewModel.Thing.Container.Iid), out var cachedThing);

            if (cachedThingExists)
            {
                var newThing = cachedThing.Value as DiagramElementThing;

                if (newThing is null)
                {
                    return;
                }

                this.DiagramThing = newThing;
            }
        }

        /// <summary>
        /// Update a <see cref="Bounds"/> with the current values
        /// </summary>
        /// <param name="bound">The <see cref="Bounds"/> to update</param>
        private void UpdateBound(Bounds bound)
        {
            if (this.DiagramRepresentation is DiagramContentItem parent)
            {
                bound.Height = (float)parent.ActualHeight;
                bound.Width = (float)parent.ActualWidth;
                bound.X = (float)parent.Position.X;
                bound.Y = (float)parent.Position.Y;
                bound.Name = "should not have a name";
            }
        }

        /// <summary>
        /// Executes the PropertyChanging event
        /// </summary>
        /// <param name="args">The <see cref="PropertyChangingEventArgs"/></param>
        void IReactiveObject.RaisePropertyChanging(PropertyChangingEventArgs args)
        {
            var propertyChanging = this.PropertyChanging;

            if (propertyChanging == null)
            {
                return;
            }

            propertyChanging((object)this, args);
        }

        /// <summary>
        /// Executes the PropertyChanged event
        /// </summary>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/></param>
        void IReactiveObject.RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            var propertyChanged = this.PropertyChanged;

            if (propertyChanged == null)
            {
                return;
            }

            propertyChanged((object)this, args);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
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

                this.PositionObservable?.Dispose();

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
    }
}
