﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionsBrowserViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView.ViewModels;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.Services;
    using CDP4EngineeringModel.Utilities;
    using CDP4EngineeringModel.ViewModels.Dialogs;

    using CommonServiceLocator;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// Represent the view-model of the browser that displays all the <see cref="ElementDefinition"/>s in one <see cref="Iteration"/>
    /// </summary>
    public class ElementDefinitionsBrowserViewModel : ModellingThingBrowserViewModelBase, IPanelViewModel, IDropTarget, IHaveMessageBusHandler
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// a <see cref="ElementDefinitionBrowserChildComparer"/> that is used to assist in sorted inserts
        /// </summary>
        private static readonly ElementDefinitionBrowserChildComparer rowComparer = new ElementDefinitionBrowserChildComparer();

        /// <summary>
        /// The <see cref="IParameterSubscriptionBatchService"/> used to create multiple <see cref="ParameterSubscription"/>s in a batch operation
        /// </summary>
        private readonly IParameterSubscriptionBatchService parameterSubscriptionBatchService;

        /// <summary>
        /// The <see cref="IChangeOwnershipBatchService"/> used to change the ownership of multiple <see cref="IOwnedThing"/>s in a batch operation
        /// </summary>
        private readonly IChangeOwnershipBatchService changeOwnershipBatchService;

        /// <summary>
        /// The <see cref="IMessageBoxService"/> used to show user messages.
        /// </summary>
        private readonly IMessageBoxService messageBoxService;

        /// <summary>
        /// The <see cref="IObfuscationService"/> used to determine if rows should be hidden based on obfuscation
        /// </summary>
        private readonly IObfuscationService obfuscationService;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for the <see cref="CanCreateParameterGroup"/> property
        /// </summary>
        private bool canCreateParameterGroup;

        /// <summary>
        /// Backing field for the <see cref="CanCreateElementDefinition"/> property
        /// </summary>
        private bool canCreateElementDefinition;

        /// <summary>
        /// Backing field for <see cref="CanCreateSubscription"/>
        /// </summary>
        private bool canCreateSubscription;

        /// <summary>
        /// Backing field for <see cref="CanCreateBatchSubscriptions"/>
        /// </summary>
        private bool canCreateBatchSubscriptions;

        /// <summary>
        /// Backing field for <see cref="CanDeleteBatchSubscriptions"/>
        /// </summary>
        private bool canDeleteBatchSubscriptions;

        /// <summary>
        /// Backing field for <see cref="CanCreateOverride"/>
        /// </summary>
        private bool canCreateOverride;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionsBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The thing Dialog Navigation Service</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <param name="parameterSubscriptionBatchService">
        /// The <see cref="IParameterSubscriptionBatchService"/> used to create multiple <see cref="ParameterSubscription"/>s in a batch operation
        /// </param>
        /// <param name="changeOwnershipBatchService">
        /// The <see cref="IChangeOwnershipBatchService"/> used to change the ownership of multiple <see cref="IOwnedThing"/>s in a batch operation
        /// </param>
        public ElementDefinitionsBrowserViewModel(
            Iteration iteration,
            ISession session,
            IThingDialogNavigationService thingDialogNavigationService,
            IPanelNavigationService panelNavigationService,
            IDialogNavigationService dialogNavigationService,
            IPluginSettingsService pluginSettingsService,
            IParameterSubscriptionBatchService parameterSubscriptionBatchService,
            IChangeOwnershipBatchService changeOwnershipBatchService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            var stopWatch = Stopwatch.StartNew();

            this.Caption = "Element Definitions";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.parameterSubscriptionBatchService = parameterSubscriptionBatchService;
            this.changeOwnershipBatchService = changeOwnershipBatchService;
            this.messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

            this.obfuscationService = new ObfuscationService();
            this.obfuscationService.Initialize(this.Thing, this.Session);
            this.Disposables.Add(this.obfuscationService);

            this.ElementDefinitionRowViewModels = new DisposableReactiveList<IRowViewModelBase<Thing>>();

            this.ExecuteLongRunningDispatcherAction(
                () =>
                {
                    this.UpdateElementDefinition(true);
                    this.AddSubscriptions();
                    this.UpdateProperties();
                    stopWatch.Stop();
                    logger.Info("The Element Definition browser loaded in {0}", stopWatch.Elapsed.ToString("hh':'mm':'ss'.'fff"));
                },
                $"Loading {this.Caption}");
        }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup => this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>();

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get => this.currentModel;
            private set => this.RaiseAndSetIfChanged(ref this.currentModel, value);
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get => this.currentIteration;
            private set => this.RaiseAndSetIfChanged(ref this.currentIteration, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession"/> can create a <see cref="ParameterGroup"/>
        /// </summary>
        public bool CanCreateParameterGroup
        {
            get => this.canCreateParameterGroup;
            set => this.RaiseAndSetIfChanged(ref this.canCreateParameterGroup, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ISession"/> can create a <see cref="ElementDefinition"/>
        /// </summary>
        public bool CanCreateElementDefinition
        {
            get => this.canCreateElementDefinition;
            set => this.RaiseAndSetIfChanged(ref this.canCreateElementDefinition, value);
        }

        /// <summary>
        /// Gets a value indicating whether the create subscription command shall be enabled
        /// </summary>
        public bool CanCreateSubscription
        {
            get => this.canCreateSubscription;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateSubscription, value);
        }

        /// <summary>
        /// Gets a value indicating whether the batch create subscription command shall be enabled
        /// </summary>
        public bool CanCreateBatchSubscriptions
        {
            get => this.canCreateBatchSubscriptions;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateBatchSubscriptions, value);
        }

        /// <summary>
        /// Gets a value indicating whether the batch delete subscription command shall be enabled
        /// </summary>
        public bool CanDeleteBatchSubscriptions
        {
            get => this.canDeleteBatchSubscriptions;
            private set => this.RaiseAndSetIfChanged(ref this.canDeleteBatchSubscriptions, value);
        }

        /// <summary>
        /// Gets a value indicating whether the create override command shall be enabled
        /// </summary>
        public bool CanCreateOverride
        {
            get => this.canCreateOverride;
            private set => this.RaiseAndSetIfChanged(ref this.canCreateOverride, value);
        }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to Copy Model Code to clipboard <see cref="ParameterRowViewModel"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CopyModelCodeToClipboardCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to show the usages of specified element definition
        /// </summary>
        public ReactiveCommand<Unit, Unit> HighlightElementUsagesCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ParameterGroup"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateParameterGroup { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to create a <see cref="ElementDefinition"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateElementDefinition { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> used to copy a <see cref="ElementDefinition"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CopyElementDefinitionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="ParameterOverride"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateOverrideCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create a <see cref="ParameterSubscription"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSubscriptionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to create multiple <see cref="ParameterSubscription"/>s in batch operation mode
        /// </summary>
        public ReactiveCommand<Unit, Unit> BatchCreateSubscriptionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to delete multiple <see cref="ParameterSubscription"/>s in batch operation mode
        /// </summary>
        public ReactiveCommand<Unit, Unit> BatchDeleteSubscriptionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to change the ownership of an <see cref="IOwnedThing"/> and its contained items
        /// </summary>
        public ReactiveCommand<Unit, Unit> ChangeOwnershipCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to set an <see cref="ElementDefinition"/> as the top element of the selected iteration
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetAsTopElementDefinitionCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to unset an <see cref="ElementDefinition"/> as the top element of the selected iteration
        /// </summary>
        public ReactiveCommand<Unit, Unit> UnsetAsTopElementDefinitionCommand { get; private set; }

        /// <summary>
        /// Gets the list of rows representing a <see cref="ElementDefinition"/>
        /// </summary>
        /// <remarks>This was made into a list of generic row to use the ReactiveList extension</remarks>
        public DisposableReactiveList<IRowViewModelBase<Thing>> ElementDefinitionRowViewModels { get; private set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Gets the <see cref="MessageBusHandler"/>
        /// </summary>
        public new MessageBusHandler MessageBusHandler { get; } = new MessageBusHandler();

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
            try
            {
                logger.Trace("drag over {0}", dropInfo.TargetItem);
                var droptarget = dropInfo.TargetItem as IDropTarget;

                if (droptarget != null)
                {
                    droptarget.DragOver(dropInfo);
                    return;
                }

                var elementDefinition = dropInfo.Payload as ElementDefinition;

                if (elementDefinition != null)
                {
                    if (elementDefinition.Iid == Guid.Empty)
                    {
                        logger.Debug("Copying an Element Definition that has been created as template - iid is the empty guid");

                        dropInfo.Effects = DragDropEffects.Move;
                        return;
                    }
                    else
                    {
                        dropInfo.Effects = elementDefinition.TopContainer == this.Thing.TopContainer
                            ? DragDropEffects.None
                            : DragDropEffects.Copy;

                        return;
                    }
                }

                var list = dropInfo.Payload as List<ElementDefinition>;

                if (list != null && list.Any())
                {
                    dropInfo.Effects = list.First().TopContainer == this.Thing.TopContainer
                        ? DragDropEffects.None
                        : DragDropEffects.Copy;

                    return;
                }

                dropInfo.Effects = DragDropEffects.None;
            }
            catch (Exception ex)
            {
                dropInfo.Effects = DragDropEffects.None;
                logger.Error(ex, "drag-over caused an error");
                throw;
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
            var dragSource = dragInfo.Payload as IDragSource;

            if (dragSource != null)
            {
                dragSource.StartDrag(dragInfo);
                return;
            }

            // dragging list of ElementDefinitionRows (from another browser)
            if (dragInfo.Payload is IList dragSourceListCollection)
            {
                var dragSourceList = dragSourceListCollection.OfType<ElementDefinitionRowViewModel>();
                dragInfo.Payload = dragSourceList.Select(x => x.Thing).ToList();
                dragInfo.Effects = DragDropEffects.All;
                return;
            }
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var droptarget = dropInfo.TargetItem as IDropTarget;

            if (droptarget != null)
            {
                try
                {
                    this.IsBusy = true;

                    await droptarget.Drop(dropInfo);
                }
                catch (Exception ex)
                {
                    this.Feedback = ex.Message;
                }
                finally
                {
                    this.IsBusy = false;
                }

                return;
            }

            var elementDefinition = dropInfo.Payload as ElementDefinition;

            if (elementDefinition != null)
            {
                if (elementDefinition.Iid == Guid.Empty)
                {
                    logger.Debug("Copying an Element Definition that has been created as template - iid is the empty guid");

                    dropInfo.Effects = DragDropEffects.Copy;

                    try
                    {
                        this.IsBusy = true;
                        await ElementDefinitionService.CreateElementDefinitionFromTemplate(this.Session, this.Thing, elementDefinition);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.Message);
                        this.Feedback = e.Message;
                    }
                    finally
                    {
                        this.IsBusy = false;
                    }
                }
                else
                {
                    // copy the payload to this iteration
                    try
                    {
                        this.IsBusy = true;

                        var copyCreator = new CopyCreator(this.Session, this.DialogNavigationService);
                        await copyCreator.Copy(elementDefinition, this.Thing, dropInfo.KeyStates);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.Message);
                        this.Feedback = e.Message;
                    }
                    finally
                    {
                        this.IsBusy = false;
                    }
                }

                return;
            }

            // deal with multiselect ElementDefinitions
            if (dropInfo.Payload is List<ElementDefinition> list)
            {
                foreach (var definition in list)
                {
                    if (definition.Iid == Guid.Empty)
                    {
                        logger.Debug("Copying an Element Definition that has been created as template - iid is the empty guid");

                        dropInfo.Effects = DragDropEffects.Copy;

                        try
                        {
                            this.IsBusy = true;
                            await ElementDefinitionService.CreateElementDefinitionFromTemplate(this.Session, this.Thing, definition);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.Message);
                            this.Feedback = e.Message;
                        }
                        finally
                        {
                            this.IsBusy = false;
                        }
                    }
                    else
                    {
                        // copy the payload to this iteration
                        try
                        {
                            this.IsBusy = true;

                            var copyCreator = new CopyCreator(this.Session, this.DialogNavigationService);
                            await copyCreator.Copy(definition, this.Thing, dropInfo.KeyStates);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e.Message);
                            this.Feedback = e.Message;
                        }
                        finally
                        {
                            this.IsBusy = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the create <see cref="ReactiveCommandCreator"/> that allow a user to create the different kinds of <see cref="ParameterType"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.ComputeNotContextDependentPermission();

            this.CreateParameterGroup = ReactiveCommandCreator.Create(this.ExecuteCreateParameterGroup, this.WhenAnyValue(vm => vm.CanCreateParameterGroup));
            this.CreateElementDefinition = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<ElementDefinition>(this.Thing), this.WhenAnyValue(vm => vm.CanCreateElementDefinition));
            this.CopyElementDefinitionCommand = ReactiveCommandCreator.Create(this.ExecuteCopyElementDefinition, this.WhenAnyValue(vm => vm.CanCreateElementDefinition));
            this.CreateSubscriptionCommand = ReactiveCommandCreator.Create(this.ExecuteCreateSubscriptionCommand, this.WhenAnyValue(x => x.CanCreateSubscription));
            this.BatchCreateSubscriptionCommand = ReactiveCommandCreator.Create(this.ExecuteBatchCreateSubscriptionCommand, this.WhenAnyValue(x => x.CanCreateBatchSubscriptions));
            this.BatchDeleteSubscriptionCommand = ReactiveCommandCreator.Create(this.ExecuteBatchDeleteSubscriptionCommand, this.WhenAnyValue(x => x.CanDeleteBatchSubscriptions));
            this.ChangeOwnershipCommand = ReactiveCommandCreator.Create(this.ExecuteChangeOwnershipCommand);
            this.SetAsTopElementDefinitionCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteSetAsTopElementDefinitionCommandAsync);
            this.UnsetAsTopElementDefinitionCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteUnsetAsTopElementDefinitionCommandAsync);
            this.CreateOverrideCommand = ReactiveCommandCreator.Create(this.ExecuteCreateParameterOverride, this.WhenAnyValue(vm => vm.CanCreateOverride));
            this.HighlightElementUsagesCommand = ReactiveCommandCreator.Create(this.ExecuteHighlightElementUsagesCommand);
            this.CopyModelCodeToClipboardCommand = ReactiveCommandCreator.Create(this.ExecuteCopyModelCodeToClipboardCommand);
        }

        /// <summary>
        /// Compute the permissions to create the different kinds of <see cref="ParameterType"/>s using the <see cref="IPermissionService"/>
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            if (this.SelectedThing == null)
            {
                return;
            }

            var parameterRow = this.SelectedThing as ParameterOrOverrideBaseRowViewModel;

            if (parameterRow == null)
            {
                return;
            }

            this.Session.OpenIterations.TryGetValue(this.Thing, out var tuple);

            var parameter = parameterRow.Thing as Parameter;

            if (parameter != null)
            {
                if (tuple != null)
                {
                    this.CanCreateOverride = this.SelectedThing.ContainerViewModel is ElementUsageRowViewModel
                                             && (parameter.Owner == tuple.Item1 || parameter.AllowDifferentOwnerOfOverride)
                                             && this.PermissionService.CanWrite(ClassKind.ParameterOverride, this.SelectedThing.ContainerViewModel.Thing);

                    this.CanCreateSubscription = this.SelectedThing.ContainerViewModel is ElementDefinitionRowViewModel
                                                 && parameter.Owner != tuple.Item1
                                                 && this.PermissionService.CanWrite(ClassKind.ParameterSubscription, this.SelectedThing.Thing);
                }

                return;
            }

            if (tuple != null)
            {
                this.CanCreateSubscription = this.SelectedThing.ContainerViewModel is ElementUsageRowViewModel
                                             && ((ParameterOverride)parameterRow.Thing).Owner != tuple.Item1
                                             && this.PermissionService.CanWrite(ClassKind.ParameterSubscription, this.SelectedThing.Thing);
            }
        }

        /// <summary>
        /// Populate the <see cref="ContextMenuItemViewModel"/>s of the current browser
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.SelectedThing == null)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Create an Element Definition", "", this.CreateElementDefinition, MenuItemKind.Create, ClassKind.ElementDefinition));
                return;
            }

            this.ContextMenu.Add(new ContextMenuItemViewModel("Create Multiple Subscriptions", "", this.BatchCreateSubscriptionCommand, MenuItemKind.Create, ClassKind.NotThing));
            this.ContextMenu.Add(new ContextMenuItemViewModel("Delete Multiple Subscriptions", "", this.BatchDeleteSubscriptionCommand, MenuItemKind.Delete, ClassKind.NotThing));

            if (this.SelectedThing is IHaveModelCode)
            {
                this.ContextMenu.Add(new ContextMenuItemViewModel("Copy Model Code to Clipboard", "", this.CopyModelCodeToClipboardCommand, MenuItemKind.None, ClassKind.NotThing));
            }

            var elementDefRow = this.SelectedThing as ElementDefinitionRowViewModel;

            if (elementDefRow != null)
            {
                this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Create an Element Definition", "", this.CreateElementDefinition, MenuItemKind.Create, ClassKind.ElementDefinition));
                this.ContextMenu.Insert(1, new ContextMenuItemViewModel("Create a Parameter Group", "", this.CreateParameterGroup, MenuItemKind.Create, ClassKind.ParameterGroup));
                this.ContextMenu.Insert(2, new ContextMenuItemViewModel("Copy the Element Definition", "", this.CopyElementDefinitionCommand, MenuItemKind.Copy, ClassKind.ElementDefinition));
                this.ContextMenu.Insert(3, new ContextMenuItemViewModel("Highlight Element Usages", "", this.HighlightElementUsagesCommand, MenuItemKind.Highlight, ClassKind.ElementUsage));
                this.ContextMenu.Insert(4, new ContextMenuItemViewModel("Change Ownership", "", this.ChangeOwnershipCommand, MenuItemKind.Edit, ClassKind.NotThing));

                if (elementDefRow.IsTopElement)
                {
                    this.ContextMenu.Insert(5, new ContextMenuItemViewModel("Unset as Top Element", "", this.UnsetAsTopElementDefinitionCommand, MenuItemKind.Edit, ClassKind.NotThing));
                }
                else
                {
                    this.ContextMenu.Insert(5, new ContextMenuItemViewModel("Set as Top Element", "", this.SetAsTopElementDefinitionCommand, MenuItemKind.Edit, ClassKind.NotThing));
                }

                return;
            }

            var usageRow = this.SelectedThing as ElementUsageRowViewModel;

            if (usageRow != null)
            {
                // clear generic menu
                this.ContextMenu.Clear();

                var editMenu = new ContextMenuItemViewModel("Edit", "", null, MenuItemKind.Edit);
                var editUsage = new ContextMenuItemViewModel("Element Usage", "", this.UpdateCommand, MenuItemKind.Edit, ClassKind.ElementUsage);

                var definition = usageRow.Thing.ElementDefinition;
                var editDefinition = new ContextMenuItemViewModel("Element Definition", "", this.ExecuteUpdateCommand, definition, this.PermissionService.CanWrite(definition), MenuItemKind.Edit);
                editMenu.SubMenu.Add(editUsage);
                editMenu.SubMenu.Add(editDefinition);

                var inspectMenu = new ContextMenuItemViewModel("Inspect", "", null, MenuItemKind.Inspect);
                var inspectUsage = new ContextMenuItemViewModel("Element Usage", "", this.InspectCommand, MenuItemKind.Inspect, ClassKind.ElementUsage);
                var inspectDefinition = new ContextMenuItemViewModel("Element Definition", "", this.ExecuteInspectCommand, definition, true, MenuItemKind.Inspect);
                inspectMenu.SubMenu.Add(inspectUsage);
                inspectMenu.SubMenu.Add(inspectDefinition);

                this.ContextMenu.Add(editMenu);
                this.ContextMenu.Add(inspectMenu);
                this.ContextMenu.Add(new ContextMenuItemViewModel("Delete", "", this.DeleteCommand, MenuItemKind.Delete, ClassKind.ElementUsage));

                this.ContextMenu.Add(new ContextMenuItemViewModel("Navigate to Element Definition", "", this.ChangeFocusCommand, MenuItemKind.Navigate, ClassKind.ElementDefinition));

                if (this.SelectedThing.ContainedRows.Count > 0)
                {
                    this.ContextMenu.Add(this.SelectedThing.IsExpanded ? new ContextMenuItemViewModel("Collapse Rows", "", this.CollpaseRowsCommand, MenuItemKind.None, ClassKind.NotThing) : new ContextMenuItemViewModel("Expand Rows", "", this.ExpandRowsCommand, MenuItemKind.None, ClassKind.NotThing));
                }

                if (this.SelectedThing is IHaveModelCode)
                {
                    this.ContextMenu.Add(new ContextMenuItemViewModel("Copy Model Code to Clipboard", "", this.CopyModelCodeToClipboardCommand, MenuItemKind.None, ClassKind.NotThing));
                }

                return;
            }

            var parameterGroupRow = this.SelectedThing as ParameterGroupRowViewModel;

            if (parameterGroupRow != null)
            {
                this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Create a Parameter Group", "", this.CreateParameterGroup, MenuItemKind.Create, ClassKind.ParameterGroup));
                return;
            }

            var parameterRow = this.SelectedThing as ParameterRowViewModel;

            if (parameterRow != null)
            {
                if (parameterRow.ContainerViewModel is ElementUsageRowViewModel)
                {
                    this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Override this Parameter", "", this.CreateOverrideCommand, MenuItemKind.Create, ClassKind.ParameterOverride));
                }
                else if (parameterRow.ContainerViewModel is ElementDefinitionRowViewModel)
                {
                    this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Subscribe to this Parameter", "", this.CreateSubscriptionCommand, MenuItemKind.Create, ClassKind.ParameterSubscription));
                }

                return;
            }

            var overrideRow = this.SelectedThing as ParameterOverrideRowViewModel;

            if (overrideRow != null)
            {
                this.ContextMenu.Insert(0, new ContextMenuItemViewModel("Subscribe to this Override", "", this.CreateSubscriptionCommand, MenuItemKind.Create, ClassKind.ParameterSubscription));
            }
        }

        /// <summary>
        /// Handles the <see cref="DomainChangedEvent"/>
        /// </summary>
        /// <param name="domainChangeEvent">The <see cref="DomainChangedEvent"/></param>
        protected override void UpdateDomain(DomainChangedEvent domainChangeEvent)
        {
            base.UpdateDomain(domainChangeEvent);
            this.ElementDefinitionRowViewModels.ClearAndDispose();
            this.UpdateElementDefinition(true);
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> event-handler.
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateElementDefinition();
            this.UpdateProperties();
        }

        /// <summary>
        /// executes the <see cref="ChangeFocusCommand"/>
        /// </summary>
        protected override void ExecuteChangeFocusCommand()
        {
            var usage = (ElementUsage)this.SelectedThing.Thing;
            var definitionRow = this.ElementDefinitionRowViewModels.SingleOrDefault(x => x.Thing == usage.ElementDefinition);

            if (definitionRow != null)
            {
                this.SelectedThing = definitionRow;
                this.FocusedRow = definitionRow;
            }
        }

        /// <summary>
        /// Brings the Thing into view in the tree
        /// </summary>
        /// <param name="thing">The Thing to focus on</param>
        public void ChangeFocusOnThing(Thing thing)
        {
            IRowViewModelBase<Thing> rowToFocus = null;

            switch (thing)
            {
                case ElementDefinition elementDefinition:
                    rowToFocus = this.ElementDefinitionRowViewModels.SingleOrDefault(x => x.Thing == elementDefinition);
                    break;
                case ElementUsage elementUsage:
                    var containerElementDefinition = elementUsage.Container as ElementDefinition;

                    if (containerElementDefinition == null)
                    {
                        return;
                    }

                    var elementDefinitionRow = this.ElementDefinitionRowViewModels.SingleOrDefault(x => x.Thing == containerElementDefinition);

                    if (elementDefinitionRow == null)
                    {
                        return;
                    }

                    rowToFocus = elementDefinitionRow.ContainedRows.SingleOrDefault(x => x.Thing == elementUsage);
                    break;
                case Parameter parameter:
                    var elementRootNode = this.ElementDefinitionRowViewModels.SingleOrDefault(x => x.Thing == parameter.Container);

                    if (elementRootNode == null)
                    {
                        return;
                    }

                    elementRootNode.IsExpanded = true;

                    // get all the group rows from parameter and expand them recursively in the reverse order
                    if (parameter.Group != null)
                    {
                        var groupRows = this.GetParameterBaseGroupHierarchyFromRoot(parameter, elementRootNode);

                        if (!groupRows.Any())
                        {
                            // something is wrong and hierarchy cant be made.
                            return;
                        }

                        foreach (var rowViewModelBase in groupRows)
                        {
                            rowViewModelBase.IsExpanded = true;
                        }

                        rowToFocus = groupRows.Last().ContainedRows.SingleOrDefault(x => x.Thing == parameter);
                    }
                    else
                    {
                        rowToFocus = elementRootNode.ContainedRows.SingleOrDefault(x => x.Thing == parameter);
                    }

                    break;

                case ParameterOverride parameterOverride:
                    var usage = parameterOverride.Container as ElementUsage;

                    if (usage == null)
                    {
                        return;
                    }

                    var rootNode = this.ElementDefinitionRowViewModels.SingleOrDefault(x => x.Thing == usage.Container);

                    if (rootNode == null)
                    {
                        return;
                    }

                    rootNode.IsExpanded = true;

                    // get usage row
                    var usageRow = rootNode.ContainedRows.SingleOrDefault(x => x.Thing == usage);

                    if (usageRow == null)
                    {
                        return;
                    }

                    usageRow.IsExpanded = true;

                    // get all the group rows from parameter and expand them recursively in the reverse order
                    if (parameterOverride.Group != null)
                    {
                        var groupRows = this.GetParameterBaseGroupHierarchyFromRoot(parameterOverride, usageRow);

                        if (!groupRows.Any())
                        {
                            // something is wrong and hierarchy cant be made.
                            return;
                        }

                        foreach (var rowViewModelBase in groupRows)
                        {
                            rowViewModelBase.IsExpanded = true;
                        }

                        rowToFocus = groupRows.Last().ContainedRows.SingleOrDefault(x => x.Thing == parameterOverride);
                    }
                    else
                    {
                        rowToFocus = usageRow.ContainedRows.SingleOrDefault(x => x.Thing == parameterOverride);
                    }

                    break;
            }

            if (rowToFocus == null)
            {
                return;
            }

            this.SelectedThing = rowToFocus;
            this.FocusedRow = rowToFocus;
        }

        /// <summary>
        /// Get the group row hierchy of the parameterbase and a root
        /// </summary>
        /// <param name="parameterBase">The parameterbase that is located somewhere in the subtree of the root</param>
        /// <param name="root">The root node from which to calculate</param>
        /// <returns>List of group rows that is ordered from the root to the parameter base</returns>
        private List<IRowViewModelBase<Thing>> GetParameterBaseGroupHierarchyFromRoot(ParameterBase parameterBase, IRowViewModelBase<Thing> root)
        {
            var result = new List<IRowViewModelBase<Thing>>();

            if (parameterBase.Group == null)
            {
                return result;
            }

            // get a list of groups from parameter up
            var groups = new List<ParameterGroup>();

            var group = parameterBase.Group;

            while (group != null)
            {
                groups.Add(group);
                group = group.ContainingGroup;
            }

            // reverse order the rows starting from root
            var current = root;
            groups.Reverse();

            foreach (var parameterGroup in groups)
            {
                var groupRow = current.ContainedRows.SingleOrDefault(x => x.Thing == parameterGroup);

                if (groupRow != null)
                {
                    result.Add(groupRow);
                    current = groupRow;
                }
                else
                {
                    // something is broken in the chain and it cannot be established
                    return new List<IRowViewModelBase<Thing>>();
                }
            }

            return result;
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

            foreach (var elementDef in this.ElementDefinitionRowViewModels)
            {
                elementDef.Dispose();
            }

            this.MessageBusHandler.Dispose();
        }

        /// <summary>
        /// Execute the <see cref="CopyModelCodeToClipboardCommand"/>
        /// </summary>
        private void ExecuteCopyModelCodeToClipboardCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            if (this.SelectedThing is IHaveModelCode modelCode)
            {
                Clipboard.SetText(modelCode.ModelCode);
            }
        }

        /// <summary>
        /// Executes the <see cref="OpenAnnotationWindowCommand"/>
        /// </summary>
        /// <param name="annotation">The associated <see cref="ModellingAnnotationItem"/></param>
        protected override void ExecuteOpenAnnotationWindow(ModellingAnnotationItem annotation)
        {
            var vm = new AnnotationFloatingDialogViewModel(annotation, this.Session);
            this.DialogNavigationService.NavigateFloating(vm);
        }

        /// <summary>
        /// Update the rows to display
        /// </summary>
        /// <param name="initial">
        /// A boolean indicating if this is the first load
        /// </param>
        private void UpdateElementDefinition(bool initial = false)
        {
            var currentDef = this.ElementDefinitionRowViewModels.Select(x => (ElementDefinition)x.Thing).ToList();
            var addedDef = this.Thing.Element.Except(currentDef).ToList();

            if (initial)
            {
                this.AddElementDefinitionRows(addedDef);
            }
            else
            {
                foreach (var elementDefinition in addedDef)
                {
                    this.AddElementDefinitionRow(elementDefinition);
                }

                var removedDef = currentDef.Except(this.Thing.Element).ToList();

                foreach (var elementDefinition in removedDef)
                {
                    this.RemoveElementDefinitionRow(elementDefinition);
                }
            }

            var topElementDefinitionOld = this.ElementDefinitionRowViewModels.FirstOrDefault(vm => ((ElementDefinitionRowViewModel)vm).IsTopElement);

            if (this.Thing.TopElement == null)
            {
                // clear the top elements
                if (topElementDefinitionOld != null)
                {
                    ((ElementDefinitionRowViewModel)topElementDefinitionOld).IsTopElement = false;
                }

                return;
            }

            if (this.ElementDefinitionRowViewModels.FirstOrDefault(vm => vm.Thing.Iid == this.Thing.TopElement.Iid) is ElementDefinitionRowViewModel topElementDefinitionNew)
            {
                topElementDefinitionNew.IsTopElement = true;

                // clear old top element.
                if (topElementDefinitionOld != null &&
                    topElementDefinitionOld != topElementDefinitionNew)
                {
                    ((ElementDefinitionRowViewModel)topElementDefinitionOld).IsTopElement = false;
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="ShowElementUsagesCommand"/>.
        /// </summary>
        protected virtual void ExecuteHighlightElementUsagesCommand()
        {
            // clear all highlights
            this.Session.CDPMessageBus.SendMessage(new CancelHighlightEvent());

            // highlight the selected thing
            this.Session.CDPMessageBus.SendMessage(new ElementUsageHighlightEvent((ElementDefinition)this.SelectedThing.Thing), this.SelectedThing.Thing);
            this.Session.CDPMessageBus.SendMessage(new ElementUsageHighlightEvent((ElementDefinition)this.SelectedThing.Thing), null);
        }

        /// <summary>
        /// Add the row of the associated <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="elementDef">The <see cref="ElementDefinition"/> to add</param>
        private void AddElementDefinitionRow(ElementDefinition elementDef)
        {
            this.Session.OpenIterations.TryGetValue(this.Thing, out var tuple);

            if (tuple != null)
            {
                var row = new ElementDefinitionRowViewModel(elementDef, tuple.Item1, this.Session, this, this.obfuscationService);
                this.ElementDefinitionRowViewModels.SortedInsert(row, rowComparer);
            }
        }

        /// <summary>
        /// Add rows of the associated <see cref="ElementDefinition"/>s
        /// </summary>
        /// <param name="elementDefs">The <see cref="ElementDefinition"/>s to add</param>
        private void AddElementDefinitionRows(IEnumerable<ElementDefinition> elementDefs)
        {
            this.Session.OpenIterations.TryGetValue(this.Thing, out var tuple);

            if (tuple != null)
            {
                var rows = new List<ElementDefinitionRowViewModel>();

                foreach (var elementDef in elementDefs)
                {
                    rows.Add(new ElementDefinitionRowViewModel(elementDef, tuple.Item1, this.Session, this, this.obfuscationService));
                }

                rows.Sort(rowComparer);
                this.ElementDefinitionRowViewModels.AddRange(rows);
            }
        }

        /// <summary>
        /// Remove the row of the associated <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="elementDef">The <see cref="ElementDefinition"/> to remove</param>
        private void RemoveElementDefinitionRow(ElementDefinition elementDef)
        {
            var row = this.ElementDefinitionRowViewModels.SingleOrDefault(x => x.Thing == elementDef);

            if (row != null)
            {
                this.ElementDefinitionRowViewModels.RemoveAndDispose(row);
            }
        }

        /// <summary>
        /// Computes the permissions that are only user dependent
        /// </summary>
        /// <remarks>This shall be called at initialization or when the domain changes</remarks>
        private void ComputeNotContextDependentPermission()
        {
            this.CanCreateElementDefinition = this.PermissionService.CanWrite(ClassKind.ElementDefinition, this.Thing);
            this.CanCreateParameterGroup = this.PermissionService.CanWrite(ClassKind.ParameterGroup, this.Thing);
            this.CanCreateBatchSubscriptions = this.PermissionService.CanWrite(ClassKind.ParameterSubscription, this.Thing);
            this.CanDeleteBatchSubscriptions = this.PermissionService.CanWrite(ClassKind.ParameterSubscription, this.Thing);
        }

        /// <summary>
        /// Executes the <see cref="CreateSubscriptionCommand"/>
        /// </summary>
        private async void ExecuteCreateSubscriptionCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var parameterOrOverride = this.SelectedThing.Thing as ParameterOrOverrideBase;

            if (parameterOrOverride == null)
            {
                return;
            }

            Tuple<DomainOfExpertise, Participant> tuple;
            this.Session.OpenIterations.TryGetValue(this.Thing, out tuple);

            if (tuple != null)
            {
                var subscription = new ParameterSubscription
                {
                    Owner = tuple.Item1
                };

                var transactionContext = TransactionContextResolver.ResolveContext(parameterOrOverride);
                var transaction = new ThingTransaction(transactionContext);

                var clone = parameterOrOverride.Clone(false);
                transaction.Create(subscription);
                transaction.CreateOrUpdate(clone);
                clone.ParameterSubscription.Add(subscription);

                await this.DalWrite(transaction);
            }
        }

        /// <summary>
        /// Execute the <see cref="CreateCommand"/>
        /// </summary>
        private async void ExecuteCreateParameterOverride()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var elementUsage = this.SelectedThing.ContainerViewModel.Thing as ElementUsage;

            if (elementUsage == null)
            {
                return;
            }

            var parameter = this.SelectedThing.Thing as Parameter;

            if (parameter == null)
            {
                return;
            }

            this.Session.OpenIterations.TryGetValue(this.Thing, out var tuple);

            if (tuple != null)
            {
                var parameterOverride = new ParameterOverride
                {
                    Parameter = parameter,
                    Owner = tuple.Item1
                };

                var transactionContext = TransactionContextResolver.ResolveContext(elementUsage);
                var transaction = new ThingTransaction(transactionContext);

                transaction.Create(parameterOverride);

                var elementUsageClone = elementUsage.Clone(false);
                transaction.CreateOrUpdate(elementUsageClone);
                elementUsageClone.ParameterOverride.Add(parameterOverride);

                await this.DalWrite(transaction);
            }
        }

        /// <summary>
        /// Execute the <see cref="CopyElementDefinitionCommand"/>
        /// </summary>
        private async void ExecuteCopyElementDefinition()
        {
            var elementDef = this.SelectedThing.Thing as ElementDefinition;
            var copyUsage = true;

            if (elementDef != null && elementDef.ContainedElement.Any())
            {
                var yesNoDialogViewModel = new YesNoDialogViewModel("Confirmation", "Would you like to copy the Element Usages?");
                var result = this.DialogNavigationService.NavigateModal(yesNoDialogViewModel);

                copyUsage = result.Result.HasValue && result.Result.Value;
            }

            try
            {
                this.IsBusy = true;
                var copyCreator = new CopyElementDefinitionCreator(this.Session);
                await copyCreator.Copy((ElementDefinition)this.SelectedThing.Thing, copyUsage);
            }
            catch (Exception exception)
            {
                logger.Error(exception, "An error occured when deleting a copy of an Element Definition");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Execute the <see cref="BatchCreateSubscriptionCommand"/>
        /// </summary>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        private async void ExecuteBatchCreateSubscriptionCommand()
        {
            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            var model = (EngineeringModel)this.Thing.Container;
            var allowedDomainOfExpertises = model.EngineeringModelSetup.ActiveDomain.Where(x => x != currentDomainOfExpertise);

            var requiredRls = model.RequiredRdls;
            var allowedCategories = requiredRls.SelectMany(rdl => rdl.DefinedCategory).Where(c => c.PermissibleClass.Contains(ClassKind.ElementDefinition));
            var allowedParameterTypes = requiredRls.SelectMany(rdl => rdl.ParameterType);

            var categoryDomainParameterTypeSelectorDialogViewModel = new CategoryDomainParameterTypeSelectorDialogViewModel(allowedParameterTypes, allowedCategories, allowedDomainOfExpertises);

            categoryDomainParameterTypeSelectorDialogViewModel.DialogTitle = "Create Parameter Subscriptions";

            var result = this.DialogNavigationService.NavigateModal(categoryDomainParameterTypeSelectorDialogViewModel) as CategoryDomainParameterTypeSelectorResult;

            if (result == null || !result.Result.HasValue || !result.Result.Value)
            {
                return;
            }

            try
            {
                this.IsBusy = true;

                await this.parameterSubscriptionBatchService.Create(this.Session, this.Thing, result.IsUncategorizedIncluded, result.Categories, result.DomainOfExpertises, result.ParameterTypes);
            }
            catch (Exception exception)
            {
                logger.Error(exception, "An error occured when creating ParameterSubscriptions in a batch operation");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Execute the <see cref="ExecuteBatchDeleteSubscriptionCommand"/>
        /// </summary>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        private async void ExecuteBatchDeleteSubscriptionCommand()
        {
            var owner = this.Session.QuerySelectedDomainOfExpertise(this.Thing);

            var filteredParameterTypes = this.Thing.Element.SelectMany(e => e.Parameter)
                .Where(p => p.Owner != owner && p.ParameterSubscription.Any())
                .Select(p => p.ParameterType).Distinct();

            if (!filteredParameterTypes.Any())
            {
                this.messageBoxService.Show("No parameters have been subscribed to", "Delete Subscriptions", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var subscribedElements = this.Thing.Element.Where(e => e.Parameter.Any(p => p.Owner != owner && p.ParameterSubscription.Any()));
            var filteredCategories = subscribedElements.SelectMany(e => e.Category).Distinct();

            var filteredDomainOfExpertises = this.Thing.Element.SelectMany(e => e.Parameter)
                .Where(p => p.Owner != owner && p.ParameterSubscription.Any())
                .Select(p => p.Owner).Distinct();

            var categoryDomainParameterTypeSelectorDialogViewModel = new CategoryDomainParameterTypeSelectorDialogViewModel(filteredParameterTypes, filteredCategories, filteredDomainOfExpertises);

            categoryDomainParameterTypeSelectorDialogViewModel.DialogTitle = "Remove Parameter Subscriptions";

            var result = this.DialogNavigationService.NavigateModal(categoryDomainParameterTypeSelectorDialogViewModel) as CategoryDomainParameterTypeSelectorResult;

            if (result == null || !result.Result.HasValue || !result.Result.Value)
            {
                return;
            }

            try
            {
                this.IsBusy = true;

                Func<IEnumerable<Parameter>, bool> confirmationCallBack = p =>
                    this.messageBoxService.Show(
                        $"{p.Count()} Parameter Subscriptions will be deleted. Are you sure?",
                        "Delete Subscriptions",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Information) == MessageBoxResult.OK;

                await this.parameterSubscriptionBatchService.Delete(
                    this.Session,
                    this.Thing,
                    result.IsUncategorizedIncluded,
                    result.Categories,
                    result.DomainOfExpertises,
                    result.ParameterTypes,
                    confirmationCallBack);
            }
            catch (Exception exception)
            {
                logger.Error(exception, "An error occured when creating ParameterSubscriptions in a batch operation");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Executes the <see cref="SetAsTopElementDefinitionCommand"/>
        /// </summary>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns> 
        private async Task ExecuteSetAsTopElementDefinitionCommandAsync()
        {
            var elementDefRow = this.SelectedThing as ElementDefinitionRowViewModel;

            if (elementDefRow?.Thing != null && (this.Thing.TopElement == null || this.Thing.TopElement != elementDefRow.Thing))
            {
                var iterationClone = this.Thing.Clone(false);
                iterationClone.TopElement = elementDefRow.Thing;

                var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
                var transaction = new ThingTransaction(transactionContext);
                transaction.CreateOrUpdate(iterationClone);

                await this.DalWrite(transaction);
            }
        }

        /// <summary>
        /// Executes the <see cref="UnsetAsTopElementDefinitionCommand"/>
        /// </summary>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        private async Task ExecuteUnsetAsTopElementDefinitionCommandAsync()
        {
            var iteration = this.Thing;
            var elementDefRow = this.SelectedThing as ElementDefinitionRowViewModel;

            if (elementDefRow?.Thing != null && iteration.TopElement == elementDefRow.Thing)
            {
                var iterationClone = iteration.Clone(false);
                iterationClone.TopElement = null;

                var transactionContext = TransactionContextResolver.ResolveContext(iteration);
                var transaction = new ThingTransaction(transactionContext);
                transaction.CreateOrUpdate(iterationClone);

                await this.DalWrite(transaction);
            }
        }

        /// <summary>
        /// Execute the <see cref="ChangeOwnershipCommand"/>
        /// </summary>
        /// <returns>
        /// an awaitable <see cref="Task"/>
        /// </returns>
        private async void ExecuteChangeOwnershipCommand()
        {
            var model = (EngineeringModel)this.Thing.Container;
            var allowedDomainOfExpertises = model.EngineeringModelSetup.ActiveDomain;

            var changeOwnershipSelectionDialogViewModel = new ChangeOwnershipSelectionDialogViewModel(allowedDomainOfExpertises);
            var result = this.DialogNavigationService.NavigateModal(changeOwnershipSelectionDialogViewModel) as ChangeOwnershipSelectionResult;

            if (result == null || !result.Result.HasValue || !result.Result.Value)
            {
                return;
            }

            try
            {
                this.IsBusy = true;

                await this.changeOwnershipBatchService.Update(
                    this.Session,
                    this.SelectedThing.Thing,
                    result.DomainOfExpertise,
                    result.IsContainedItemChangeOwnershipSelected,
                    new List<ClassKind> { ClassKind.ElementDefinition, ClassKind.Parameter });
            }
            catch (Exception exception)
            {
                logger.Error(exception, "An error occured when chaning Ownership of an Element Definition and contained Parameters in a batch operation");
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.UpdateProperties());

            this.Disposables.Add(iterationSetupSubscription);
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Thing);
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";
        }

        /// <summary>
        /// Executes the <see cref="CreateParameterGroup"/> command
        /// </summary>
        private void ExecuteCreateParameterGroup()
        {
            ElementDefinition containerElementDefinition = null;

            var group = new ParameterGroup();

            var containingParameterGroup = this.SelectedThing.Thing as ParameterGroup;

            if (containingParameterGroup != null)
            {
                containerElementDefinition = containingParameterGroup.GetContainerOfType<ElementDefinition>();
                group.ContainingGroup = containingParameterGroup;
            }
            else
            {
                containerElementDefinition = this.SelectedThing.Thing as ElementDefinition;
            }

            if (containerElementDefinition == null)
            {
                logger.Debug("The container ElementDefinition could not be set. Something is wrong");
                return;
            }

            this.ExecuteCreateCommand(group, containerElementDefinition);
        }
    }
}
