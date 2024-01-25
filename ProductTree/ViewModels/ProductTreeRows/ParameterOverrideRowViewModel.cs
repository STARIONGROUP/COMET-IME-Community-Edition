// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideRowViewModel.cs" company="RHEA System S.A.">
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
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// row-view-model that represent a <see cref="ParameterOverride"/> in the product tree
    /// </summary>
    public class ParameterOverrideRowViewModel : ParameterOrOverrideBaseRowViewModel, IDropTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideRowViewModel"/> class
        /// </summary>
        /// <param name="parameterOverride">The associated <see cref="ParameterOverride"/></param>
        /// <param name="option">The actual <see cref="Option"/></param>
        /// <param name="session">The current <see cref="ISession"/></param>
        /// <param name="containerViewModel">the container view-model that contains this row</param>
        public ParameterOverrideRowViewModel(ParameterOverride parameterOverride, Option option, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterOverride, option, session, containerViewModel)
        {
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            var parameterOverride = (ParameterOverride)this.Thing;

            IDisposable listener;

            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;

            Action<ObjectChangedEvent> action = x =>
            {
                this.UpdateProperties();
                this.UpdateModelCode();
            };

            if (this.AllowMessageBusSubscriptions)
            {
                listener = this.CDPMessageBus.Listen<ObjectChangedEvent>(parameterOverride.Parameter)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);
            }
            else
            {
                var parameterObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Parameter));

                listener = this.MessageBusHandler.GetHandler<ObjectChangedEvent>()
                    .RegisterEventHandler(parameterObserver, new ObjectChangedMessageBusEventHandlerSubscription(parameterOverride.Parameter, discriminator, action));
            }

            this.Disposables.Add(listener);
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueSetBase"/> for an <see cref="Option"/> (if this <see cref="ParameterOrOverrideBase"/> is option dependent) and a <see cref="ActualFiniteState"/> (if it is state dependent)
        /// </summary>
        /// <param name="actualState">The <see cref="ActualFiniteState"/></param>
        /// <param name="actualOption">The <see cref="Option"/></param>
        /// <returns>The <see cref="ParameterValueSetBase"/> if a value is defined for the <see cref="Option"/></returns>
        protected override ParameterValueSetBase GetValueSet(ActualFiniteState actualState = null, Option actualOption = null)
        {
            var isStateDependent = this.StateDependence != null;
            var parameterOverride = (ParameterOverride)this.Thing;
            var valueset = parameterOverride.ValueSet.SingleOrDefault(x => (!isStateDependent || x.ActualState == actualState) && (!this.IsOptionDependent || x.ActualOption == actualOption));

            return valueset;
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
        public override void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Payload = this.Thing;
            dragInfo.Effects = DragDropEffects.All;
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is RelationalExpression expression && this.ThingCreator.IsCreateBinaryRelationshipForRequirementVerificationAllowed(this.Thing, expression))
            {
                this.DragOver(dropInfo, expression);

                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Update the drag state when the payload is a <see cref="RelationalExpression"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> to update</param>
        /// <param name="expression">The <see cref="RelationalExpression"/> payload</param>
        private void DragOver(IDropInfo dropInfo, RelationalExpression expression)
        {
            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is RelationalExpression expression && this.ThingCreator.IsCreateBinaryRelationshipForRequirementVerificationAllowed(this.Thing, expression))
            {
                await this.Drop(this.Thing, expression);
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation when the payload is a <see cref="RelationalExpression"/>
        /// </summary>
        /// <param name="parameter">
        /// The <see cref="ParameterOrOverrideBase"/>
        /// </param>
        /// <param name="expression">
        /// The <see cref="RelationalExpression"/>
        /// </param>
        private async Task Drop(ParameterOrOverrideBase parameter, RelationalExpression expression)
        {
            await this.CreateBinaryRelationship(parameter, expression);
        }
    }
}
