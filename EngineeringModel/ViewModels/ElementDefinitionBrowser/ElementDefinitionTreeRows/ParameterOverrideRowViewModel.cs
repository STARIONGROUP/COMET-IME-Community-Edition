// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The parameter row view model.
    /// </summary>
    public class ParameterOverrideRowViewModel : ParameterOrOverrideBaseRowViewModel, IDropTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideRowViewModel"/> class. 
        /// </summary>
        /// <param name="parameterOverride">
        /// The parameter Override.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">
        /// The container view-model.
        /// </param>
        public ParameterOverrideRowViewModel(ParameterOverride parameterOverride, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterOverride, session, containerViewModel, false)
        {
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var parameterOverride = (ParameterOverride)this.Thing;

            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterOverride.Parameter)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.ObjectChangeEventHandler(new ObjectChangedEvent(this.Thing)));

            this.Disposables.Add(listener);
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
