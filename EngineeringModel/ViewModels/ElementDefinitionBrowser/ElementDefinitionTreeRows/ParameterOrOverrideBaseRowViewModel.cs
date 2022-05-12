// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterOrOverrideBaseRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
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
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Validation;

    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// The row representing a <see cref="ParameterOrOverrideBase"/>
    /// </summary>
    public abstract class ParameterOrOverrideBaseRowViewModel : ParameterBaseRowViewModel<ParameterOrOverrideBase>
    {
        /// <summary>
        /// The active participant.
        /// </summary>
        protected Participant activeParticipant;

        /// <summary>
        /// Backing field for <see cref="IsPublishable"/>
        /// </summary>
        private bool isPublishable;

        /// <summary>
        /// The backing field for <see cref="ThingCreator"/>
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOrOverrideBaseRowViewModel"/> class
        /// </summary>
        /// <param name="parameterOrOverrideBase">
        /// The associated <see cref="ParameterOrOverrideBase"/>
        /// </param>
        /// <param name="session">
        /// The associated <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The container Row.
        /// </param>
        /// <param name="isReadOnly">
        /// A value indicating whether this row shall be made read-only in the current context.
        /// </param>
        protected ParameterOrOverrideBaseRowViewModel(ParameterOrOverrideBase parameterOrOverrideBase, ISession session, IViewModelBase<Thing> containerViewModel, bool isReadOnly)
            : base(parameterOrOverrideBase, session, containerViewModel, isReadOnly)
        {
            var engineeringModel = (EngineeringModel)this.Thing.TopContainer;
            this.activeParticipant = engineeringModel.GetActiveParticipant(this.Session.ActivePerson);
            this.SetOwnerValue();
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
        /// Gets a value indicating whether this <see cref="ParameterOrOverrideBase"/> has publishable values
        /// </summary>
        public bool IsPublishable
        {
            get => this.isPublishable;
            private set => this.RaiseAndSetIfChanged(ref this.isPublishable, value);
        }

        /// <summary>
        /// Sets the values of this row in case where the <see cref="ParameterOrOverrideBase"/> is neither option-dependent nor state-dependent and is a <see cref="ScalarParameterType"/>
        /// </summary>
        public override void SetProperties()
        {
            var valueset = this.GetValueSet();

            if (valueset == null)
            {
                this.LogNoValueSetError();

                return;
            }

            this.SetProperties(valueset);
            this.CheckPublishabledStatus();

            if (this.valueSetListener.Any())
            {
                return;
            }

            Func<ObjectChangedEvent, bool> discriminator = 
                objectChange => (objectChange.EventKind == EventKind.Updated) && (objectChange.ChangedThing.RevisionNumber > this.RevisionNumber);
            Action<ObjectChangedEvent> action = x => this.SetProperties();

            if (this.AllowMessageBusSubscriptions)
            {
                var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(valueset)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.valueSetListener.Add(listener);
            }
            else
            {
                var parameterValueSetObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterValueSetBase));
                this.valueSetListener.Add(
                    this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(parameterValueSetObserver, new ObjectChangedMessageBusEventHandlerSubscription(valueset, discriminator, action)));
            }
        }

        /// <summary>
        /// Creates a clone of the edited <see cref="ParameterValueSetBase"/> and writes it to the data-source.
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="fieldName">The property name</param>
        public override void CreateCloneAndWrite(object newValue, string fieldName)
        {
            var valueset = this.GetValueSet();

            if (valueset == null)
            {
                return;
            }

            var clone = valueset.Clone(false);
            this.UpdateValueSets(clone);
            this.EndInlineEdit(clone);
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
        /// Create subscription to listen to updates of the value sets
        /// </summary>
        /// <remarks>
        /// These subscriptions are necessary to update the publishable status of this row
        /// </remarks>
        protected override void CreateValueSetsSubscription()
        {
            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;
            Action<ObjectChangedEvent> action = x => this.CheckPublishabledStatus();

            if (this.AllowMessageBusSubscriptions)
            {
                foreach (var valueSet in this.Thing.ValueSets)
                {
                    var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(valueSet)
                        .Where(discriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(action);

                    this.valueSetListener.Add(listener);
                }
            }
            else
            {
                var parameterValueSetObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(ParameterValueSetBase));
                foreach (var valueSet in this.Thing.ValueSets.OfType<ParameterValueSetBase>())
                {
                    this.valueSetListener.Add(
                        this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(parameterValueSetObserver, new ObjectChangedMessageBusEventHandlerSubscription(valueSet, discriminator, action)));
                }            
            }
        }

        /// <summary>
        /// Set the listener for the owner
        /// </summary>
        protected override void SetOwnerListener()
        {
            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;
            Action<ObjectChangedEvent> action = x =>
                        {
                            this.OwnerName = this.Thing.Owner.Name;
                            this.OwnerShortName = this.Thing.Owner.ShortName;
                        };

            IDisposable listener;

            if (this.AllowMessageBusSubscriptions)
            {
                listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Owner)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);
            }
            else
            {
                var ownerObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise));
                listener = this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(ownerObserver, new ObjectChangedMessageBusEventHandlerSubscription(this.Thing.Owner, discriminator, action));
            }

            this.OwnerListener = new KeyValuePair<DomainOfExpertise, IDisposable>(this.Thing.Owner, listener);
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        /// <remarks>
        /// Refreshes the container view-model of this row, i.e. <see cref="ElementDefinitionRowViewModel"/> or <see cref="ElementUsageRowViewModel"/> to re-draw its rows
        /// </remarks>
        private void UpdateProperties()
        {
            this.SetOwnerValue();

            // refresh the container row if this is replaced by a subscription
            this.Session.OpenIterations.TryGetValue(this.Thing.GetContainerOfType<Iteration>(), out var tuple);

            if (this.Thing.ParameterSubscription.Any(x => x.Owner == tuple.Item1))
            {
                this.RefreshContainerRows();
            }
        }

        /// <summary>
        /// Set the owner value
        /// </summary>
        private void SetOwnerValue()
        {
            if (this.Owner != null)
            {
                this.OwnerName = this.Owner.Name;
                this.OwnerShortName = this.Owner.ShortName;
            }

            if (this.OwnerListener.Key != this.Thing.Owner)
            {
                this.OwnerListener.Value?.Dispose();
                this.SetOwnerListener();
            }
        }

        /// <summary>
        /// Refresh the rows contained by the container of this row.
        /// </summary>
        private void RefreshContainerRows()
        {
            var containerUsageRow = this.ContainerViewModel as ElementUsageRowViewModel;

            if (containerUsageRow == null)
            {
                if (this.ContainerViewModel is ElementDefinitionRowViewModel elementDefinitionRow)
                {
                    elementDefinitionRow.UpdateChildren();
                }

                return;
            }

            containerUsageRow.UpdateChildren();
        }

        /// <summary>
        /// Gets the single <see cref="ParameterValueSetBase"/> (not dependent, not state dependent)
        /// </summary>
        /// <returns>The <see cref="ParameterValueSetBase"/></returns>
        private ParameterValueSetBase GetValueSet()
        {
            return (ParameterValueSetBase)this.Thing.ValueSets.FirstOrDefault();
        }

        /// <summary>
        /// Sets the properties of this <see cref="ParameterOrOverrideBaseRowViewModel"/> from a <see cref="ParameterValueSetBase"/>.
        /// </summary>
        /// <param name="valueSet">
        /// The <see cref="ParameterValueSetBase"/>.
        /// </param>
        private void SetProperties(ParameterValueSetBase valueSet)
        {
            if (this.ContainedRows.Count == 0)
            {
                this.ScaleShortName = this.Thing.Scale == null ? "-" : this.Thing.Scale.ShortName;
            }

            if (this.Thing.ValueSets.Count() > 1)
            {
                this.Value = string.Empty;
                this.Formula = string.Empty;
                this.Computed = string.Empty;
                this.Manual = string.Empty;
                this.Reference = string.Empty;
                this.Published = string.Empty;

                this.Switch = null;
                this.State = string.Empty;
                this.Option = null;
            }
            else if (this.Thing.ParameterType is SampledFunctionParameterType samplesFunctionParameterType)
            {
                var cols = samplesFunctionParameterType.NumberOfValues;

                this.Computed = $"[{valueSet.Computed.Count / cols}x{cols}]";
                this.Manual = $"[{valueSet.Manual.Count / cols}x{cols}]";
                this.Reference = $"[{valueSet.Reference.Count / cols}x{cols}]";
                this.Value = $"[{valueSet.ActualValue.Count / cols}x{cols}]";
                this.Formula = $"[{valueSet.Formula.Count / cols}x{cols}]";
                this.Published = $"[{valueSet.Published.Count / cols}x{cols}]";
            }
            else
            {
                this.Value = this.GetStringDisplayFromValueSet(valueSet.ActualValue, true);
                this.Formula = this.GetStringDisplayFromValueSet(valueSet.Formula, false);
                this.Computed = this.GetStringDisplayFromValueSet(valueSet.Computed, true);
                this.Manual = this.GetObjectDisplayFromValueSet(valueSet.Manual);
                this.Reference = this.GetObjectDisplayFromValueSet(valueSet.Reference);
                this.Published = this.GetStringDisplayFromValueSet(valueSet.Published, true);

                this.Switch = valueSet.ValueSwitch;
                this.State = valueSet.ActualState == null ? "-" : valueSet.ActualState.ShortName;
                this.Option = valueSet.ActualOption;
            }
        }

        /// <summary>
        /// Returns a value from a valueset that is usefull for edittable values
        /// </summary>
        /// <param name="valueArray">The <see cref="ValueArray{string}"/></param>
        /// <returns>ValueSet value as an object, corresponding to the correct <see cref="ParameterType"/>, which is handled by a template selector</returns>
        private object GetObjectDisplayFromValueSet(ValueArray<string> valueArray)
        {
            if (valueArray.Count > 1)
            {
                return null;
            }

            if (valueArray.Count == 1)
            {
                return valueArray.First().ToValueSetObject(this.ParameterType);
            }

            return ValueSetConverter.DefaultObject(this.ParameterType);
        }

        /// <summary>
        /// Returns a value from a valueset that is usefull to display in the UI
        /// </summary>
        /// <param name="valueArray">The <see cref="ValueArray{string}"/></param>
        /// <param name="valueConformsToParameterType">States that the value must be compliant with the <see cref="ParameterType"/> value format.</param>
        /// <returns>ValueSet value as a string, corresponding to the correct <see cref="ParameterType"/></returns>
        private string GetStringDisplayFromValueSet(ValueArray<string> valueArray, bool valueConformsToParameterType)
        {
            if (valueArray.Count > 1)
            {
                return string.Empty;
            }

            if (valueArray.Count == 1)
            {
                var value = valueArray.First();

                if (valueConformsToParameterType)
                {
                    return value.ToValueSetString(this.ParameterType);
                }

                return value;
            }

            return ValueValidator.DefaultValue;
        }

        /// <summary>
        /// Check if the current <see cref="ParameterValueSetBase"/> is publishable (the published and actual values are different)
        /// </summary>
        private void CheckPublishabledStatus()
        {
            foreach (ParameterValueSetBase parameterValueSetBase in this.Thing.ValueSets)
            {
                try
                {
                    for (var i = 0; i < parameterValueSetBase.QueryParameterType().NumberOfValues; i++)
                    {
                        if (parameterValueSetBase.Published[i] != parameterValueSetBase.ActualValue[i])
                        {
                            this.IsPublishable = true;

                            return;
                        }
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    logger.Error($"The ParameterValueSetBase {parameterValueSetBase.Iid} has an incorrect number of values");
                }
            }

            this.IsPublishable = false;
        }

        /// <summary>
        /// The logs the error if the <see cref="Thing"/> is a <see cref="Parameter"/> or a <see cref="ParameterOverride"/> without value set.
        /// </summary>
        private void LogNoValueSetError()
        {
            var elementBase = (ElementBase)this.Thing.Container;

            logger.Error(
                "No value set found for the {0} {1} with id: {2} contained in the {3} {4}",
                this.Thing.ClassKind,
                this.Thing.ParameterType.Name,
                this.Thing.Iid,
                elementBase.ClassKind,
                elementBase.Name);
        }

        /// <summary>
        /// The update component values.
        /// </summary>
        /// <param name="valueSet">
        /// The value set.
        /// </param>
        public void UpdateValueSets(ParameterValueSetBase valueSet)
        {
            var actualOption = valueSet.ActualOption;
            var actualState = valueSet.ActualState;

            if (actualOption != null)
            {
                var optionRow = this.ContainedRows.Cast<ParameterOptionRowViewModel>().Single(x => x.ActualOption == actualOption);

                if (actualState != null)
                {
                    if (actualState.Kind != ActualFiniteStateKind.FORBIDDEN)
                    {
                        var actualStateRow = optionRow.ContainedRows.Cast<ParameterStateRowViewModel>().Single(x => x.ActualState == actualState);
                        this.UpdateScalarOrCompoundValueSet(valueSet, actualStateRow);
                    }
                }
                else
                {
                    this.UpdateScalarOrCompoundValueSet(valueSet, optionRow);
                }
            }
            else
            {
                if (actualState != null)
                {
                    if (actualState.Kind != ActualFiniteStateKind.FORBIDDEN)
                    {
                        var actualStateRow = this.ContainedRows.Cast<ParameterStateRowViewModel>().Single(x => x.ActualState == actualState);
                        this.UpdateScalarOrCompoundValueSet(valueSet, actualStateRow);
                    }
                }
                else
                {
                    this.UpdateScalarOrCompoundValueSet(valueSet);
                }
            }
        }

        /// <summary>
        /// Call the correct update method depending on kind of parameter type (scalar, compound)
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase"/> to update</param>
        /// <param name="row">The <see cref="ParameterValueBaseRowViewModel"/> containing the information, or if null the current row</param>
        private void UpdateScalarOrCompoundValueSet(ParameterValueSetBase valueSet, ParameterValueBaseRowViewModel row = null)
        {
            if (this.IsCompoundType)
            {
                this.UpdateCompoundValueSet(valueSet, row);
            }
            else
            {
                this.UpdateScalarValueSet(valueSet, row);
            }
        }

        /// <summary>
        /// Update value-set for a scalar parameter.
        /// </summary>
        /// <param name="valueSet">The value set to update</param>
        /// <param name="row">The value row containing the information. If null the data is retrieved from the current row.</param>
        /// <remarks>
        /// If <paramref name="row"/> is null, it means the parameter is not compound, not option dependent and not state dependent.
        /// </remarks>
        private void UpdateScalarValueSet(ParameterValueSetBase valueSet, ParameterValueBaseRowViewModel row = null)
        {
            valueSet.ValueSwitch = row?.Switch ?? this.Switch.Value;
            valueSet.Computed = row == null ? new ValueArray<string>(new List<string> { this.Computed }) : new ValueArray<string>(new List<string> { row.Computed });
            valueSet.Manual = row == null ? new ValueArray<string>(new List<string> { this.Manual.ToValueSetString(this.ParameterType) }) : new ValueArray<string>(new List<string> { row.Manual.ToValueSetString(this.ParameterType) });
            valueSet.Reference = row == null ? new ValueArray<string>(new List<string> { this.Reference.ToValueSetString(this.ParameterType) }) : new ValueArray<string>(new List<string> { row.Reference.ToValueSetString(this.ParameterType) });
        }

        /// <summary>
        /// Update value-set for a compound parameter.
        /// </summary>
        /// <param name="valueSet">The value set to update</param>
        /// <param name="row">The value row containing the information. If null the data is retrieved from the current row.</param>
        /// <remarks>
        /// If <paramref name="row"/> is null, it means the parameter is not compound, not option dependent and not state dependent.
        /// </remarks>
        private void UpdateCompoundValueSet(ParameterValueSetBase valueSet, ParameterValueBaseRowViewModel row = null)
        {
            var componentRows = row == null
                ? this.ContainedRows.Cast<ParameterComponentValueRowViewModel>().ToList()
                : row.ContainedRows.Cast<ParameterComponentValueRowViewModel>().ToList();

            valueSet.Computed = new ValueArray<string>(new string[componentRows.Count]);
            valueSet.Manual = new ValueArray<string>(new string[componentRows.Count]);
            valueSet.Reference = new ValueArray<string>(new string[componentRows.Count]);
            valueSet.ValueSwitch = componentRows[0].Switch.Value; // all the switches should have the same value

            for (var i = 0; i < componentRows.Count; i++)
            {
                valueSet.Computed[i] = componentRows[i].Computed;
                valueSet.Manual[i] = componentRows[i].Manual.ToValueSetString(this.ParameterType);
                valueSet.Reference[i] = componentRows[i].Reference.ToValueSetString(this.ParameterType);
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
