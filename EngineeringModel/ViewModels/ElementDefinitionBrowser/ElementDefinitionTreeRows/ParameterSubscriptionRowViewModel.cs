// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The row representing a <see cref="ParameterSubscription"/> in the Element Definition browser
    /// </summary>
    public class ParameterSubscriptionRowViewModel : ParameterBaseRowViewModel<ParameterSubscription>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionRowViewModel"/> class
        /// </summary>
        /// <param name="parameterSubscription">
        /// The associated <see cref="ParameterSubscription"/>
        /// </param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">the container view-model</param>
        public ParameterSubscriptionRowViewModel(ParameterSubscription parameterSubscription, ISession session, IViewModelBase<Thing> containerViewModel, bool isReadOnly)
            : base(parameterSubscription, session, containerViewModel, isReadOnly)
        {
            this.UpdateOwnerValue();
        }

        #endregion

        /// <summary>
        /// Initializes the extra listeners needed for this row
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            var parameterOrOverride = (ParameterOrOverrideBase)this.Thing.Container;
            var listener =
                CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterOrOverride)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(x => { this.ObjectChangeEventHandler(new ObjectChangedEvent(this.Thing)); this.UpdateOwnerValue(); });
            this.Disposables.Add(listener);

            var parameterOverride = parameterOrOverride as ParameterOverride;
            if (parameterOverride != null)
            {
                var parameterListener =
                    CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterOverride.Parameter)
                        .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(x => this.ObjectChangeEventHandler(new ObjectChangedEvent(this.Thing)));
                this.Disposables.Add(parameterListener);
            }
        }

        /// <summary>
        /// Set the owner listener
        /// </summary>
        protected override void SetOwnerListener()
        {
            var parameterOrOverride = (ParameterOrOverrideBase)this.Thing.Container;
            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterOrOverride.Owner)
                                   .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                                   .ObserveOn(RxApp.MainThreadScheduler)
                                   .Subscribe(x => { this.OwnerName = ((DomainOfExpertise)x.ChangedThing).Name; this.OwnerShortName = ((DomainOfExpertise)x.ChangedThing).ShortName; });

            this.OwnerListener = new KeyValuePair<DomainOfExpertise, IDisposable>(parameterOrOverride.Owner, listener);
        }

        /// <summary>
        /// Sets the values of this row in case where the <see cref="ParameterSubscription"/> is neither option-dependent nor state-dependent and is a <see cref="ScalarParameterType"/>
        /// </summary>
        public override void SetProperties()
        {
            var valueset = this.Thing.ValueSet.FirstOrDefault();

            if (valueset == null)
            {
                logger.Error("No value set was found for parameter subscription {0}", this.Thing.Iid);
                return;
            }

            this.Value = valueset.ActualValue.Any() ? valueset.ActualValue.First() : "-";
            if (this.ContainedRows.Count == 0)
            {
                this.ScaleShortName = this.Thing.Scale == null ? "-" : this.Thing.Scale.ShortName;
            }
            this.Switch = valueset.ValueSwitch;
            this.Computed = valueset.Computed.Any() ? valueset.Computed.First() : "-";
            this.Published = this.Computed;
            this.Manual = valueset.Manual.Any() ? valueset.Manual.First().ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Reference = valueset.Reference.Any() ? valueset.Reference.First().ToValueSetObject(this.ParameterType) : ValueSetConverter.DefaultObject(this.ParameterType);
            this.Formula = valueset.SubscribedValueSet.Formula.Any() ? valueset.SubscribedValueSet.Formula.First() : "-";

            if (this.valueSetListener.Any())
            {
                return;
            }

            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(valueset)
                            .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(_ => this.SetProperties());
            this.valueSetListener.Add(listener);

            var subscribedListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(valueset.SubscribedValueSet)
                            .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                            .ObserveOn(RxApp.MainThreadScheduler)
                            .Subscribe(_ => this.SetProperties());
            this.valueSetListener.Add(subscribedListener);
        }

        /// <summary>
        /// Create subscription to listen to updates of the value sets
        /// </summary>
        protected override void CreateValueSetsSubscription()
        {
            // no subscription needed for this row
        }

        /// <summary>
        /// Creates a clone of the edited <see cref="ParameterSubscriptionValueSet"/> and writes it to the data-source.
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <param name="fieldName">The property name</param>
        public override void CreateCloneAndWrite(object newValue, string fieldName)
        {
            var valueset = this.Thing.ValueSet.FirstOrDefault();
            if (valueset == null)
            {
                return;
            }

            var clone = valueset.Clone(false);
            this.UpdateValueSets(clone);
            this.EndInlineEdit(clone);
        }

        /// <summary>
        /// Update the <see cref="ParameterSubscriptionValueSet"/> with the row values
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterSubscriptionValueSet"/> to update</param>
        public void UpdateValueSets(ParameterSubscriptionValueSet valueSet)
        {
            var actualOption = valueSet.ActualOption;
            var actualState = valueSet.ActualState;

            if (actualOption != null)
            {
                var optionRow = this.ContainedRows.Cast<ParameterOptionRowViewModel>().Single(x => x.ActualOption == actualOption);
                if (actualState != null)
                {
                    var actualStateRow = optionRow.ContainedRows.Cast<ParameterStateRowViewModel>().Single(x => x.ActualState == actualState);
                    this.RedirectUpdateCall(valueSet, actualStateRow);
                }
                else
                {
                    this.RedirectUpdateCall(valueSet, optionRow);
                }
            }
            else
            {
                if (actualState != null)
                {
                    var actualStateRow = this.ContainedRows.Cast<ParameterStateRowViewModel>().Single(x => x.ActualState == actualState);
                    this.RedirectUpdateCall(valueSet, actualStateRow);
                }
                else
                {
                    this.RedirectUpdateCall(valueSet);
                }
            }
        }

        #region Update value sets Methods
        /// <summary>
        /// Call the correct update method depending on kind of parameter type (scalar, compound)
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterSubscriptionValueSet"/> to update</param>
        /// <param name="row">The <see cref="ParameterValueBaseRowViewModel"/> containing the information, or if null the current row</param>
        private void RedirectUpdateCall(ParameterSubscriptionValueSet valueSet, ParameterValueBaseRowViewModel row = null)
        {
            if (this.IsCompoundType)
            {
                this.UpdateCompoundValueSet(valueSet, row);
            }
            else
            {
                this.UpdateSimpleValueSet(valueSet, row);
            }
        }

        /// <summary>
        /// Update value-set for a not-compound parameter.
        /// </summary>
        /// <param name="valueSet">The value set to update</param>
        /// <param name="row">The value row containing the information. If null the data is retrieved from the current row.</param>
        /// <remarks>
        /// If <paramref name="row"/> is null, it means the parameter is not compound, not option dependent and not state dependent.
        /// </remarks>
        private void UpdateSimpleValueSet(ParameterSubscriptionValueSet valueSet, ParameterValueBaseRowViewModel row = null)
        {
            valueSet.ValueSwitch = row == null ? this.Switch.Value : row.Switch.Value;
            valueSet.Manual = row == null ? new ValueArray<string>(new List<string> { ValueSetConverter.ToValueSetString(this.Manual, this.ParameterType) }) : new ValueArray<string>(new List<string> { ValueSetConverter.ToValueSetString(row.Manual, this.ParameterType) });
        }

        /// <summary>
        /// Update value-set for a compound parameter.
        /// </summary>
        /// <param name="valueSet">The value set to update</param>
        /// <param name="row">The value row containing the information. If null the data is retrieved from the current row.</param>
        /// <remarks>
        /// If <paramref name="row"/> is null, it means the parameter is not compound, not option dependent and not state dependent.
        /// </remarks>
        private void UpdateCompoundValueSet(ParameterSubscriptionValueSet valueSet, ParameterValueBaseRowViewModel row = null)
        {
            var componentRows = (row == null)
                ? this.ContainedRows.Cast<ParameterComponentValueRowViewModel>().ToList()
                : row.ContainedRows.Cast<ParameterComponentValueRowViewModel>().ToList();

            valueSet.Manual = new ValueArray<string>(new string[componentRows.Count]);
            valueSet.ValueSwitch = componentRows[0].Switch.Value; // all the switches should have the same value
            for (var i = 0; i < componentRows.Count; i++)
            {
                valueSet.Manual[i] = componentRows[i].Manual.ToValueSetString(this.ParameterType);
            }
        }

        /// <summary>
        /// Update the owner value
        /// </summary>
        private void UpdateOwnerValue()
        {
            var parameter = (ParameterOrOverrideBase)this.Thing.Container;
            if (parameter.Owner != null)
            {
                this.OwnerName = "[" + parameter.Owner.Name + "]";
                this.OwnerShortName = "[" + parameter.Owner.ShortName + "]";
            }

            if (this.OwnerListener.Key != parameter.Owner)
            {
                this.OwnerListener.Value.Dispose();
                this.SetOwnerListener();
            }
        }
        #endregion

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
            if (propertyName == "Reference")
            {
                return false;
            }
            
            return base.IsEditable(propertyName);
        }
    }
}