// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RowViewModelBase.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
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

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.MessageBus;

    using Converters;

    using Navigation;

    using Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Events;

    using Navigation.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The Base view-model class for rows
    /// </summary>
    /// <typeparam name="T">The <see cref="Thing"/> represented by the row</typeparam>
    public abstract class RowViewModelBase<T> : ViewModelBase<T>, IHaveThingStatus, IRowViewModelBase<T> where T : Thing
    {
        /// <summary>
        /// The <see cref="IDialogNavigationService"/> that is responsible for navigating to a <see cref="IDialogViewModel"/>
        /// </summary>
        protected readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Backing field for <see cref="Index"/>
        /// </summary>
        private int index;

        /// <summary>
        /// Out property for the <see cref="HasError"/> property
        /// </summary>
        private bool hasError;

        /// <summary>
        /// Backing property for the <see cref="IsHighlighted"/>
        /// </summary>
        private bool isHighlighted;

        /// <summary>
        /// Backing field for <see cref="ErrorMsg"/>
        /// </summary>
        private string errorMsg;

        /// <summary>
        /// Backing field for <see cref="RowStatus"/>
        /// </summary>
        private RowStatusKind rowStatus;

        /// <summary>
        /// Backing field for <see cref="IsExpanded"/>
        /// </summary>
        private bool isExpanded;

        /// <summary>
        /// Backing field for <see cref="Tooltip"/>
        /// </summary>
        private string tooltip;

        /// <summary>
        /// Backing field for <see cref="ThingStatus"/>
        /// </summary>
        private ThingStatus thingStatus;

        /// <summary>
        /// Backing field for <see cref="Details"/>
        /// </summary>
        private string details;

        /// <summary>
        /// Initializes a new instance of the <see cref="RowViewModelBase{T}"/> class.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> represented by the row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The parent Row</param>
        protected RowViewModelBase(T thing, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(thing, session, containerViewModel)
        {
            this.ContainedRows = new DisposableReactiveList<IRowViewModelBase<Thing>>();
            this.HighlightCancelDisposables = new List<IDisposable>();

            var rowContainerViewModel = this.ContainerViewModel as IRowViewModelBase<Thing>;

            // todo implement the container view-model on all rows
            this.TopContainerViewModel = (rowContainerViewModel == null)
                ? this.ContainerViewModel
                : rowContainerViewModel.TopContainerViewModel;

            var browser = this.TopContainerViewModel as IBrowserViewModelBase<Thing>;
            if (browser != null)
            {
                // get the IDialogNavigationService from the browser
                this.dialogNavigationService = browser.DialogNavigationService;
            }

            if (this.Thing is NotThing)
            {
                return;
            }

            this.InitializeSubscriptions();
            this.UpdateTooltip();
        }

        /// <summary>
        /// Gets or sets a value representing the <see cref="RowStatusKind"/>
        /// </summary>
        /// <remarks>
        /// The default is RowStatusKind.Active
        /// </remarks>
        public RowStatusKind RowStatus
        {
            get { return this.rowStatus; }
            set { this.RaiseAndSetIfChanged(ref this.rowStatus, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="ThingStatus"/>
        /// </summary>
        public ThingStatus ThingStatus
        {
            get { return this.thingStatus; }
            protected set { this.RaiseAndSetIfChanged(ref this.thingStatus, value); }
        }

        /// <summary>
        /// Gets or sets the Contained <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public DisposableReactiveList<IRowViewModelBase<Thing>> ContainedRows { get; protected set; }

        /// <summary>
        /// Gets the top container <see cref="IViewModelBase{T}"/>
        /// </summary>
        /// <remarks>
        /// this should either be a <see cref="IDialogViewModelBase{T}"/> or a <see cref="IBrowserViewModelBase{T}"/>
        /// </remarks>
        public IViewModelBase<Thing> TopContainerViewModel { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the row has an error upon a inline operation
        /// </summary>
        public bool HasError
        {
            get { return this.hasError; }
            set { this.RaiseAndSetIfChanged(ref this.hasError, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the row is highlighted
        /// </summary>
        public bool IsHighlighted
        {
            get { return this.isHighlighted; }
            set { this.RaiseAndSetIfChanged(ref this.isHighlighted, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the row is favorited
        /// </summary>
        public bool IsFavorite
        {
            get { return this.isHighlighted; }
            set { this.RaiseAndSetIfChanged(ref this.isHighlighted, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this row should be expanded or collapsed
        /// </summary>
        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set { this.RaiseAndSetIfChanged(ref this.isExpanded, value); }
        }

        /// <summary>
        /// Expands the current row and all contained rows along the containment hierarchy
        /// </summary>
        public void ExpandAllRows()
        {
            this.IsExpanded = true;

            foreach (var row in this.ContainedRows)
            {
                row.ExpandAllRows();
            }
        }

        /// <summary>
        /// Collapses the current row and all contained rows along the containment hierarchy
        /// </summary>
        public void CollapseAllRows()
        {
            this.IsExpanded = false;

            foreach (var row in this.ContainedRows)
            {
                row.CollapseAllRows();
            }
        }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMsg
        {
            get { return this.errorMsg; }
            set { this.RaiseAndSetIfChanged(ref this.errorMsg, value); }
        }

        /// <summary>
        /// Gets or sets the index of the row
        /// </summary>
        /// <remarks>
        /// this property is used in the case of <see cref="OrderedItemList{T}"/>
        /// </remarks>
        public int Index
        {
            get { return this.index; }
            set { this.RaiseAndSetIfChanged(ref this.index, value); }
        }

        /// <summary>
        /// Gets or sets the tooltip
        /// </summary>
        public string Tooltip
        {
            get { return this.tooltip; }
            protected set { this.RaiseAndSetIfChanged(ref this.tooltip, value); }
        }

        /// <summary>
        /// Gets or sets the details
        /// </summary>
        public string Details
        {
            get { return this.details; }
            protected set { this.RaiseAndSetIfChanged(ref this.details, value); }
        }

        /// <summary>
        /// Indicates if edit functions in context menu are active
        /// </summary>
        public virtual bool CanEditThing { get; } = true;

        /// <summary>
        /// Gets the list of <see cref="IDisposable"/> objects that are referenced by this class used for cancelation of highlight.
        /// </summary>
        protected List<IDisposable> HighlightCancelDisposables { get; private set; }

        /// <summary>
        /// The ClassKind of the current thing
        /// </summary>
        public virtual string RowType
        {
            get
            {
                var converter = new CamelCaseToSpaceConverter();
                return (this.Thing == null)
                    ? "-"
                    : converter.Convert(this.Thing.ClassKind, null, null, null)?.ToString();
            }
        }

        /// <summary>
        /// Gets the validation error message
        /// </summary>
        public string Error { get; protected set; }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        /// <remarks>
        /// Used by the view through the IDataErrorInfo interface to validate a field onPropertyChanged
        /// </remarks>
        public virtual string this[string columnName]
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">The name of the property whose error message to get</param>
        /// <param name="newValue">The new value for the row</param>
        /// <returns>The error message for the property. The default is an empty string ("").</returns>
        /// <remarks>
        /// Used when inline-editing, the values are updated on focus lost
        /// </remarks>
        public virtual string ValidateProperty(string columnName, object newValue)
        {
            return ValidationService.ValidateProperty(columnName, newValue, this);
        }

        /// <summary>
        /// Clears the row highlighting for itself and its children.
        /// </summary>
        public void ClearRowHighlighting()
        {
            if (this.IsHighlighted)
            {
                this.IsHighlighted = false;
            }
        }

        /// <summary>
        /// Computes for the entire row or for a specific property of the row whether it is editable based on the
        /// result of the <see cref="PermissionService.CanWrite"/> method and potential conditions of the property of the Row that is being edited.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property for which the value is computed. This allows to include the
        /// specific property of the row-view-model in the computation. If the propertyname is empty
        /// then the whole row is taken into account. If a property is specified only that property
        /// is taken into account.
        /// </param>
        /// <returns>
        /// True if the row or the more specific the property is editable or not.
        /// </returns>
        public virtual bool IsEditable(string propertyName = "")
        {
            return this.Session.PermissionService.CanWrite(this.Thing);
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected virtual void InitializeSubscriptions()
        {
            Action<HighlightByCategoryEvent> highlightByCategoryEventAction = _ => this.HighlightEventHandler();

            if (this.AllowMessageBusSubscriptions)
            {
                var highlightSubscription = this.Session.CDPMessageBus.Listen<HighlightEvent>(this.Thing)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.HighlightEventHandler());
                
                this.Disposables.Add(highlightSubscription);

                // category highlighting
                if (this.Thing is ICategorizableThing thingAsCategorizableThing)
                {
                    var highlightCategorySubscription = this.Session.CDPMessageBus.Listen<HighlightByCategoryEvent>()
                        .Where(e => thingAsCategorizableThing.IsMemberOfCategory(e.Category))
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(highlightByCategoryEventAction);

                    this.Disposables.Add(highlightCategorySubscription);
                }
 
                this.Disposables.Add(this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Relationship))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateThingStatus()));
            }
            else
            {
                var highlightObserver = this.Session.CDPMessageBus.Listen<HighlightEvent>();

                this.Disposables.Add(this.MessageBusHandler.GetHandler<HighlightEvent>().RegisterEventHandler(
                    highlightObserver,
                    new MessageBusEventHandlerSubscription<HighlightEvent>(
                        e => e.HighlightedThing == this.Thing,
                        e => this.HighlightEventHandler())));

                // category highlighting
                if (this.Thing is ICategorizableThing thingAsCategorizableThing)
                {
                    var highlightByCategoryObserver = this.Session.CDPMessageBus.Listen<HighlightByCategoryEvent>();

                    this.Disposables.Add(this.MessageBusHandler.GetHandler<HighlightByCategoryEvent>().RegisterEventHandler(
                        highlightByCategoryObserver,
                        new MessageBusEventHandlerSubscription<HighlightByCategoryEvent>(
                            e => thingAsCategorizableThing.IsMemberOfCategory(e.Category), 
                            highlightByCategoryEventAction
                    )));
                }

                var relationshipObserver = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Relationship));

                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(
                    relationshipObserver, 
                    new ObjectChangedMessageBusEventHandlerSubscription(
                        typeof(Relationship), 
                        objectChange => true, 
                        objectChange => this.UpdateThingStatus())));
            }

            this.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(this.ErrorMsg))
                {
                    this.HasError = !string.IsNullOrEmpty(this.ErrorMsg);
                }
            };

            this.Disposables.Add(
                this.WhenAnyValue(x => x.Tooltip).Subscribe(_ => this.UpdateDetails()));
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected virtual void UpdateThingStatus()
        {
        }

        /// <summary>
        /// Compute the rows to remove and to add from a <see cref="IEnumerable{TThing}"/> compared to the <see cref="ContainedRows"/> list
        /// </summary>
        /// <typeparam name="TThing">The type of <see cref="Thing"/> represented by the rows</typeparam>
        /// <param name="currentThings">The current <see cref="IEnumerable{TThing}"/> that shall be represented</param>
        /// <param name="addRowMethod">The method that instantiates and adds the rows to the <see cref="ContainedRows"/> list</param>
        protected void ComputeRows<TThing>(IEnumerable<TThing> currentThings, Action<TThing> addRowMethod)
            where TThing : Thing
        {
            var current = currentThings.ToArray();

            var existingRowThing = this.ContainedRows.Where(x => x.Thing is TThing).Select(x => (TThing) x.Thing)
                .ToArray();
            var newThing = current.Except(existingRowThing);
            var oldThing = existingRowThing.Except(current);

            foreach (var thing in oldThing)
            {
                this.RemoveRow(thing);
            }

            foreach (var thing in newThing)
            {
                addRowMethod(thing);
            }
        }

        /// <summary>
        /// Remove and dispose the row associated to the <paramref name="removedThing"/>
        /// </summary>
        /// <param name="removedThing">The <see cref="Thing"/> which associated row shall be removed</param>
        protected void RemoveRow(Thing removedThing)
        {
            var row = this.ContainedRows.SingleOrDefault(rowViewModel => rowViewModel.Thing == removedThing);
            if (row != null)
            {
                this.ContainedRows.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for highlight of row
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        protected virtual void HighlightEventHandler()
        {
            this.IsHighlighted = true;

            // add a subscription to handle cancel of highlight
            var cancelHighlightSubscription = this.Session.CDPMessageBus.Listen<CancelHighlightEvent>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.CancelHighlightEventHandler());

            this.HighlightCancelDisposables.Add(cancelHighlightSubscription);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for cancel of highlight of row
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        protected virtual void CancelHighlightEventHandler()
        {
            this.IsHighlighted = false;

            foreach (var cancelationSubscription in this.HighlightCancelDisposables)
            {
                cancelationSubscription.Dispose();
            }
        }

        /// <summary>
        /// Write the inline operations to the Data-access-layer
        /// </summary>
        /// <param name="clone">The <see cref="Thing"/> to update</param>
        /// <param name="showConfirmation">A value indicating whether a confirmation should be displayed</param>
        protected async Task DalWrite(Thing clone, bool showConfirmation = false)
        {
            var transactionContext = TransactionContextResolver.ResolveContext(clone);
            var transaction = new ThingTransaction(transactionContext, clone);

            await this.DalWrite(transaction, showConfirmation);
        }

        /// <summary>
        /// Write the inline operations to the Data-access-layer
        /// </summary>
        /// <param name="transaction">The <see cref="ThingTransaction"/> that contains the operations</param>
        /// <param name="showConfirmation">A value indicating whether a confirmation should be displayed</param>
        protected async Task DalWrite(ThingTransaction transaction, bool showConfirmation = false)
        {
            try
            {
                var operationContainer = transaction.FinalizeTransaction();
                await this.Session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                logger.Error("The inline update operation failed: {0}", ex.Message);
                this.ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// Creates, update and write a clone in the data-source when inline-editing with a new value for one of its property
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="fieldName">The property name</param>
        public virtual void CreateCloneAndWrite(object newValue, string fieldName)
        {
            var clone = this.Thing.Clone(false);
            var type = clone.GetType();

            try
            {
                var propInfo = type.GetProperty(fieldName);
                var propertyValue = Convert.ChangeType(newValue, propInfo.PropertyType);
                propInfo.SetValue(clone, propertyValue, null);
                this.EndInlineEdit(clone);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    "Cannot use the generic method to publish the new value. Please override CreateCloneToPublish.", e);
            }
        }

        /// <summary>
        /// Persist the updated <see cref="Thing"/> after an inline edit or revert the change if <see cref="DalWrite"/> fails.
        /// </summary>
        /// <param name="clone">
        /// The updated thing to save.
        /// </param>
        public async void EndInlineEdit(Thing clone)
        {
            var transactionContext = TransactionContextResolver.ResolveContext(clone);
            var transaction = new ThingTransaction(transactionContext, clone);
            await this.DalWrite(transaction);
            if (this.HasError)
            {
                this.ObjectChangeEventHandler(new ObjectChangedEvent(this.Thing));
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
            foreach (var disposable in this.HighlightCancelDisposables)
            {
                disposable.Dispose();
            }

            foreach (var row in this.ContainedRows)
            {
                row.Dispose();
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
            this.UpdateTooltip();
        }

        /// <summary>
        /// Update this <see cref="Tooltip"/>
        /// </summary>
        protected virtual void UpdateTooltip()
        {
            this.Tooltip = this.Thing?.Tooltip();
        }

        /// <summary>
        /// Update this <see cref="Details"/>
        /// </summary>
        protected virtual void UpdateDetails()
        {
            this.Details = this.Thing?.Tooltip();
        }

        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/>
        /// should be set to a value other than <see cref="DragDropEffects.None"/>.
        /// </remarks>
        public virtual void StartDrag(IDragInfo dragInfo)
        {
            logger.Trace("Start drag {0}:{1}", this.Thing, this.Thing.Iid);
            dragInfo.Payload = this.Thing;
            dragInfo.Effects = DragDropEffects.All;
        }

        /// <summary>
        /// Show a confirmation message in the taskbar
        /// </summary>
        protected void ShowConfirmation(string title, string message, NotificationKind notificationKind)
        {
            this.Session.CDPMessageBus.SendMessage(new TaskbarNotificationEvent(title, message, notificationKind));
        }

        /// <summary>
        /// Checks if all the descendant rows are expanded.
        /// </summary>
        /// <returns>True if all the descendants are expanded or if the row has no descendants, false otherwise</returns>
        public bool AllChildRowsExpanded()
        {
            if (!this.IsExpanded)
            {
                return false;
            }

            return !this.ContainedRows.Where(x => { return x.ContainedRows != null && x.ContainedRows.Count > 0; }).Any(x => !x.AllChildRowsExpanded());
        }
    }
}
