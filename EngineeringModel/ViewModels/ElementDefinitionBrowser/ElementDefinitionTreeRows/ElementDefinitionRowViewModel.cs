// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModel.cs" company="RHEA System S.A.">
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
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Builders;
    using CDP4Composition.DragDrop;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4EngineeringModel.Services;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;
    using Utilities;

    using IDropTarget = CDP4Composition.DragDrop.IDropTarget;

    /// <summary>
    /// The row representing an <see cref="ElementDefinition"/>
    /// </summary>
    public class ElementDefinitionRowViewModel : ElementBaseRowViewModel<ElementDefinition>, IDropTarget
    {
        /// <summary>
        /// The backing field for <see cref="IsTopElement"/>
        /// </summary>
        private bool isTopElement;

        /// <summary>
        /// The backing field for <see cref="ThingCreator"/>
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Backing field for <see cref="DisplayCategory"/>
        /// </summary>
        private string displayCategory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel"/> class
        /// </summary>
        /// <param name="elementDefinition">The associated <see cref="ElementDefinition"/></param>
        /// <param name="currentDomain">The active <see cref="DomainOfExpertise"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container view-model</param>
        /// <param name="obfuscationService">The obfuscation service</param>
        public ElementDefinitionRowViewModel(ElementDefinition elementDefinition, DomainOfExpertise currentDomain, ISession session, IViewModelBase<Thing> containerViewModel,
            IObfuscationService obfuscationService)
            : base(elementDefinition, currentDomain, session, containerViewModel, obfuscationService)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the value indicating whether this is a top element or not.
        /// </summary>
        public override bool IsTopElement
        {
            get { return this.isTopElement; }
            set { this.RaiseAndSetIfChanged(ref this.isTopElement, value); }
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
        /// Gets or sets the Categories in display format
        /// </summary>
        public string DisplayCategory
        {
            get => this.displayCategory;
            set => this.RaiseAndSetIfChanged(ref this.displayCategory, value);
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            Func<ObjectChangedEvent, bool> optionDiscriminator = 
                objectChange => 
                    objectChange.EventKind == EventKind.Updated 
                    && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache 
                    && objectChange.ChangedThing.TopContainer == this.Thing.TopContainer;

            Action<ObjectChangedEvent> optionAction = x => this.UpdateModelCode();

            if (this.AllowMessageBusSubscriptions)
            {
                var optionRemoveListener =
                    CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Option))
                        .Where(optionDiscriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(optionAction);

                this.Disposables.Add(optionRemoveListener);
            }
            else
            {
                var optionObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Option));

                this.Disposables.Add(
                    this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(optionObserver, new ObjectChangedMessageBusEventHandlerSubscription(typeof(Option), optionDiscriminator, optionAction)));
            }
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
            if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
            {
                this.DragOver(dropInfo, parameterTypeAndScale);
                return;
            }

            if (dropInfo.Payload is List<ElementDefinition> list)
            {
                this.DragOver(dropInfo, list[0]);
                return;
            }

            if (dropInfo.Payload is ElementDefinition elementDefinition)
            {
                this.DragOver(dropInfo, elementDefinition);
                return;
            }

            if (dropInfo.Payload is Category category)
            {
                this.DragOver(dropInfo, category);
                return;
            }

            if (dropInfo.Payload is Parameter parameter && parameter.Container == this.Thing)
            {
                this.DragOver(dropInfo, parameter);
                return;
            }

            // moving the group into a group of the same element definition
            if (dropInfo.Payload is ParameterGroup @group && group.Container == this.Thing)
            {
                this.DragOver(dropInfo, group);
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
            if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
            {
                await this.Drop(dropInfo, parameterTypeAndScale);
            }

            if (dropInfo.Payload is List<ElementDefinition> list)
            {
                foreach (var definition in list)
                {
                    await this.Drop(dropInfo, definition);
                }
            }

            if (dropInfo.Payload is ElementDefinition elementDefinition)
            {
                await this.Drop(dropInfo, elementDefinition);
            }

            // moving 
            if (dropInfo.Payload is Parameter parameter)
            {
                await this.Drop(dropInfo, parameter);
            }

            // moving the group right under the element definition
            if (dropInfo.Payload is ParameterGroup group)
            {
                await this.Drop(dropInfo, group);
            }

            if (dropInfo.Payload is Category category)
            {
                await this.Drop(dropInfo, category);
            }
        }
        
        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
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
        /// Update this row and its children
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();
            this.PopulateParameterGroups();
            this.PopulateParameters();
            this.PopulateElemenUsages();
            this.UpdateCategories();
        }

        /// <summary>
        /// Gets the Categories in display format
        /// </summary>
        private void UpdateCategories()
        {
            DisplayCategory = new CategoryStringBuilder()
                        .AddCategories("ED", this.Thing.Category)
                        .Build();
        }

        /// <summary>
        /// Populate the <see cref="ParameterGroupRowViewModel"/>
        /// </summary>
        private void PopulateParameterGroups()
        {
            this.PopulateParameterGroups(this.Thing);
        }

        /// <summary>
        /// Populate the <see cref="ParameterSubscriptionRowViewModel"/>
        /// </summary>
        private void UpdateParameterSubscription()
        {
            // add or remove Subscription
            var definedParameterWithSubscription = this.Thing.Parameter.Where(x => x.ParameterSubscription.Any(s => s.Owner == this.currentDomain)).ToList();
            var currentSubscription = this.parameterBaseCache.Keys.OfType<ParameterSubscription>().ToList();

            var definedSubscription = definedParameterWithSubscription.Select(x => x.ParameterSubscription.Single(s => s.Owner == this.currentDomain)).ToList();

            // DELETED Parameter Subscription
            var deletedSubscription = currentSubscription.Except(definedSubscription).ToList();
            this.RemoveParameterBase(deletedSubscription);
            
            // ADDED Parameter Subscription
            var addedSubscription = definedSubscription.Except(currentSubscription).ToList();
            this.AddParameterBase(addedSubscription);
        }

        /// <summary>
        /// Populates the <see cref="ParameterOrOverrideBaseRowViewModel"/>
        /// </summary>
        protected override void PopulateParameters()
        {
            this.UpdateParameterSubscription();

            var definedParameterWithoutSubscription = this.Thing.Parameter.Where(x => x.ParameterSubscription.All(s => s.Owner != this.currentDomain)).ToList();
            var currentParameter = this.parameterBaseCache.Keys.OfType<Parameter>().ToList();

            // DELETED Parameter
            var deletedParameters = currentParameter.Except(definedParameterWithoutSubscription).ToList();
            this.RemoveParameterBase(deletedParameters);

            // add new parameters
            var addedParameters = definedParameterWithoutSubscription.Except(currentParameter).ToList();
            this.AddParameterBase(addedParameters);
        }

        /// <summary>
        /// Update this <see cref="Tooltip"/> with extra information.
        /// </summary>
        protected override void UpdateTooltip()
        {
            base.UpdateTooltip();

            var sb = new StringBuilder(this.Tooltip);

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("When dragging and dropping Element Definitions between models, the following modifier keys can be used:");
            sb.AppendLine();
            sb.AppendLine("- None: The ownership in the target model is set to the Active Domain. The values are set to the default \"-\".");
            sb.AppendLine();
            sb.AppendLine("- Ctrl: The ownership in the target model is set to the Active Domain. The values are set to values as defined in the source Model.");
            sb.AppendLine();
            sb.AppendLine("- Shift: The ownership in the target model is set to the ownership as defined in the source model. The values are set to the default \"-\".");
            sb.AppendLine();
            sb.AppendLine("- Ctrl + Shift: The ownership in the target model is set to the ownership as defined in the source model. The values are set to values as defined in the source Model.");
            sb.AppendLine();
            sb.AppendLine("NOTE: The Element Definition is always copied, including the contained Element Usages and Parameters.");
            sb.AppendLine("NOTE: This functionality is only supported if the target server is COMET.");

            this.Tooltip = sb.ToString();
        }

        /// <summary>
        /// Populates the <see cref="ElementUsageRowViewModel"/>
        /// </summary>
        private void PopulateElemenUsages()
        {
            var currentUsages = this.ContainedRows.OfType<ElementUsageRowViewModel>().Select(x => x.Thing).ToList();

            var deletedUsages = currentUsages.Except(this.Thing.ContainedElement).ToList();
            foreach (var deletedUsage in deletedUsages)
            {
                var row = this.ContainedRows.OfType<ElementUsageRowViewModel>().SingleOrDefault(x => x.Thing == deletedUsage);
                if (row == null)
                {
                    continue;
                }

                this.ContainedRows.RemoveAndDispose(row);
            }

            var addedUsages = this.Thing.ContainedElement.Except(currentUsages).ToList();
            foreach (var elementUsage in addedUsages)
            {
                var row = new ElementUsageRowViewModel(elementUsage, this.currentDomain, this.Session, this, this.ObfuscationService);
                this.ContainedRows.SortedInsert(row, ChildRowComparer);
            }
        }
        
        /// <summary>
        /// Update the children rows of the current row
        /// </summary>
        public void UpdateChildren()
        {
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Set the <see cref="IDropInfo.Effects"/> when the payload is an <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="dropinfo">The <see cref="IDropInfo"/></param>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> in the payload</param>
        private void DragOver(IDropInfo dropinfo, ElementDefinition elementDefinition)
        {
            var iteration = (Iteration) this.Thing.Container;
            if (!this.PermissionService.CanWrite(ClassKind.ElementDefinition, iteration))
            {
                dropinfo.Effects = DragDropEffects.None;
                return;
            }

            if (elementDefinition.Iid == Guid.Empty)
            {
                logger.Debug("Copying an Element Definition that has been created as template - iid is the empty guid");

                dropinfo.Effects = DragDropEffects.Move;
                return;
            }

            if (elementDefinition.TopContainer == this.Thing.TopContainer)
            {
                dropinfo.Effects = elementDefinition.HasUsageOf(this.Thing)
                    ? DragDropEffects.None
                    : DragDropEffects.Copy;
            }
            else
            {
                // copying from another EM
                dropinfo.Effects = DragDropEffects.Copy;
                return;
            }

            if (!iteration.Element.Contains(elementDefinition))
            {
                dropinfo.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Set the <see cref="IDropInfo.Effects"/> when the payload is an <see cref="Tuple{ParameterType, MeasurementScale}"/>
        /// </summary>
        /// <param name="dropinfo">The <see cref="IDropInfo"/></param>
        /// <param name="tuple">The <see cref="Tuple{ParameterType, MeasurementScale}"/> in the payload</param>
        private void DragOver(IDropInfo dropinfo, Tuple<ParameterType, MeasurementScale> tuple)
        {
            // check if parameter type is in the chain of rdls
            var model = (EngineeringModel)this.Thing.TopContainer;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
            var rdlChains = new List<ReferenceDataLibrary> {mrdl};
            rdlChains.AddRange(mrdl.RequiredRdls);

            if (!rdlChains.Contains(tuple.Item1.Container))
            {
                dropinfo.Effects = DragDropEffects.None;
                logger.Warn("A parameter with the current parameter type cannot be created as the parameter type does not belong to the available libraries.");
                return;
            }

            if (!this.PermissionService.CanWrite(ClassKind.Parameter, this.Thing))
            {
                dropinfo.Effects = DragDropEffects.None;
                logger.Warn("You do not have permission to create a parameter in this element definition.");
                return;
            }

            var parameterType = tuple.Item1;

            // A parameter that references the drag-over ParameterType already exists
            if (this.Thing.Parameter.Any(x => x.ParameterType == parameterType))
            {
                logger.Warn("A parameter with the current parameter type already exists.");
                dropinfo.Effects = DragDropEffects.None;
                return;
            }

            dropinfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// Set the <see cref="IDropInfo.Effects"/> when the payload is an <see cref="Parameter"/>
        /// </summary>
        /// <param name="dropinfo">The <see cref="IDropInfo"/></param>
        /// <param name="parameter">The <see cref="Parameter"/> in the payload</param>
        private void DragOver(IDropInfo dropinfo, Parameter parameter)
        {
            if (!this.PermissionService.CanWrite(parameter))
            {
                dropinfo.Effects = DragDropEffects.None;
                return;
            }

            dropinfo.Effects = DragDropEffects.Move;
        }

        /// <summary>
        /// Set the <see cref="IDropInfo.Effects"/> when the payload is an <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="dropinfo">The <see cref="IDropInfo"/></param>
        /// <param name="group">The <see cref="ParameterGroup"/> in the payload</param>
        private void DragOver(IDropInfo dropinfo, ParameterGroup group)
        {
            if (!this.PermissionService.CanWrite(group))
            {
                dropinfo.Effects = DragDropEffects.None;
                return;
            }

            dropinfo.Effects = DragDropEffects.Move;
        }
        
        /// <summary>
        /// Handle the drop of a <see cref="Tuple{ParameterType, MeasurementScale}"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> containing the payload</param>
        /// <param name="parameterTypeAndScale">The <see cref="Tuple{ParameterType, MeasurementScale}"/></param>
        private async Task Drop(IDropInfo dropInfo, Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
        {
            var parameterType = parameterTypeAndScale.Item1;
            var measeurementScale = parameterTypeAndScale.Item2;

            if (this.Thing.Parameter.Any(x => x.ParameterType == parameterType))
            {
                dropInfo.Effects = DragDropEffects.None;
                return;
            }

            try
            {
                await this.ThingCreator.CreateParameter(this.Thing, null, parameterType, measeurementScale, this.currentDomain, this.Session);
            }
            catch (Exception ex)
            {
                this.ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// Handle the drop of a <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> containing the payload</param>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/></param>
        private async Task Drop(IDropInfo dropInfo, ElementDefinition elementDefinition)
        {
            try
            {
                if (elementDefinition.Iid == Guid.Empty)
                {
                    logger.Debug("Copying an Element Definition that has been created as template - iid is the empty guid");
                    dropInfo.Effects = DragDropEffects.Copy;

                    var iteration = (Iteration)this.Thing.Container;
                    await ElementDefinitionService.CreateElementDefinitionFromTemplate(this.Session, iteration, elementDefinition);
                    return;
                }
                
                if (elementDefinition.TopContainer == this.Thing.TopContainer)
                {
                    await this.ThingCreator.CreateElementUsage(this.Thing, elementDefinition, this.currentDomain, this.Session);
                }
                else
                {
                    // copy the payload to this iteration
                    var copyCreator = new CopyCreator(this.Session, this.dialogNavigationService);
                    await copyCreator.Copy(elementDefinition, (Iteration)this.Thing.Container, dropInfo.KeyStates);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                this.ErrorMsg = ex.Message;
            }
        }

        /// <summary>
        /// Handle the drop of a <see cref="Parameter"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> containing the payload</param>
        /// <param name="parameter">The <see cref="Parameter"/></param>
        private async Task Drop(IDropInfo dropInfo, Parameter parameter)
        {
            if (dropInfo.Effects == DragDropEffects.Move)
            {
                var clone = parameter.Clone(false);
                clone.Group = null;
                await this.DalWrite(clone);
            }
        }

        /// <summary>
        /// Handle the drop of a <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="dropInfo">The <see cref="IDropInfo"/> containing the payload</param>
        /// <param name="group">The <see cref="ParameterGroup"/></param>
        private async Task Drop(IDropInfo dropInfo, ParameterGroup group)
        {
            if (dropInfo.Effects == DragDropEffects.Move)
            {
                var clone = group.Clone(false);
                clone.ContainingGroup = null;
                await this.DalWrite(clone);
            }
        }
    }
}
