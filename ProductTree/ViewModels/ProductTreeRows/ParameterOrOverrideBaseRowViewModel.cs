// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterOrOverrideBaseRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// The row-view-model that represents a <see cref="ParameterOrOverrideBase"/>
    /// </summary>
    public abstract class ParameterOrOverrideBaseRowViewModel : CDP4CommonView.ParameterOrOverrideBaseRowViewModel<ParameterOrOverrideBase>, IModelCodeRowViewModel
    {
        /// <summary>
        /// The current <see cref="ParameterGroup"/>
        /// </summary>
        private ParameterGroup currentGroup;

        /// <summary>
        /// The listener for updates on <see cref="MeasurementScale"/>
        /// </summary>
        private IDisposable measurementScaleListener;

        /// <summary>
        /// Backing field for <see cref="Usage"/>
        /// </summary>
        private ParameterUsageKind usage;

        /// <summary>
        /// Backing field for <see cref="IsPublishable"/>
        /// </summary>
        private bool isPublishable;

        /// <summary>
        /// The cache that stores the listeners that allow this row to react on update on the associated <see cref="Thing"/>
        /// </summary>
        private readonly Dictionary<Thing, IDisposable> valueSetListeners = new Dictionary<Thing, IDisposable>();

        /// <summary>
        /// The state listeners
        /// </summary>
        private readonly List<IDisposable> actualFiniteStateListener;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// The backing field for <see cref="ThingCreator"/>
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOrOverrideBaseRowViewModel"/> class
        /// </summary>
        /// <param name="parameterOrOverride">The <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="option">The current <see cref="Option"/></param>
        /// <param name="session">The current <see cref="ISession"/></param>
        /// <param name="containerViewModel">The row that represents a <see cref="IViewModelBase{T}"/> that contains this row through <see cref="ParameterGroup"/>s or not.</param>
        protected ParameterOrOverrideBaseRowViewModel(ParameterOrOverrideBase parameterOrOverride, Option option, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterOrOverride, session, containerViewModel)
        {
            // initialize the current group
            this.actualFiniteStateListener = new List<IDisposable>();
            this.currentGroup = this.Thing.Group;

            this.IsOptionDependent = this.Thing.IsOptionDependent;
            this.Option = option;
            this.StateDependence = this.Thing.StateDependence;

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the <see cref="IThingCreator"/> that is used to create different <see cref="Things"/>.
        /// </summary>
        public IThingCreator ThingCreator
        {
            get => this.thingCreator = this.thingCreator ?? ServiceLocator.Current.GetInstance<IThingCreator>();
            set => this.thingCreator = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current represented <see cref="ParameterOrOverrideBase"/> is publishable
        /// </summary>
        public bool IsPublishable
        {
            get => this.isPublishable;
            set => this.RaiseAndSetIfChanged(ref this.isPublishable, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="MeasurementScale"/>
        /// </summary>
        public MeasurementScale MeasurementScale { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="ParameterUsageKind"/>
        /// </summary>
        public ParameterUsageKind Usage
        {
            get => this.usage;
            set => this.RaiseAndSetIfChanged(ref this.usage, value);
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
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var parameterTypeListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.ParameterType)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.PopulateValueSetProperty());

            this.Disposables.Add(parameterTypeListener);
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
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
            this.measurementScaleListener.Dispose();

            this.actualFiniteStateListener.ForEach(x => x.Dispose());
            this.actualFiniteStateListener.Clear();

            foreach (var listener in this.valueSetListeners.Values)
            {
                listener.Dispose();
            }

            this.valueSetListeners.Clear();
        }

        /// <summary>
        /// Update the properties of this row. Automatically call on Update of this <see cref="ParameterOrOverrideBaseRowViewModel.Thing"/>
        /// </summary>
        protected void UpdateProperties()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
            this.Value = null;
            this.IsPublishable = false;
            this.UpdateOwnerNameAndShortName();
            this.ModelCode = this.Thing.ModelCode();

            if (this.StateDependence != this.Thing.StateDependence)
            {
                this.StateDependence = this.Thing.StateDependence;
            }

            if ((this.MeasurementScale == null) || (this.MeasurementScale != this.Thing.Scale))
            {
                this.measurementScaleListener?.Dispose();

                this.measurementScaleListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Scale)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.PopulateValueSetProperty());

                this.MeasurementScale = this.Thing.Scale;
            }

            this.SetUsage();
            this.PopulateValueSetProperty();

            if (this.Thing.Group != this.currentGroup)
            {
                // update the position of this row in the parameter-group hierarchy
                ((IParameterRowContainer)this.ContainerViewModel).UpdateParameterBasePosition(this.Thing);
                this.currentGroup = this.Thing.Group;
            }
        }

        /// <summary>
        /// Updates the <see cref="OwnerName"/> and <see cref="OwnerShortName"/> properties of the current view-model
        /// </summary>
        private void UpdateOwnerNameAndShortName()
        {
            if (this.Owner != null)
            {
                this.OwnerName = this.Owner.Name;
                this.OwnerShortName = this.Owner.ShortName;
            }
        }

        /// <summary>
        /// Set the Usage for this <see cref="ParameterOrOverrideBaseRowViewModel"/>
        /// </summary>
        private void SetUsage()
        {
            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out var tuple);

            var subscribeTo = this.Thing.ParameterSubscription.Any(x => x.Owner == tuple.Item1);
            var subscribedByOthers = this.Thing.ParameterSubscription.Any(x => x.Owner != tuple.Item1);

            if (subscribeTo)
            {
                this.Usage = ParameterUsageKind.Subscribed;
            }
            else if (subscribedByOthers)
            {
                this.Usage = ParameterUsageKind.SubscribedByOthers;
            }
            else
            {
                this.Usage = ParameterUsageKind.Unused;
            }
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueSetBase"/> for an <see cref="Option"/> (if this <see cref="ParameterOrOverrideBase"/> is option dependent) and a <see cref="ActualFiniteState"/> (if it is state dependent)
        /// </summary>
        /// <param name="actualState">The <see cref="ActualFiniteState"/></param>
        /// <param name="actualOption">The <see cref="Option"/></param>
        /// <returns>The <see cref="ParameterValueSetBase"/> if a value is defined for the <see cref="Option"/></returns>
        protected abstract ParameterValueSetBase GetValueSet(ActualFiniteState actualState = null, Option actualOption = null);

        /// <summary>
        /// Update the listeners for the <see cref="ParameterValueSetBase"/> of this row
        /// </summary>
        private void UpdateValueSetListeners()
        {
            var currentValueSet = this.valueSetListeners.Keys.OfType<ParameterValueSetBase>().ToList();

            var updatedValueSet = new List<IValueSet>();

            var valueset = this.Thing.IsOptionDependent ? this.Thing.ValueSets.Where(x => x.ActualOption == this.Option) : this.Thing.ValueSets;
            updatedValueSet.AddRange(valueset);

            var addedValueSet = updatedValueSet.Except(currentValueSet);
            var removedValueSet = currentValueSet.Except(updatedValueSet);

            foreach (Thing parameterValueSet in addedValueSet)
            {
                var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterValueSet)
                    .Where(objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.PopulateValueSetProperty());

                this.valueSetListeners.Add(parameterValueSet, listener);
            }

            foreach (Thing parameterValueSet in removedValueSet)
            {
                if (this.valueSetListeners.TryGetValue(parameterValueSet, out var listener))
                {
                    listener.Dispose();
                }

                this.valueSetListeners.Remove(parameterValueSet);
            }
        }

        /// <summary>
        /// Populates the values. Clear the value-set rows and repopulate them.
        /// </summary>
        private void PopulateValueSetProperty()
        {
            var actualOption = this.IsOptionDependent ? this.Option : null;
            this.Name = this.Thing.ParameterType.Name;
            this.IsPublishable = false; // reset the status

            this.ContainedRows.ClearAndDispose();

            this.UpdateValueSetListeners();

            // Not State-dependent 
            if (this.StateDependence == null)
            {
                this.PopulateStateIndependentValues(actualOption);
            }
            else
            {
                // State dependent
                this.PopulateStateDependentValues(actualOption);
            }
        }

        /// <summary>
        /// Populate the row if this <see cref="ParameterOrOverrideBase"/> is state-independent
        /// </summary>
        /// <param name="actualOption">The <see cref="Option"/></param>
        private void PopulateStateIndependentValues(Option actualOption)
        {
            // get the single ParameterValueSetBase
            var valueSet = this.GetValueSet(actualOption: actualOption);
            this.IsPublishable = this.CheckValuesPublishabledStatus(valueSet);

            if ((valueSet == null) && !this.IsOptionDependent)
            {
                logger.Warn("The value set of Parameter or override {0} is null for a option and state independent. it should not happen.", this.Thing.Iid);

                return;
            }

            // Scalar Type -> Set value to this row
            if (this.Thing.ParameterType is ScalarParameterType)
            {
                if (valueSet != null)
                {
                    this.SetScalarValue(valueSet);
                }
            }
            else
            {
                // Compound -> set value to the component row
                var compoundType = (CompoundParameterType)this.Thing.ParameterType;

                var index = 0;

                foreach (var component in compoundType.Component.SortedItems)
                {
                    if (!(component.Value.ParameterType is ScalarParameterType))
                    {
                        throw new NotImplementedException("Compound of Compound have yet to be implemented.");
                    }

                    var row = new ParameterTypeComponentRowViewModel(component.Value, this.Session, this);
                    this.ContainedRows.Add(row);

                    if (valueSet != null)
                    {
                        row.SetScalarValue(this.Thing, valueSet, index);
                        index++;
                    }
                }
            }
        }

        /// <summary>
        /// Create or remove a row representing an <see cref="ActualFiniteState"/>
        /// </summary>
        /// <param name="actualOption">The actual option</param>
        /// <param name="actualState">The actual state</param>
        private void UpdateActualStateRow(Option actualOption, ActualFiniteState actualState)
        {
            var existingRow = this.ContainedRows.OfType<ParameterStateRowViewModel>()
                .SingleOrDefault(x => x.ActualState == actualState);

            if (actualState.Kind == ActualFiniteStateKind.FORBIDDEN)
            {
                if (existingRow != null)
                {
                    this.ContainedRows.RemoveAndDispose(existingRow);
                }

                return;
            }

            // mandatory state
            if (existingRow != null)
            {
                return;
            }

            var valueSet = this.GetValueSet(actualState, actualOption);

            if (valueSet == null)
            {
                logger.Error("product tree: The valueset is null for an actualFiniteState. it should not happen.");

                return;
            }

            var stateRow = new ParameterStateRowViewModel(this.Thing, actualState, this.Session, this);
            this.ContainedRows.Add(stateRow);
            var isStatePublishable = this.CheckValuesPublishabledStatus(valueSet);
            this.IsPublishable = this.IsPublishable || isStatePublishable;

            var compoundType = this.Thing.ParameterType as CompoundParameterType;

            if (compoundType == null)
            {
                stateRow.SetScalarValue(valueSet);
                stateRow.IsPublishable = isStatePublishable;
            }
            else
            {
                stateRow.IsPublishable = isStatePublishable;

                // create nested state row
                var index = 0;

                foreach (var component in compoundType.Component.SortedItems)
                {
                    if (!(component.Value.ParameterType is ScalarParameterType))
                    {
                        throw new NotImplementedException("Compound of Compound have yet to be implemented");
                    }

                    var componentrow = new ParameterTypeComponentRowViewModel(component.Value, this.Session, this);
                    stateRow.ContainedRows.Add(componentrow);

                    componentrow.SetScalarValue(this.Thing, valueSet, index);
                    index++;
                }
            }
        }

        /// <summary>
        /// Populate the row if this <see cref="ParameterOrOverrideBase"/> is state-dependent
        /// </summary>
        /// <param name="actualOption">The <see cref="Option"/></param>
        private void PopulateStateDependentValues(Option actualOption)
        {
            this.actualFiniteStateListener.ForEach(x => x.Dispose());
            this.actualFiniteStateListener.Clear();

            foreach (var state in this.Thing.StateDependence.ActualState)
            {
                var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(state)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => this.UpdateActualStateRow(actualOption, state));

                this.actualFiniteStateListener.Add(listener);
            }

            this.Thing.StateDependence.ActualState.Sort(new ActualFiniteStateComparer());

            foreach (var state in this.Thing.StateDependence.ActualState.Where(x => x.Kind == ActualFiniteStateKind.MANDATORY))
            {
                this.UpdateActualStateRow(actualOption, state);
            }
        }

        /// <summary>
        /// Set the value of this row in case of the <see cref="ParameterType"/> is a <see cref="ScalarParameterType"/>
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase"/> containing the value</param>
        private void SetScalarValue(ParameterValueSetBase valueSet)
        {
            // perform checks to see if this is indeed a scalar value
            if (valueSet.Published.Count() > 1)
            {
                logger.Warn("The value set of Parameter or override {0} is marked as Scalar, yet has multiple values.", this.Thing.Iid);
            }

            this.Value = valueSet.Published.FirstOrDefault();

            // handle zero values returned
            if (this.Value == null)
            {
                logger.Warn("The value set of Parameter or override {0} is marked as Scalar, yet has no values.", this.Thing.Iid);
                this.Value = "-";
            }

            if (this.Thing.Scale != null)
            {
                this.Value += " [" + this.Thing.Scale.ShortName + "]";
            }

            this.Switch = valueSet.ValueSwitch;
        }

        /// <summary>
        /// Check if the current <see cref="ParameterValueSetBase"/> is publishable (the published and actual values are different)
        /// </summary>
        /// <param name="valueset">The <see cref="ParameterValueSetBase"/></param>
        /// <returns>True if the Published value is different from the Actual value</returns>
        private bool CheckValuesPublishabledStatus(ParameterValueSetBase valueset)
        {
            if (valueset == null)
            {
                return false;
            }

            try
            {
                for (var i = 0; i < valueset.QueryParameterType().NumberOfValues; i++)
                {
                    if (valueset.Published[i] != valueset.ActualValue[i])
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (ArgumentOutOfRangeException e)
            {
                logger.Error($"The ParameterValueSetBase {valueset.Iid} has an incorrect number of values");

                return false;
            }
        }

        /// <summary>
        /// Create a <see cref="BinaryRelationship"/> between this expression and a <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        protected async Task CreateBinaryRelationship(ParameterOrOverrideBase parameter, RelationalExpression relationalExpression)
        {
            if (this.Thing?.GetContainerOfType<Iteration>() is Iteration iteration)
            {
                await this.ThingCreator.CreateBinaryRelationshipForRequirementVerification(this.Session, iteration, parameter, relationalExpression);
            }
        }
    }
}
