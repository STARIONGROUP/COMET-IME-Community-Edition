// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationalExpressionRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Requirements.Extensions;
    using CDP4Requirements.ViewModels.RequirementBrowser;

    using CDP4RequirementsVerification;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// the row-view-model representing a <see cref="RelationalExpression"/>
    /// </summary>
    public class RelationalExpressionRowViewModel : CDP4CommonView.RelationalExpressionRowViewModel, IDropTarget, IDeprecatableThing, IHaveWritableRequirementStateOfCompliance
    {
        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/> property.
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Backing field for <see cref="RelationalExpressionRowViewModel.RequirementStateOfCompliance"/>
        /// </summary>
        private RequirementStateOfCompliance requirementStateOfCompliance;

        /// <summary>
        /// The backing field for <see cref="ThingCreator"/>
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalExpressionRowViewModel"/> class
        /// </summary>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{T}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public RelationalExpressionRowViewModel(RelationalExpression relationalExpression, ISession session, IViewModelBase<Thing> containerViewModel) : base(relationalExpression, session, containerViewModel)
        {
            this.UpdateProperties();
            this.SetRequirementStateOfComplianceChangedEventSubscription(this.Thing, this.Disposables, session.CDPMessageBus);
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
        /// Gets or sets the ParametricConstraintsVerifier
        /// </summary>
        public RequirementStateOfCompliance RequirementStateOfCompliance
        {
            get => this.requirementStateOfCompliance;
            set => this.RaiseAndSetIfChanged(ref this.requirementStateOfCompliance, value);
        }

        /// <summary>
        /// Gets the short string representation of the current <see cref="RelationalExpression"/>
        /// </summary>
        public string ShortName => this.Thing != null ? this.Thing.StringValue : string.Empty;

        /// <summary>
        /// Gets the parameter type name of the current RelationalExpression
        /// </summary>
        public string Name => this.Thing?.ParameterType != null ? this.Thing.ParameterType.Name : string.Empty;

        /// <summary>
        /// Gets the value of the current RelationalExpression
        /// </summary>
        public string Definition => this.Thing != null ? string.Join(", ", this.Thing.Value) : string.Empty;

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
            if (dropInfo.Payload is ParameterOrOverrideBase parameter && this.ThingCreator.IsCreateBinaryRelationshipForRequirementVerificationAllowed(parameter, this.Thing))
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
            if (dropInfo.Payload is ParameterOrOverrideBase parameter && this.ThingCreator.IsCreateBinaryRelationshipForRequirementVerificationAllowed(parameter, this.Thing))
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
                await this.ThingCreator.CreateBinaryRelationshipForRequirementVerification(this.Session, iteration, parameter, relationalExpression);
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
            this.UpdateTooltip();
            this.ResetRequirementStateOfComplianceTree();
        }

        /// <summary>
        /// Update this <see cref="RowViewModelBase{T}.Tooltip"/>
        /// </summary>
        protected override void UpdateTooltip()
        {
            var relationships = this.Thing.QueryRelationships.OfType<BinaryRelationship>().Where(x => x.Source is ParameterOrOverrideBase).ToList();

            if (relationships.Any())
            {
                var strBuild = new StringBuilder();

                foreach (var relationship in relationships)
                {
                    if (strBuild.Length > 0)
                    {
                        strBuild.AppendLine("");
                    }

                    strBuild.Append($"⟶ {relationship.Source.UserFriendlyName}");
                }

                this.Tooltip = strBuild.ToString();

                return;
            }

            base.UpdateTooltip();
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
