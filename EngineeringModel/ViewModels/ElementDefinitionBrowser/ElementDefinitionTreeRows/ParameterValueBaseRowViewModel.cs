// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueBaseRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The base row view-model that displays the value-set of a <see cref="ParameterBase"/> 
    /// when its type is not a <see cref="ScalarParameterType"/> and it is not option and state dependent
    /// </summary>
    public abstract class ParameterValueBaseRowViewModel : CDP4CommonView.ParameterBaseRowViewModel<ParameterBase>, IValueSetRow, IHaveModelCode
    {
        /// <summary>
        /// Backing field for <see cref="Formula"/>
        /// </summary>
        private string formula;

        /// <summary>
        /// The Index of the <see cref="ValueArray"/> in which the value is contained
        /// </summary>
        protected readonly int ValueIndex;

        /// <summary>
        /// The <see cref="Option"/> associated with this row if the <see cref="ParameterBase"/> is option-dependent
        /// </summary>
        public readonly Option ActualOption;

        /// <summary>
        /// The <see cref="ActualFiniteState"/> associated with this row if the <see cref="ParameterBase"/> is state-dependent
        /// </summary>
        public readonly ActualFiniteState ActualState;

        /// <summary>
        /// A value that indicates whether the value set's listener is initialized
        /// </summary>
        private bool isValueSetListenerInitialized;

        /// <summary>
        /// Backing field for <see cref="IsPublishable"/>
        /// </summary>
        private bool isPublishable;

        /// <summary>
        /// A value indicating whether this <see cref="ParameterBase"/> is editable in the current context
        /// </summary>
        private readonly bool isParameterBaseReadOnlyInDataContext;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValueBaseRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">
        /// The associated <see cref="ParameterBase"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/>
        /// </param>
        /// <param name="actualOption">
        /// The actual <see cref="Option"/> represented if any
        /// </param>
        /// <param name="actualState">
        /// The actual <see cref="ActualFiniteState"/> represented if any
        /// </param>
        /// <param name="containerRow">
        /// The row container
        /// </param>
        /// <param name="valueIndex">
        /// The index of the component if applicable
        /// </param>
        /// <param name="isReadOnly">
        /// A value indicating whether the row is read-only
        /// </param>
        protected ParameterValueBaseRowViewModel(ParameterBase parameterBase, ISession session, Option actualOption, ActualFiniteState actualState, IViewModelBase<Thing> containerRow, int valueIndex = 0, bool isReadOnly = false)
            : base(parameterBase, session, containerRow)
        {
            this.isParameterBaseReadOnlyInDataContext = isReadOnly;
            this.ActualOption = actualOption;
            this.ActualState = actualState;

            this.ValueIndex = valueIndex;
            this.ParameterTypeClassKind = this.Thing.ParameterType.ClassKind;

            this.UpdateThingStatus();
            this.SetOwnerValue();
        }

        /// <summary>
        /// Gets the model-code
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            protected set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// Update the model code property of itself and all contained rows recursively
        /// </summary>
        public void UpdateModelCode()
        {
            IValueSet valueSet = null;

            if (this.Thing is Parameter)
            {
                valueSet = this.GetParameterValueSet();
            }
            else if (this.Thing is ParameterOverride)
            {
                valueSet = this.GetParameterOverrideValueSet();
            }
            else if (this.Thing is ParameterSubscription)
            {
                valueSet = this.GetParameterSubscriptionValueSet();
            }

            if (valueSet == null)
            {
                logger.Error("No Value set was found for the option: {0}, state: {1}", this.ActualOption == null ? "null" : this.ActualOption.Name, this.ActualState == null ? "null" : this.ActualState.Name);
                return;
            }

            this.ModelCode = valueSet.ModelCode(this.ValueIndex);

            foreach (var containedRow in this.ContainedRows)
            {
                var modelCodeRow = containedRow as IHaveModelCode;
                modelCodeRow?.UpdateModelCode();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ParameterBase"/> is publishable
        /// </summary>
        public bool IsPublishable
        {
            get => this.isPublishable;
            private set => this.RaiseAndSetIfChanged(ref this.isPublishable, value);
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> represented by this <see cref="IValueSetRow"/>
        /// </summary>
        public ClassKind ParameterTypeClassKind { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ParameterType"/> of this <see cref="Parameter"/> is a <see cref="EnumerationParameterType"/>
        /// </summary>
        public bool IsMultiSelect
        {
            get
            {
                var enumPt = this.Thing.ParameterType as EnumerationParameterType;

                if (enumPt != null)
                {
                    return enumPt.AllowMultiSelect;
                }

                var cpt = this.Thing.ParameterType as CompoundParameterType;

                if (cpt == null)
                {
                    return false;
                }

                enumPt = cpt.Component[this.ValueIndex].ParameterType as EnumerationParameterType;

                if (enumPt == null)
                {
                    return false;
                }

                return enumPt.AllowMultiSelect;
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

                if (this.Thing == null)
                {
                    return enumValues;
                }

                var enumPt = this.Thing.ParameterType as EnumerationParameterType;

                if (enumPt != null)
                {
                    enumValues.AddRange(enumPt.ValueDefinition);
                    return enumValues;
                }

                var cpt = this.Thing.ParameterType as CompoundParameterType;

                if (cpt != null)
                {
                    enumPt = cpt.Component[this.ValueIndex].ParameterType as EnumerationParameterType;

                    if (enumPt != null)
                    {
                        enumValues.AddRange(enumPt.ValueDefinition);
                    }
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
        /// Set the Values of this row
        /// </summary>
        public virtual void SetValues()
        {
            if (this.Thing is ParameterSubscription)
            {
                this.SetParameterSubscriptionValues();
            }
            else
            {
                this.SetParameterOrOverrideValues();
                this.UpdateIsPublishableStatus();
            }

            this.ScaleShortName = this.Thing.Scale == null ? "-" : this.Thing.Scale.ShortName;
            this.UpdateThingStatus();
        }

        /// <summary>
        /// Creates a clone to write it on the data-source when inline-editing with a new value for one of its property
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="fieldName">The property name</param>
        public override void CreateCloneAndWrite(object newValue, string fieldName)
        {
            Thing clone;

            if (this.Thing.ClassKind == ClassKind.Parameter || this.Thing.ClassKind == ClassKind.ParameterOverride)
            {
                var valueset = this.Thing.ClassKind == ClassKind.Parameter ? (ParameterValueSetBase)this.GetParameterValueSet() : this.GetParameterOverrideValueSet();

                if (valueset == null)
                {
                    return;
                }

                clone = valueset.Clone(false);
                this.UpdateValueSet((ParameterValueSetBase)clone);
            }
            else
            {
                // subscription
                var valueset = this.GetParameterSubscriptionValueSet();

                if (valueset == null)
                {
                    return;
                }

                clone = valueset.Clone(false);
                this.UpdateValueSet((ParameterSubscriptionValueSet)clone);
            }

            this.EndInlineEdit(clone);
        }

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
            // turn off inline editing for SampledFunctionParameter
            if (this.Thing.ParameterType is SampledFunctionParameterType)
            {
                return false;
            }

            if (this.isParameterBaseReadOnlyInDataContext)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(this.Error))
            {
                return false;
            }

            if (this.Thing is ParameterSubscription && propertyName == "Reference")
            {
                return false;
            }

            return base.IsEditable(propertyName);
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
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
                var parameterType = this.ParameterType;
                var scale = this.Thing.Scale;

                return ParameterValueValidator.Validate(newValue, parameterType, scale);
            }

            return null;
        }

        /// <summary>
        /// Sets the values of this row in the case where the represented thing is a <see cref="ParameterSubscription"/>
        /// </summary>
        private void SetParameterSubscriptionValues()
        {
            var parameterSubscription = this.Thing as ParameterSubscription;

            if (parameterSubscription == null)
            {
                return;
            }

            if (this.ContainedRows.Any())
            {
                return;
            }

            var valueSet = this.GetParameterSubscriptionValueSet();

            if (valueSet == null)
            {
                logger.Error("No Value set was found for the option: {0}, state: {1}", this.ActualOption == null ? "null" : this.ActualOption.Name, this.ActualState == null ? "null" : this.ActualState.Name);
                return;
            }

            this.AddValueSetListener(valueSet);

            this.Switch = valueSet.ValueSwitch;
            this.Computed = valueSet.Computed.Count() > this.ValueIndex ? valueSet.Computed[this.ValueIndex] : "-";
            this.Manual = valueSet.Manual.Count() > this.ValueIndex ? valueSet.Manual[this.ValueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Reference = valueSet.Reference.Count() > this.ValueIndex ? valueSet.Reference[this.ValueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Value = valueSet.ActualValue.Count() > this.ValueIndex ? valueSet.ActualValue[this.ValueIndex] : "-";
            this.UpdateModelCode();
        }

        /// <summary>
        /// Sets the values of this row in the case where the represented thing is a <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        private void SetParameterOrOverrideValues()
        {
            if (this.ContainedRows.Any())
            {
                return;
            }

            ParameterValueSetBase valueSet;

            if (this.Thing is Parameter)
            {
                valueSet = this.GetParameterValueSet();
            }
            else
            {
                valueSet = this.GetParameterOverrideValueSet();
            }

            if (valueSet == null)
            {
                logger.Error("No Value set was found for the option: {0}, state: {1}", this.ActualOption == null ? "null" : this.ActualOption.Name, this.ActualState == null ? "null" : this.ActualState.Name);
                return;
            }

            this.AddValueSetListener(valueSet);

            if (this.Thing.ParameterType is SampledFunctionParameterType samplesFunctionParameterType)
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
                this.Computed = valueSet.Computed.Count() > this.ValueIndex ? valueSet.Computed[this.ValueIndex] : "-";
                this.Manual = valueSet.Manual.Count() > this.ValueIndex ? valueSet.Manual[this.ValueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
                this.Reference = valueSet.Reference.Count() > this.ValueIndex ? valueSet.Reference[this.ValueIndex].ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
                this.Value = valueSet.ActualValue.Count() > this.ValueIndex ? valueSet.ActualValue[this.ValueIndex] : "-";
                this.Formula = valueSet.Formula.Count() > this.ValueIndex ? valueSet.Formula[this.ValueIndex] : "-";
                this.Published = valueSet.Published.Count() > this.ValueIndex ? valueSet.Published[this.ValueIndex] : "-";
            }

            this.State = valueSet.ActualState != null ? valueSet.ActualState.Name : "-";
            this.Option = valueSet.ActualOption;
            this.Switch = valueSet.ValueSwitch;

            if (valueSet.Published.Count() <= this.ValueIndex)
            {
                this.Error = "No ValueSet found for this component";
            }

            this.UpdateModelCode();
        }

        /// <summary>
        /// Gets the <see cref="ParameterOverrideValueSet"/> of this <see cref="ParameterOverride"/> if applicable
        /// </summary>
        /// <returns>The <see cref="ParameterOverrideValueSet"/></returns>
        private ParameterOverrideValueSet GetParameterOverrideValueSet()
        {
            var parameterOverride = (ParameterOverride)this.Thing;
            ParameterOverrideValueSet valueSet = null;

            if (this.ActualOption == null && this.ActualState == null)
            {
                return parameterOverride.ValueSet.FirstOrDefault();
            }

            if (this.ActualOption != null && this.ActualState == null)
            {
                valueSet = parameterOverride.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption);
            }

            if (this.ActualOption == null && this.ActualState != null)
            {
                valueSet = parameterOverride.ValueSet.FirstOrDefault(v => v.ActualState == this.ActualState);
            }

            if (this.ActualOption != null && this.ActualState != null)
            {
                valueSet = parameterOverride.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption && v.ActualState == this.ActualState);
            }

            return valueSet;
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueSet"/> of this <see cref="Parameter"/> if applicable
        /// </summary>
        /// <returns>The <see cref="ParameterValueSet"/></returns>
        private ParameterValueSet GetParameterValueSet()
        {
            var parameter = (Parameter)this.Thing;

            if (this.ActualOption == null && this.ActualState == null)
            {
                return parameter.ValueSet.FirstOrDefault();
            }

            if (this.ActualOption != null && this.ActualState == null)
            {
                return parameter.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption);
            }

            if (this.ActualOption == null && this.ActualState != null)
            {
                return parameter.ValueSet.FirstOrDefault(v => v.ActualState == this.ActualState);
            }

            return parameter.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption && v.ActualState == this.ActualState);
        }

        /// <summary>
        /// Gets the <see cref="ParameterSubscriptionValueSet"/> of this <see cref="ParameterSubscription"/>
        /// </summary>
        /// <returns>The <see cref="ParameterSubscriptionValueSet"/></returns>
        private ParameterSubscriptionValueSet GetParameterSubscriptionValueSet()
        {
            var parameterSubscription = (ParameterSubscription)this.Thing;

            ParameterSubscriptionValueSet valueSet;

            if (this.ActualOption == null && this.ActualState == null)
            {
                valueSet = parameterSubscription.ValueSet.FirstOrDefault();
            }
            else if (this.ActualOption != null && this.ActualState == null)
            {
                valueSet = parameterSubscription.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption);
            }
            else if (this.ActualOption == null && this.ActualState != null)
            {
                valueSet = parameterSubscription.ValueSet.FirstOrDefault(v => v.ActualState == this.ActualState);
            }
            else
            {
                valueSet = parameterSubscription.ValueSet.FirstOrDefault(v => v.ActualOption == this.ActualOption && v.ActualState == this.ActualState);
            }

            return valueSet;
        }

        /// <summary>
        /// Add the value set update listener for this row
        /// </summary>
        /// <param name="thing">The <see cref="ParameterValueSetBase"/> which updates need to be handled in this row</param>
        private void AddValueSetListener(ParameterValueSetBase thing)
        {
            if (this.isValueSetListenerInitialized)
            {
                return;
            }

            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber;
            Action<ObjectChangedEvent> action = x => this.SetValues();

            if (this.AllowMessageBusSubscriptions)
            {
                var listener = this.CDPMessageBus.Listen<ObjectChangedEvent>(thing)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.Disposables.Add(listener);
            }
            else
            {
                var parameterValueSetBaseObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterValueSetBase));
                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(parameterValueSetBaseObserver, new ObjectChangedMessageBusEventHandlerSubscription(thing, discriminator, action)));
            }

            this.isValueSetListenerInitialized = true;
        }

        /// <summary>
        /// Add the value set update listener for this row
        /// </summary>
        /// <param name="thing">The <see cref="ParameterSubscriptionValueSet"/> which updates need to be handled in this row</param>
        private void AddValueSetListener(ParameterSubscriptionValueSet thing)
        {
            if (this.isValueSetListenerInitialized)
            {
                return;
            }

            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber;
            Action<ObjectChangedEvent> action = x => this.SetValues();

            if (this.AllowMessageBusSubscriptions)
            {
                var listener = this.CDPMessageBus.Listen<ObjectChangedEvent>(thing)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.Disposables.Add(listener);

                var subscribedListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(thing.SubscribedValueSet)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.Disposables.Add(subscribedListener);
            }
            else
            {
                var parameterSubscriptionValueSetObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterSubscriptionValueSet));
                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(parameterSubscriptionValueSetObserver, new ObjectChangedMessageBusEventHandlerSubscription(thing, discriminator, action)));

                var parameterValueSetObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(ParameterValueSetBase));
                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(parameterValueSetObserver, new ObjectChangedMessageBusEventHandlerSubscription(thing.SubscribedValueSet, discriminator, action)));
            }

            this.isValueSetListenerInitialized = true;
        }

        /// <summary>
        /// Update the clone of the <see cref="ParameterValueSetBase"/> represented by this row
        /// </summary>
        /// <param name="valueset">The clone of the <see cref="ParameterValueSetBase"/> to update</param>
        private void UpdateValueSet(ParameterValueSetBase valueset)
        {
            if (this.ContainerViewModel is ParameterValueBaseRowViewModel parameterValueBaseRow)
            {
                parameterValueBaseRow.UpdateValueSet(valueset);
            }

            if (this.ContainerViewModel is ParameterOrOverrideBaseRowViewModel parameterOrOverrideRow)
            {
                parameterOrOverrideRow.UpdateValueSets(valueset);
            }
        }

        /// <summary>
        /// Update the clone of the <see cref="ParameterSubscriptionValueSet"/> represented by this row
        /// </summary>
        /// <param name="valueset">The clone of the <see cref="ParameterSubscriptionValueSet"/> to update</param>
        private void UpdateValueSet(ParameterSubscriptionValueSet valueset)
        {
            if (this.ContainerViewModel is ParameterValueBaseRowViewModel parameterValueBaseRow)
            {
                parameterValueBaseRow.UpdateValueSet(valueset);
            }

            if (this.ContainerViewModel is ParameterSubscriptionRowViewModel parameterSubscriptionRow)
            {
                parameterSubscriptionRow.UpdateValueSets(valueset);
            }
        }

        /// <summary>
        /// Update the <see cref="IsPublishable"/> value
        /// </summary>
        private void UpdateIsPublishableStatus()
        {
            if (this.ContainedRows.Any())
            {
                this.IsPublishable = false;
                return;
            }

            this.IsPublishable = this.Published != this.Value;
        }

        /// <summary>
        /// Set the value of the owner for this row
        /// </summary>
        private void SetOwnerValue()
        {
            var subscription = this.Thing as ParameterSubscription;
            IDisposable listener = null;

            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;

            Action<ObjectChangedEvent> action = x =>
            {
                this.OwnerName = ((DomainOfExpertise)x.ChangedThing).Name;
                this.OwnerShortName = ((DomainOfExpertise)x.ChangedThing).ShortName;
            };

            if (subscription != null)
            {
                var parameterOrOverride = (ParameterOrOverrideBase)subscription.Container;

                if (parameterOrOverride.Owner != null)
                {
                    this.OwnerName = "[" + parameterOrOverride.Owner.Name + "]";
                    this.OwnerShortName = "[" + parameterOrOverride.Owner.ShortName + "]";
                }

                if (this.AllowMessageBusSubscriptions)
                {
                    listener = this.CDPMessageBus.Listen<ObjectChangedEvent>(parameterOrOverride.Owner)
                        .Where(discriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(action);
                }
                else
                {
                    var ownerObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise));

                    listener =
                        this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(ownerObserver, new ObjectChangedMessageBusEventHandlerSubscription(parameterOrOverride.Owner, discriminator, action));
                }
            }
            else
            {
                if (this.Owner != null)
                {
                    this.OwnerName = this.Owner.Name;
                    this.OwnerShortName = this.Owner.ShortName;

                    if (this.AllowMessageBusSubscriptions)
                    {
                        listener = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.Owner)
                            .Where(discriminator)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(action);
                    }
                    else
                    {
                        var ownerObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise));

                        listener =
                            this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(ownerObserver, new ObjectChangedMessageBusEventHandlerSubscription(this.Thing.Owner, discriminator, action));
                    }
                }
            }

            if (listener != null)
            {
                this.Disposables.Add(listener);
            }
        }
    }
}
