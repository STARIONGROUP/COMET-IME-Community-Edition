// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterBaseRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Builders;
    using CDP4Composition.Extensions;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Microsoft.Extensions.Logging;

    using ReactiveUI;

    /// <summary>
    /// The Base row-class for <see cref="ParameterBase"/>
    /// </summary>
    /// <typeparam name="T">A <see cref="ParameterBase"/> type</typeparam>
    public abstract class ParameterBaseRowViewModel<T> : CDP4CommonView.ParameterBaseRowViewModel<T>, IValueSetRow, IHaveModelCode where T : ParameterBase
    {
        /// <summary>
        /// The current <see cref="ParameterGroup"/>
        /// </summary>
        private ParameterGroup currentGroup;

        /// <summary>
        /// Backing field for <see cref="Formula"/>
        /// </summary>
        private string formula;

        /// <summary>
        /// The value-set listeners cache
        /// </summary>
        protected List<IDisposable> valueSetListener;

        /// <summary>
        /// The state listeners
        /// </summary>
        private readonly List<IDisposable> actualFiniteStateListener;

        /// <summary>
        /// A value indicating whether this <see cref="ParameterBase"/> is editable in the current context
        /// </summary>
        private readonly bool isParameterBaseReadOnlyInDataContext;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Backing field for <see cref="Category"/>
        /// </summary>
        private IEnumerable<Category> category;

        /// <summary>
        /// Backing field for <see cref="DisplayCategory"/>
        /// </summary>
        private string displayCategory;

        /// <summary>
        /// Backing field for <see cref="LoggerFactory"/> 
        /// </summary>
        private ILoggerFactory loggerFactory;

        /// <summary>
        /// The INJECTED <see cref="ILoggerFactory"/> 
        /// </summary>
        protected ILoggerFactory LoggerFactory
        {
            get
            {
                if (this.loggerFactory == null)
                {
                    this.loggerFactory = ServiceLocator.Current.GetInstance<ILoggerFactory>();
                }

                return this.loggerFactory;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBaseRowViewModel{T}"/> class. 
        /// </summary>
        /// <param name="parameterBase">
        /// The associated <see cref="ParameterBase"/>
        /// </param>
        /// <param name="session">
        /// The associated <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The <see cref="ElementBase{T}"/> row that contains this row.
        /// </param>
        /// <param name="isReadOnly">
        /// A value indicating whether this row shall be made read-only in the current context.
        /// </param>
        protected ParameterBaseRowViewModel(T parameterBase, ISession session, IViewModelBase<Thing> containerViewModel, bool isReadOnly)
            : base(parameterBase, session, containerViewModel)
        {
            this.isParameterBaseReadOnlyInDataContext = isReadOnly;
            this.IsCompoundType = this.Thing.ParameterType is CompoundParameterType;
            this.currentGroup = this.Thing.Group;
            this.ParameterType = this.Thing.ParameterType;
            this.ParameterTypeClassKind = this.Thing.ParameterType.ClassKind;
            this.valueSetListener = new List<IDisposable>();
            this.actualFiniteStateListener = new List<IDisposable>();
            this.UpdateModelCode();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the owner listener
        /// </summary>
        protected KeyValuePair<DomainOfExpertise, IDisposable> OwnerListener { get; set; }

        /// <summary>
        /// Gets the parent element, or usages Category
        /// </summary>
        public IEnumerable<Category> Category
        {
            get => this.category;
            private set => this.RaiseAndSetIfChanged(ref this.category, value);
        }

        /// <summary>
        /// Gets the Categories in display format
        /// </summary>
        public string DisplayCategory
        {
            get => this.displayCategory;
            protected set => this.RaiseAndSetIfChanged(ref this.displayCategory, value);
        }

        /// <summary>
        /// Gets the model-code
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            private set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ParameterType"/> of this <see cref="Parameter"/> is a <see cref="EnumerationParameterType"/>
        /// </summary>
        public bool IsMultiSelect
        {
            get
            {
                var enumPt = this.ParameterType as EnumerationParameterType;
                return enumPt == null ? false : enumPt.AllowMultiSelect;
            }
        }

        /// <summary>
        /// Gets the list of possible <see cref="EnumerationValueDefinition"/> for this <see cref="Parameter"/>
        /// </summary>
        public ReactiveList<EnumerationValueDefinition> EnumerationValueDefinition
        {
            get
            {
                var enumValues = new ReactiveList<EnumerationValueDefinition>();

                var enumPt = this.ParameterType as EnumerationParameterType;

                if (enumPt != null)
                {
                    enumValues.AddRange(enumPt.ValueDefinition);
                }

                return enumValues;
            }
        }

        /// <summary>
        /// Gets or sets the Formula column value
        /// </summary>
        public string Formula
        {
            get => this.formula;
            set => this.RaiseAndSetIfChanged(ref this.formula, value);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ParameterBase"/> is a <see cref="CompoundParameterType"/>
        /// </summary>
        public bool IsCompoundType { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="HasExcludes"/>. Property implemented here to fix binding errors.
        /// </summary>
        public bool? HasExcludes => null;

        /// <summary>
        /// Gets the value indicating whether the row is a top element. Property implemented here to fix binding errors.
        /// </summary>
        public bool IsTopElement => false;

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> represented by this <see cref="IValueSetRow"/>
        /// </summary>
        public ClassKind ParameterTypeClassKind { get; protected set; }

        /// <summary>
        /// Sets the values of this row in case where the <see cref="ParameterBase"/> is neither option-dependent nor state-dependent and is a <see cref="ScalarParameterType"/>
        /// </summary>
        public abstract void SetProperties();

        /// <summary>
        /// Create subscription to listen to updates of the value sets
        /// </summary>
        protected abstract void CreateValueSetsSubscription();

        /// <summary>
        /// Set the owner listener
        /// </summary>
        protected abstract void SetOwnerListener();

        /// <summary>
        /// Computes the entire row or specific property of the row is editable based on the
        /// result of the <see cref="PermissionService.CanWrite"/> method and potential
        /// conditions of the property of the Row that is being edited.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property for which the value is computed. This allows to include the
        /// specific property of the row-view-model in the computation. If the propertyname is empty
        /// then the whole row is taken into account. If a property is specified only that property
        /// is taken into account.
        /// </param>
        /// <returns>
        /// True if the row or more specific the property is editable or not.
        /// </returns>
        public override bool IsEditable(string propertyName = "")
        {
            if (this.Thing.ParameterType is SampledFunctionParameterType)
            {
                return false;
            }

            return !this.isParameterBaseReadOnlyInDataContext && base.IsEditable(propertyName);
        }

        /// <summary>
        /// Initializes the subscription of this row
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;

            Action<ObjectChangedEvent> action = x =>
            {
                this.UpdateProperties();
                this.UpdateModelCode();
            };

            if (this.AllowMessageBusSubscriptions)
            {
                var parameterTypeListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.ParameterType)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.Disposables.Add(parameterTypeListener);
            }
            else
            {
                var parameterTypeObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterType));

                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(parameterTypeObserver, new ObjectChangedMessageBusEventHandlerSubscription(this.Thing.ParameterType, discriminator, action)));
            }

            this.SetOwnerListener();
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
        public override string ValidateProperty(string columnName, object newValue)
        {
            if (columnName == "Manual" || columnName == "Reference")
            {
                return ParameterValueValidator.Validate(newValue, this.ParameterType, this.Thing.Scale, this.LoggerFactory);
            }

            return null;
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
            this.UpdateModelCode();
        }

        /// <summary>
        /// Dispose of the listeners
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.valueSetListener.ForEach(x => x.Dispose());
            this.actualFiniteStateListener.ForEach(x => x.Dispose());
            this.OwnerListener.Value?.Dispose();
            this.actualFiniteStateListener.Clear();
            this.valueSetListener.Clear();
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
        }

        /// <summary>
        /// Update the model code property of itself and all contained rows recursively
        /// </summary>
        public void UpdateModelCode()
        {
            this.ModelCode = this.Thing.ModelCode();

            foreach (var containedRow in this.ContainedRows)
            {
                var modelCodeRow = containedRow as IHaveModelCode;
                modelCodeRow?.UpdateModelCode();
            }
        }

        /// <summary>
        /// Update this ParameterBase row and its child nodes
        /// </summary>
        /// <remarks>
        /// if the represented <see cref="ParameterBase"/> is updated, repopulate the contained rows
        /// </remarks>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();
            this.Name = this.Thing.ParameterType.Name;
            this.UpdateCategories();

            this.ClearValues();

            // clear the listener on the unique value set represented
            foreach (var listener in this.valueSetListener)
            {
                listener.Dispose();
            }

            this.valueSetListener.Clear();

            // clear the children and repopulate
            foreach (var row in this.ContainedRows)
            {
                row.Dispose();
            }

            this.ContainedRows.ClearAndDispose();

            if (this.Thing.IsOptionDependent)
            {
                this.SetOptionProperties();
                this.CreateValueSetsSubscription();
            }
            else if (this.Thing.StateDependence != null)
            {
                this.actualFiniteStateListener.ForEach(x => x.Dispose());
                this.actualFiniteStateListener.Clear();

                this.SetStateProperties(this, null);
                this.CreateValueSetsSubscription();
            }
            else if (this.IsCompoundType)
            {
                this.SetComponentProperties(this, null, null);
                this.CreateValueSetsSubscription();
            }
            else
            {
                this.SetProperties();
            }

            // update the group-row under which this row shall be displayed
            if (this.currentGroup != this.Thing.Group)
            {
                this.currentGroup = this.Thing.Group;
                var elementBaseRow = this.ContainerViewModel as IElementBaseRowViewModel;

                if (elementBaseRow != null)
                {
                    elementBaseRow.UpdateParameterBasePosition(this.Thing);
                }
            }
        }

        /// <summary>
        /// Formats the categories for this view model to a single string
        /// </summary>
        /// <returns>The string formatted categories</returns>
        private void UpdateCategories()
        {
            var builder = new CategoryStringBuilder()
                .AddCategories("PT", this.Thing.ParameterType.Category);

            var elementBase = this.ContainerViewModel.FindThingFromContainerViewModelHierarchy<ElementBase>();

            if (elementBase != null)
            {
                if (elementBase is ElementUsage elementUsage)
                {
                    builder.AddCategories("EU", elementBase.Category);
                    builder.AddCategories("ED", elementUsage.ElementDefinition.Category);
                }
                else
                {
                    builder.AddCategories("ED", elementBase.Category);
                }
            }

            this.Category = builder.GetCategories();
            this.DisplayCategory = builder.Build();
        }

        /// <summary>
        /// Sets the option dependent rows contained in this row.
        /// </summary>
        private void SetOptionProperties()
        {
            var iteration = this.Thing.GetContainerOfType<Iteration>();

            if (iteration == null)
            {
                throw new InvalidOperationException("No Iteration Container was found.");
            }

            if (this.Thing.StateDependence != null)
            {
                this.actualFiniteStateListener.ForEach(x => x.Dispose());
                this.actualFiniteStateListener.Clear();
            }

            var newRows = new List<IRowViewModelBase<Thing>>();

            foreach (Option availableOption in iteration.Option)
            {
                var row = new ParameterOptionRowViewModel(this.Thing, availableOption, this.Session, this, this.isParameterBaseReadOnlyInDataContext);

                if (this.Thing.StateDependence != null)
                {
                    this.SetStateProperties(row, availableOption);
                }
                else if (this.IsCompoundType)
                {
                    this.SetComponentProperties(row, availableOption, null);
                }
                else
                {
                    row.SetValues();
                }

                newRows.Add(row);
            }

            this.ContainedRows.AddRange(newRows);
        }

        /// <summary>
        /// Create or remove a row representing an <see cref="ActualFiniteState"/>
        /// </summary>
        /// <param name="row">The row container for the rows to create or remove</param>
        /// <param name="actualOption">The actual option</param>
        /// <param name="actualState">The actual state</param>
        private void UpdateActualStateRow(IRowViewModelBase<Thing> row, Option actualOption, ActualFiniteState actualState, IList<IRowViewModelBase<Thing>> parentRow)
        {
            if (actualState.Kind == ActualFiniteStateKind.FORBIDDEN)
            {
                var rowToRemove =
                    row.ContainedRows.OfType<ParameterStateRowViewModel>()
                        .SingleOrDefault(x => x.ActualState == actualState);

                if (rowToRemove != null)
                {
                    row.ContainedRows.RemoveAndDispose(rowToRemove);
                }

                return;
            }

            // mandatory state
            var existingRow = row.ContainedRows.OfType<ParameterStateRowViewModel>()
                .SingleOrDefault(x => x.ActualState == actualState);

            if (existingRow != null)
            {
                return;
            }

            var stateRow = new ParameterStateRowViewModel(this.Thing, actualOption, actualState, this.Session, row, this.isParameterBaseReadOnlyInDataContext);

            if (this.Thing.ParameterType is CompoundParameterType)
            {
                this.SetComponentProperties(stateRow, actualOption, actualState);
            }
            else
            {
                stateRow.SetValues();
            }

            parentRow.Add(stateRow);
        }

        /// <summary>
        /// Initialize the listeners and process the state-dependency of this <see cref="ParameterBase"/>
        /// </summary>
        /// <param name="row">The row container</param>
        /// <param name="actualOption">The actual option</param>
        private void SetStateProperties(IRowViewModelBase<Thing> row, Option actualOption)
        {
            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;

            Action<ObjectChangedEvent> action = x =>
            {
                this.UpdateActualStateRow(row, actualOption, x.ChangedThing as ActualFiniteState, row.ContainedRows);
                this.UpdateModelCode();
            };

            if (this.AllowMessageBusSubscriptions)
            {
                foreach (var state in this.Thing.StateDependence.ActualState)
                {
                    var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(state)
                        .Where(discriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(action);

                    this.actualFiniteStateListener.Add(listener);
                }
            }
            else
            {
                var stateObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ActualFiniteState));

                foreach (var state in this.Thing.StateDependence.ActualState)
                {
                    this.actualFiniteStateListener.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(stateObserver, new ObjectChangedMessageBusEventHandlerSubscription(state, discriminator, action)));
                }
            }

            this.StateDependence.ActualState.Sort(new ActualFiniteStateComparer());
            var actualFiniteStates = this.StateDependence.ActualState.Where(x => x.Kind == ActualFiniteStateKind.MANDATORY);

            var newRows = new List<IRowViewModelBase<Thing>>();

            foreach (var state in actualFiniteStates)
            {
                this.UpdateActualStateRow(row, actualOption, state, newRows);
            }

            row.ContainedRows.AddRange(newRows);
        }

        /// <summary>
        /// Creates the component rows for this <see cref="CompoundParameterType"/> <see cref="ParameterRowViewModel"/>.
        /// </summary>
        private void SetComponentProperties(IRowViewModelBase<Thing> row, Option actualOption, ActualFiniteState actualState)
        {
            var rows = new List<IRowViewModelBase<Thing>>();

            for (var i = 0; i < ((CompoundParameterType)this.Thing.ParameterType).Component.Count; i++)
            {
                var componentRow = new ParameterComponentValueRowViewModel(this.Thing, i, this.Session, actualOption, actualState, row, this.isParameterBaseReadOnlyInDataContext);
                componentRow.SetValues();
                rows.Add(componentRow);
            }

            row.ContainedRows.AddRange(rows);
        }

        /// <summary>
        /// Clear the values
        /// </summary>
        private void ClearValues()
        {
            this.Manual = null;
            this.Reference = null;
            this.Value = null;
            this.Formula = null;
            this.Computed = null;
            this.Switch = null;
            this.Scale = null;
            this.Published = null;
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for highlight of row
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        protected override void HighlightEventHandler()
        {
            base.HighlightEventHandler();

            if (this.TopContainerViewModel is ElementDefinitionsBrowserViewModel browser)
            {
                browser.ChangeFocusOnThing(this.Thing);
            }
        }
    }
}
