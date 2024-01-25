// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterStateRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4ProductTree.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services.NestedElementTreeService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The row-view-model representing an ActualFiniteState of a <see cref="Parameter"/> 
    /// </summary>
    /// <remarks>
    /// This row shall be used when a <see cref="Parameter"/> is state dependent
    /// </remarks>
    public class ParameterStateRowViewModel : CDP4CommonView.ParameterBaseRowViewModel<ParameterBase>, IHavePath
    {
        /// <summary>
        /// Backing field for <see cref="IsPublishable"/> property.
        /// </summary>
        private bool isPublishable;

        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// The <see cref="INestedElementTreeService"/>
        /// </summary>
        private readonly INestedElementTreeService nestedElementTreeService = ServiceLocator.Current.GetInstance<INestedElementTreeService>();

        /// <summary>
        /// Backing field for <see cref="ActualState"/>
        /// </summary>
        private ActualFiniteState actualState;

        /// <summary>
        /// ParameterValueSetBase used to set the ModelCode
        /// </summary>
        private ParameterValueSetBase valueSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterStateRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">The associated value-set of a <see cref="ParameterBase"/></param>
        /// <param name="actualFiniteState">The <see cref="ActualFiniteState"/> represented</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParameterStateRowViewModel(ParameterBase parameterBase, ActualFiniteState actualFiniteState, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterBase, session, containerViewModel)
        {
            this.IsPublishable = false;
            this.ActualState = actualFiniteState;
            this.IsDefault = this.ActualState.IsDefault;
            this.Name = this.ActualState.Name;

            if (containerViewModel is ParameterOrOverrideBaseRowViewModel parameterOrOverrideBaseRowViewModel)
            {
                this.Option = parameterOrOverrideBaseRowViewModel.Option;
            }

            this.InitializeOptionSubscriptions();
        }

        /// <summary>
        /// Gets or sets the <see cref="ActualFiniteState"/> of this row
        /// </summary>
        public ActualFiniteState ActualState
        {
            get => this.actualState;
            private set => this.RaiseAndSetIfChanged(ref this.actualState, value);
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
        /// Update the model code property of itself and all contained rows recursively
        /// </summary>
        public void UpdateModelCode()
        {
            this.ModelCode = this.valueSet?.ModelCode();

            foreach (var containedRow in this.ContainedRows)
            {
                var modelCodeRow = containedRow as IHaveModelCode;
                modelCodeRow?.UpdateModelCode();
            }
        }

        /// <summary>
        /// Calculates the Path
        /// </summary>
        public string GetPath()
        {
            return this.nestedElementTreeService.GetNestedParameterPath(this.Thing, this.Option, this.ActualState);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current represented <see cref="ParameterValueSetBase"/> is publishable
        /// </summary>
        public bool IsPublishable
        {
            get => this.isPublishable;
            set => this.RaiseAndSetIfChanged(ref this.isPublishable, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ActualFiniteState"/> associated with this row is the default value of the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public bool IsDefault
        {
            get => this.isDefault;
            set => this.RaiseAndSetIfChanged(ref this.isDefault, value);
        }

        /// <summary>
        /// Initializes the <see cref="Option"/> related subscriptions
        /// </summary>
        private void InitializeOptionSubscriptions()
        {
            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;

            Action<ObjectChangedEvent> action = x =>
            {
                this.Name = this.ActualState.Name;
                this.IsDefault = this.ActualState.IsDefault;
                this.UpdateModelCode();
            };

            if (this.AllowMessageBusSubscriptions)
            {
                foreach (var possibleFiniteState in this.ActualState.PossibleState)
                {
                    var stateListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(possibleFiniteState)
                        .Where(discriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(action);

                    this.Disposables.Add(stateListener);
                }
            }
            else
            {
                var possibleFiniteStateObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(PossibleFiniteState));

                foreach (var possibleFiniteState in this.ActualState.PossibleState)
                {
                    this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(possibleFiniteStateObserver, new ObjectChangedMessageBusEventHandlerSubscription(possibleFiniteState, discriminator, action)));
                }
            }
        }

        /// <summary>
        /// Set the value for this row from the associated <see cref="ParameterValueSetBase"/>
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase"/></param>
        public void SetScalarValue(ParameterValueSetBase valueSet)
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

            // propagate values to aid in modelcode updates
            this.valueSet = valueSet;

            this.UpdateModelCode();
        }

        /// <summary>
        /// Set the value of this row in case of the <see cref="ParameterType"/> is a <see cref="SampledFunctionParameterType"/>
        /// </summary>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase"/> containing the value</param>
        public void SetSampledFunctionValue(ParameterValueSetBase valueSet)
        {
            // perform checks to see if this is indeed a scalar value
            if (valueSet.Published.Count() < 2)
            {
                logger.Warn("The value set of Parameter or override {0} is marked as SampledFunction, yet has less than 2 values.", this.Thing.Iid);
            }

            this.Switch = valueSet.ValueSwitch;

            var samplesFunctionParameterType = this.Thing.ParameterType as SampledFunctionParameterType;

            if (samplesFunctionParameterType == null)
            {
                logger.Warn("ParameterType mismatch, in {0} is marked as SampledFunction, yet cannot be converted.", this.Thing.Iid);
                this.Value = "-";

                return;
            }

            var cols = samplesFunctionParameterType.NumberOfValues;

            this.Value = $"[{valueSet.Published.Count / cols}x{cols}]";
        }
    }
}
