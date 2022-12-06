// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateToParameterTypeMapperBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ReferenceDataMapper.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4ReferenceDataMapper.Managers;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// A vew-model that allows a user to map parameters that are not state dependent to other state dependent parameters
    /// </summary>
    public class StateToParameterTypeMapperBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel, IDropTarget
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for the <see cref="SelectedElementDefinitionCategory"/>
        /// </summary>
        private Category selectedElementDefinitionCategory;

        /// <summary>
        /// Backing field for the <see cref="SelectedActualFiniteStateList"/>
        /// </summary>
        private ActualFiniteStateList selectedActualFiniteStateList;

        /// <summary>
        /// Backing field for the <see cref="SourceParameterTypes"/>
        /// </summary>
        private ReactiveList<ParameterType> sourceParameterTypes;

        /// <summary>
        /// Backing field for the <see cref="SelectedTargetMappingParameterType"/>
        /// </summary>
        private TextParameterType selectedTargetMappingParameterType;

        /// <summary>
        /// Backing field for the <see cref="SelectedTargetValueParameterType"/>
        /// </summary>
        private ScalarParameterType selectedTargetValueParameterType;

        /// <summary>
        /// Backing field for the <see cref="DataSourceManager"/>
        /// </summary>
        private DataSourceManager dataSourceManager;

        /// <summary>
        /// Backing field for the <see cref="ParameterTypesSelected"/> property
        /// </summary>
        private bool parameterTypesSelected;

        /// <summary>
        /// Backing field for <see cref="SelectedSourceParameterType"/>
        /// </summary>
        private ParameterType selectedSourceParameterType;

        /// <summary>
        /// Backing field for the <see cref="SelectedRow"/> property
        /// </summary>
        private DataRowView selectedRow;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Parameter to Actual Finite State Mapping";

        /// <summary>
        /// Initializes a new instance of the <see cref="StateToParameterTypeMapperBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="EngineeringModel"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        public StateToParameterTypeMapperBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            var sw = new Stopwatch();
            logger.Debug("Initializing StateToParameterTypeMapperBrowserViewModel");

            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.PossibleElementDefinitionCategory = new ReactiveList<Category>();
            this.PossibleActualFiniteStateList = new ReactiveList<ActualFiniteStateList>();
            this.SourceParameterTypes = new ReactiveList<ParameterType>();

            this.SourceParameterTypes.CountChanged.Subscribe(x => { this.ParameterTypesSelected = x > 0; });

            this.PossibleTargetMappingParameterType = new ReactiveList<TextParameterType>();
            this.PossibleTargetValueParameterType = new ReactiveList<ScalarParameterType>();

            this
                .WhenAnyValue(x => x.SelectedRow)
                .Subscribe(this.SelectedRowChanged);

            this.UpdateProperties();

            this.DataSourceManager = new DataSourceManager();

            logger.Debug($"Finished Initialization of StateToParameterTypeMapperBrowserViewModel in {sw.ElapsedMilliseconds} [ms]");
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
        /// Gets or sets the selected <see cref="Category"/> used to filter the <see cref="ElementDefinition"/>
        /// </summary>
        public Category SelectedElementDefinitionCategory
        {
            get => this.selectedElementDefinitionCategory;
            set => this.RaiseAndSetIfChanged(ref this.selectedElementDefinitionCategory, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ActualFiniteStateList"/>
        /// </summary>
        public ActualFiniteStateList SelectedActualFiniteStateList
        {
            get => this.selectedActualFiniteStateList;
            set => this.RaiseAndSetIfChanged(ref this.selectedActualFiniteStateList, value);
        }

        /// <summary>
        /// Gets or sets the selected source <see cref="ParameterType"/>s
        /// </summary>
        public ReactiveList<ParameterType> SourceParameterTypes
        {
            get => this.sourceParameterTypes;
            set => this.RaiseAndSetIfChanged(ref this.sourceParameterTypes, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ParameterType"/> from the <see cref="SourceParameterTypes"/> property
        /// </summary>
        public ParameterType SelectedSourceParameterType
        {
            get => this.selectedSourceParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedSourceParameterType, value);
        }

        /// <summary>
        /// Gets or sets the selected target <see cref="TextParameterType"/> that is to select a <see cref="Parameter"/> in which the
        /// mapping is to be stored
        /// </summary>
        public TextParameterType SelectedTargetMappingParameterType
        {
            get => this.selectedTargetMappingParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedTargetMappingParameterType, value);
        }

        /// <summary>
        /// Gets or sets the selected target <see cref="ScalarParameterType"/> that is to select a <see cref="Parameter"/> in which the
        /// mapped (manual) value is to be stored
        /// </summary>
        public ScalarParameterType SelectedTargetValueParameterType
        {
            get => this.selectedTargetValueParameterType;
            set => this.RaiseAndSetIfChanged(ref this.selectedTargetValueParameterType, value);
        }

        /// <summary>
        /// Gets or sets a value indicating wether the <see cref="SourceParameterTypes"/> property has data.
        /// </summary>
        public bool ParameterTypesSelected
        {
            get => this.parameterTypesSelected;
            set => this.RaiseAndSetIfChanged(ref this.parameterTypesSelected, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="DataSourceManager"/>
        /// </summary>
        public DataSourceManager DataSourceManager
        {
            get => this.dataSourceManager;
            set => this.RaiseAndSetIfChanged(ref this.dataSourceManager, value);
        }

        /// <summary>
        /// Gets or sets the selected <see cref="DataRowView"/> 
        /// </summary>
        public DataRowView SelectedRow
        {
            get => this.selectedRow;
            set => this.RaiseAndSetIfChanged(ref this.selectedRow, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{Category}"/> from which the <see cref="ElementDefinition"/> <see cref="Category"/> can be selected
        /// </summary>
        public ReactiveList<Category> PossibleElementDefinitionCategory { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{ActualFiniteStateList}"/> from which the <see cref="ActualFiniteStateList"/> can be selected
        /// </summary>
        public ReactiveList<ActualFiniteStateList> PossibleActualFiniteStateList { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{TextParameterType}"/> from which the <see cref="TextParameterType"/>
        /// </summary>
        public ReactiveList<TextParameterType> PossibleTargetMappingParameterType { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{ScalarParameterType}"/> from which the <see cref="ScalarParameterType"/>
        /// </summary>
        public ReactiveList<ScalarParameterType> PossibleTargetValueParameterType { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to start the mapping - populate the row-view-models
        /// </summary>
        public ReactiveCommand<object, object> StartMappingCommand { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to clear the mapping settings
        /// </summary>
        public ReactiveCommand<object, object> ClearSettingsCommand { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to remove the <see cref="SelectedSourceParameterType"/> from  the mapping settings <see cref="SourceParameterTypes"/>
        /// </summary>
        public ReactiveCommand<object, object> RemoveSelectedSourceParameterTypeCommand { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to execute when the selected item from <see cref="SourceParameterTypes"/> was changed
        /// </summary>
        public ReactiveCommand<object, object> SelectedMappingParameterChangedCommand { get; protected set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to execute when changed values need to be saved
        /// </summary>
        public ReactiveCommand<Unit, object> SaveValuesCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the dock layout group target name to attach this panel to on opening
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.LeftGroup;

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteStartMappingCommand = this.WhenAnyValue(
                vm => vm.SelectedElementDefinitionCategory,
                vm => vm.SelectedActualFiniteStateList,
                vm => vm.SelectedTargetMappingParameterType,
                vm => vm.SelectedTargetValueParameterType,
                (a, b, c, d) =>
                    a != null && b != null && c != null && d != null);

            this.StartMappingCommand = ReactiveCommand.Create(canExecuteStartMappingCommand);
            this.StartMappingCommand.Subscribe(_ => this.ExecuteStartMappingCommand());

            this.ClearSettingsCommand = ReactiveCommand.Create();
            this.ClearSettingsCommand.Subscribe(_ => this.ExecuteClearSettingsCommand());

            var canExecuteRemoveSelectedSourceParameterTypeCommand =
                this.WhenAnyValue(vm => vm.SelectedSourceParameterType)
                    .Select(x => x != null);

            this.RemoveSelectedSourceParameterTypeCommand = ReactiveCommand.Create(canExecuteRemoveSelectedSourceParameterTypeCommand);
            this.RemoveSelectedSourceParameterTypeCommand.Subscribe(_ => this.ExecuteRemoveSelectedSourceParameterCommand());

            this.SelectedMappingParameterChangedCommand = ReactiveCommand.Create();
            this.SelectedMappingParameterChangedCommand.Subscribe(this.ExecuteSelectedMappingParameterChangedCommand);

            this.SaveValuesCommand = ReactiveCommand.CreateFromTask(_ => this.ExecuteSaveValuesCommand(), RxApp.MainThreadScheduler);
        }

        /// <summary>
        /// Executes when the selected row changes
        /// </summary>
        /// <param name="dataRowView">The <see cref="DataRowView"/></param>
        private void SelectedRowChanged(DataRowView dataRowView)
        {
            if (dataRowView == null)
            {
                this.SelectedThing = null;
                return;
            }

            var thing = this.DataSourceManager.GetThingByDataRow<Thing>(dataRowView.Row);

            if (thing is ElementDefinition elementDefinition)
            {
                this.SelectedThing = new DummyToolTipRowViewModel<ElementDefinition>(elementDefinition, this.Session);
            }

            if (thing is ElementUsage elementUsage)
            {
                this.SelectedThing = new DummyToolTipRowViewModel<ElementUsage>(elementUsage, this.Session);
            }

            if (thing is ParameterOverride parameterOverride)
            {
                if (dataRowView[DataSourceManager.TypeColumnName].ToString() == DataSourceManager.ParameterMappingType)
                {
                    this.SelectedThing = new DummyToolTipRowViewModel<ParameterOverride>(parameterOverride, this.Session)
                    {
                        Tooltip = $"Mapping Parameter: \n{parameterOverride.Tooltip()}"
                    };
                }

                if (dataRowView[DataSourceManager.TypeColumnName].ToString() == DataSourceManager.ParameterValueType)
                {
                    this.SelectedThing = new DummyToolTipRowViewModel<ParameterOverride>(parameterOverride, this.Session)
                    {
                        Tooltip = $"Value Parameter: \n{parameterOverride.Tooltip()}"
                    };
                }
            }
        }

        /// <summary>
        /// Executes when the user wants to save changes to the data store.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task ExecuteSaveValuesCommand()
        {
            var result = await this.DataSourceManager.TrySaveValues(this.Session);

            if (result.HasChanges && result.Result)
            {
                await this.StartMappingCommand.ExecuteAsync();
            }
        }

        /// <summary>
        /// Executes when the <see cref="SelectedMappingParameterChangedCommand"/> was fired.
        /// </summary>
        /// <param name="obj"></param>
        private void ExecuteSelectedMappingParameterChangedCommand(object obj)
        {
            if (obj == null)
            {
                return;
            }

            var (row, property, parameterTypeIid) = ((DataRowView DataRow, string Property, string Value))obj;

            this.DataSourceManager.SetValue(property, row.Row, parameterTypeIid);

            var valueRow = this.DataSourceManager.GetValueRowByMappingRow(row.Row);

            if (valueRow != null)
            {
                var newPropertyValue =
                    string.IsNullOrWhiteSpace(parameterTypeIid)
                        ? null
                        : this.DataSourceManager.GetElementDefinitionParameterValueForDataRow(row.Row, new Guid(parameterTypeIid));

                this.DataSourceManager.SetValue(property, valueRow, newPropertyValue);

                var newPropertyCustomName =
                    string.IsNullOrWhiteSpace(parameterTypeIid)
                        ? null
                        : this.DataSourceManager.GetElementDefinitionParameterCustomShortNameForDataRow(row.Row, new Guid(parameterTypeIid));

                this.DataSourceManager.SetValue(this.DataSourceManager.GetShortNameColumnName(property), valueRow, newPropertyCustomName);
            }
        }

        /// <summary>
        /// Removes a <see cref="ParameterType"/> from <see cref="SourceParameterTypes"/>.
        /// </summary>
        private void ExecuteRemoveSelectedSourceParameterCommand()
        {
            if (this.SelectedSourceParameterType == null)
            {
                return;
            }

            if (!this.SourceParameterTypes.Contains(this.SelectedSourceParameterType))
            {
                return;
            }

            this.SourceParameterTypes.Remove(this.SelectedSourceParameterType);
            this.SelectedSourceParameterType = this.SourceParameterTypes.FirstOrDefault();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
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

            this.PopulatePossibleElementDefinitionCategory();
            this.PopulatePossibleActualFiniteStateList();
            this.PopulatePossibleTargetMappingParameterType();
            this.PopulatePossibleTargetValueParameterType();
        }

        /// <summary>
        /// Populates the <see cref="PossibleElementDefinitionCategory"/> with <see cref="Category"/> applicable to <see cref="ClassKind.ElementDefinition"/>
        /// </summary>
        private void PopulatePossibleElementDefinitionCategory()
        {
            logger.Debug("Populate PossibleElementDefinitionCategory");

            this.PossibleElementDefinitionCategory.Clear();
            var model = (EngineeringModel)this.Thing.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedCategories = new List<Category>(mrdl.DefinedCategory.Where(c => c.PermissibleClass.Contains(ClassKind.ElementDefinition)));
            allowedCategories.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory).Where(c => c.PermissibleClass.Contains(ClassKind.ElementDefinition)));

            this.PossibleElementDefinitionCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));

            logger.Debug($"Populated list with {this.PossibleElementDefinitionCategory.Count} PossibleElementDefinitionCategory");
        }

        /// <summary>
        /// Populates the <see cref="PossibleActualFiniteStateList"/> with <see cref="ActualFiniteStateList"/> in the current <see cref="Iteration"/>
        /// </summary>
        private void PopulatePossibleActualFiniteStateList()
        {
            logger.Debug("Populate PossibleActualFiniteStateList");

            this.PossibleActualFiniteStateList.Clear();

            this.PossibleActualFiniteStateList.AddRange(this.Thing.ActualFiniteStateList.OrderBy(x => x.ShortName));

            logger.Debug($"Populated list with {this.PossibleActualFiniteStateList.Count} PossibleActualFiniteStateList");
        }

        /// <summary>
        /// Populates the <see cref="PossibleTargetMappingParameterType"/> with <see cref="ScalarParameterType"/>
        /// </summary>
        private void PopulatePossibleTargetMappingParameterType()
        {
            logger.Debug("Populate PossibleTargetMappingParameterType");

            this.PossibleTargetMappingParameterType.Clear();
            var model = (EngineeringModel)this.Thing.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedParameterTypes = new List<TextParameterType>(mrdl.ParameterType.OfType<TextParameterType>());
            allowedParameterTypes.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType).OfType<TextParameterType>());

            this.PossibleTargetMappingParameterType.AddRange(allowedParameterTypes.OrderBy(c => c.ShortName));

            logger.Debug($"Populated list with {this.PossibleTargetMappingParameterType.Count} PossibleTargetMappingParameterType");
        }

        /// <summary>
        /// Populates the <see cref="PossibleTargetValueParameterType"/> with <see cref="ScalarParameterType"/>
        /// </summary>
        private void PopulatePossibleTargetValueParameterType()
        {
            logger.Debug("Populate PossibleTargetValueParameterType");

            this.PossibleTargetValueParameterType.Clear();
            var model = (EngineeringModel)this.Thing.Container;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedParameterTypes = new List<ScalarParameterType>(mrdl.ParameterType.OfType<ScalarParameterType>());
            allowedParameterTypes.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType).OfType<ScalarParameterType>());

            this.PossibleTargetValueParameterType.AddRange(allowedParameterTypes.OrderBy(c => c.Name));

            logger.Debug($"Populated list with {this.PossibleTargetValueParameterType.Count} PossibleTargetValueParameterType");
        }

        /// <summary>
        /// Executes the <see cref="ClearSettingsCommand"/>
        /// </summary>
        private void ExecuteClearSettingsCommand()
        {
            logger.Debug("Clear settings - select items");

            this.SelectedElementDefinitionCategory = null;
            this.SelectedActualFiniteStateList = null;
            this.SourceParameterTypes.Clear();
            this.SelectedTargetMappingParameterType = null;
            this.SelectedTargetValueParameterType = null;
            this.SelectedSourceParameterType = null;

            logger.Debug("Clear data source");

            this.DataSourceManager = new DataSourceManager();
        }

        /// <summary>
        /// Executes the <see cref="ExecuteStartMappingCommand"/>
        /// </summary>
        private void ExecuteStartMappingCommand()
        {
            logger.Debug("start mapping");

            this.DataSourceManager = new DataSourceManager(
                this.Thing,
                this.selectedElementDefinitionCategory,
                this.selectedActualFiniteStateList,
                this.sourceParameterTypes,
                this.selectedTargetMappingParameterType,
                this.selectedTargetValueParameterType);

            try
            {
                var autoAddedSourceParameterTypes = this.DataSourceManager.SourceParameterTypes.Except(this.sourceParameterTypes).ToList();

                if (autoAddedSourceParameterTypes.Any())
                {
                    this.sourceParameterTypes.AddRange(autoAddedSourceParameterTypes);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Execute start mapping caused an error:");
                throw;
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
            try
            {
                logger.Trace("drag over {0}", dropInfo.TargetItem);

                if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> tuple)
                {
                    // check if parameter type is in the chain of rdls
                    var model = (EngineeringModel)this.Thing.TopContainer;
                    var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
                    var rdlChains = new List<ReferenceDataLibrary> { mrdl };
                    rdlChains.AddRange(mrdl.RequiredRdls);

                    if (!rdlChains.Contains(tuple.Item1.Container))
                    {
                        dropInfo.Effects = DragDropEffects.None;
                        logger.Warn("A parameter with the current parameter type cannot be created as the parameter type does not belong to the available libraries.");
                        return;
                    }

                    var parameterType = tuple.Item1;

                    if (!this.AllowAddAsSourceParameterType(parameterType))
                    {
                        dropInfo.Effects = DragDropEffects.None;
                        return;
                    }

                    // A parameter that references the drag-over ParameterType already exists
                    if (this.SourceParameterTypes.Any(x => x == parameterType))
                    {
                        logger.Warn("A parameter with the current parameter type already exists.");
                        dropInfo.Effects = DragDropEffects.None;
                        return;
                    }

                    dropInfo.Effects = DragDropEffects.Copy;
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
        /// Checks if adding a <see cref="ParameterType"/> can be used as source parameter type
        /// </summary>
        /// <param name="parameterType">The <see cref="ParameterType"/></param>
        /// <returns>true if allowed, otherwise false.</returns>
        private bool AllowAddAsSourceParameterType(ParameterType parameterType)
        {
            var allowAdd = parameterType is ScalarParameterType;

            if (parameterType is CompoundParameterType compoundParameterType)
            {
                allowAdd = compoundParameterType.Component.Count > 1
                           && compoundParameterType.Component[0].ParameterType is TextParameterType
                           && compoundParameterType.Component[1].ParameterType is ScalarParameterType;
            }

            return allowAdd;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
#pragma warning disable 1998
        public async Task Drop(IDropInfo dropInfo)
#pragma warning restore 1998
        {
            if (dropInfo.Payload is Tuple<ParameterType, MeasurementScale> parameterTypeAndScale)
            {
                // check if parameter type is in the chain of rdls
                var model = (EngineeringModel)this.Thing.TopContainer;
                var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();
                var rdlChains = new List<ReferenceDataLibrary> { mrdl };
                rdlChains.AddRange(mrdl.RequiredRdls);

                if (!rdlChains.Contains(parameterTypeAndScale.Item1.Container))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                var parameterType = parameterTypeAndScale.Item1;

                if (this.SourceParameterTypes.Any(x => x == parameterType))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                if (!this.AllowAddAsSourceParameterType(parameterType))
                {
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                try
                {
                    this.SourceParameterTypes.Add(parameterType);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "drop caused an error");
                }
            }
        }
    }
}
