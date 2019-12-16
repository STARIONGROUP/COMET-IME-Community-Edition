// -------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
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
    /// the row-view-model representing a <see cref="RelationalExpression"/>
    /// </summary>
    public class RelationalExpressionRowViewModel : CDP4CommonView.RelationalExpressionRowViewModel, IDropTarget, IDeprecatableThing
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalExpressionRowViewModel"/> class
        /// </summary>
        /// <param name="notExpression">The <see cref="RelationalExpression"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public RelationalExpressionRowViewModel(RelationalExpression notExpression, ISession session, IViewModelBase<Thing> containerViewModel) : base(notExpression, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the short string representation of the current RelationalExpression
        /// </summary>
        public string ShortName => this.Thing.StringValue;

        /// <summary>
        /// Gets the parameter type name of the current RelationalExpression
        /// </summary>
        public string Name => this.Thing.ParameterType?.Name;

        /// <summary>
        /// Gets the value of the current RelationalExpression
        /// </summary>
        public string Definition => string.Join(", ", this.Thing.Value);

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get => this.isDeprecated;
            set => this.RaiseAndSetIfChanged(ref this.isDeprecated, value);
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            if (this.ContainerViewModel is IDeprecatableThing deprecatable)
            {
                var containerIsDeprecatedSubscription = deprecatable.WhenAnyValue(vm => vm.IsDeprecated)
                    .Subscribe(_ => this.UpdateIsDeprecatedDerivedFromContainerRowViewModel());

                this.Disposables.Add(containerIsDeprecatedSubscription);
            }
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
            if (dropInfo.Payload is ParameterOrOverrideBase parameter && BinaryRelationshipCreator.IsCreateBinaryRelationshipAllowed(parameter, this.Thing))
            {
                dropInfo.Effects = DragDropEffects.Copy;

                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is ParameterOrOverrideBase parameter && BinaryRelationshipCreator.IsCreateBinaryRelationshipAllowed(parameter, this.Thing))
            {
                await this.CreateBinaryRelationship(parameter, this.Thing);

                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Create a <see cref="BinaryRelationship"/> between this expression and a <see cref="Parameter"/>
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task CreateBinaryRelationship(ParameterOrOverrideBase parameter, RelationalExpression relationalExpression)
        {
            if (this.Thing?.GetContainerOfType<Iteration>() is Iteration iteration)
            {
                await BinaryRelationshipCreator.CreateBinaryRelationship(this.Session, iteration, parameter, relationalExpression);
            }
        }

        /// <summary>
        /// Update this row and its children
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();
            this.UpdateIsDeprecatedDerivedFromContainerRowViewModel();
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
        }

        /// <summary>
        /// Updates the IsDeprecated property based on the value of the container <see cref="RequirementRowViewModel"/>
        /// </summary>
        private void UpdateIsDeprecatedDerivedFromContainerRowViewModel()
        {
            if (this.ContainerViewModel is IDeprecatableThing deprecatable)
            {
                this.IsDeprecated = deprecatable.IsDeprecated;
            }
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
    }
}
