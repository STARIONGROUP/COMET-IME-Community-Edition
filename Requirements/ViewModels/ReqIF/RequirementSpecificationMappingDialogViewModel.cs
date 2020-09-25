// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementSpecificationMappingDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Web.UI;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.Settings.JsonConverters;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// The requirement specification mapping dialog view model.
    /// </summary>
    [DialogViewModelExport("RequirementSpecificationMappingDialogViewModel", "The dialog used to map the Reqif DatatypeDefinition to ParameterType.")]
    public class RequirementSpecificationMappingDialogViewModel : ReqIfMappingDialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="SelectedThing"/>
        /// </summary>
        private IRowViewModelBase<Thing> selectedThing;

        /// <summary>
        /// The <see cref="ThingFactory"/>
        /// </summary>
        private readonly ThingFactory thingFactory;
        
        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The <see cref="IPluginSettingsService"/>
        /// </summary>
        private readonly IPluginSettingsService pluginSettingsService;

        /// <summary>
        /// The <see cref="ImportMappingConfiguration"/>
        /// </summary>
        private readonly ImportMappingConfiguration importMappingConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementSpecificationMappingDialogViewModel"/> class.
        /// Used by MEF.
        /// </summary>
        public RequirementSpecificationMappingDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementSpecificationMappingDialogViewModel"/> class.
        /// </summary>
        /// <param name="thingFactory">The <see cref="ThingFactory"/></param>
        /// <param name="iteration">The iteration</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The thing Dialog Navigation Service</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="lang">The current langguage code</param>
        /// <param name="importMappingConfiguration">The <see cref="ImportMappingConfiguration"/></param>
        public RequirementSpecificationMappingDialogViewModel(ThingFactory thingFactory, Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, string lang, ImportMappingConfiguration importMappingConfiguration)
            : base(iteration, session, thingDialogNavigationService, lang)
        {
            this.PreviewRows = new DisposableReactiveList<IRowViewModelBase<Thing>>();
            this.thingFactory = thingFactory;
            this.dialogNavigationService = dialogNavigationService;
            this.pluginSettingsService = ServiceLocator.Current.GetInstance<IPluginSettingsService>();
            this.importMappingConfiguration = importMappingConfiguration;

            this.PopulateRows();

            this.InitializeCommands();
        }

        /// <summary>
        /// Gets the back <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> BackCommand { get; private set; }

        /// <summary>
        /// Gets the "Ok" <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Save mapping configuration <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> SaveMappingCommand { get; private set; }

        /// <summary>
        /// Gets or sets the Inspect Command
        /// </summary>
        public ReactiveCommand<object> InspectCommand { get; protected set; }

        /// <summary>
        /// Gets the <see cref="RequirementsSpecificationRowViewModel"/> rows
        /// </summary>
        public DisposableReactiveList<IRowViewModelBase<Thing>> PreviewRows { get; private set; }

        /// <summary>
        /// Gets or sets the selected row
        /// </summary>
        public IRowViewModelBase<Thing> SelectedThing
        {
            get { return this.selectedThing; }
            set { this.RaiseAndSetIfChanged(ref this.selectedThing, value); }
        }

        /// <summary>
        /// Create a <see cref="ThingTransaction"/> object from the <see cref="RequirementsSpecification"/>
        /// </summary>
        /// <returns>The <see cref="ThingTransaction"/></returns>
        private ThingTransaction CreateTransaction()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.IterationClone);
            var transaction = new ThingTransaction(transactionContext, this.IterationClone);

            foreach (var externalIdentifierMap in this.IterationClone.ExternalIdentifierMap)
            {
                transaction.CreateOrUpdate(externalIdentifierMap);
            }

            foreach (var specMap in this.thingFactory.SpecificationMap)
            {
                transaction.CreateDeep(specMap.Value);
            }

            foreach (var relationshipMap in this.thingFactory.SpecRelationMap)
            {
                transaction.CreateDeep(relationshipMap.Value);
            }

            foreach (var relationshipMap in this.thingFactory.RelationGroupMap)
            {
                transaction.CreateDeep(relationshipMap.Value);
            }

            return transaction;
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private async Task ExecuteOkCommand()
        {
            var transaction = this.CreateTransaction();
            var operationContainer = transaction.FinalizeTransaction();

            this.IsBusy = true;
            await this.Session.Write(operationContainer);
            this.IsBusy = false;
            this.DialogResult = new MappingDialogNavigationResult(true, true);
        }

        /// <summary>
        /// Execute the <see cref="BackCommand"/>
        /// </summary>
        private void ExecuteBackCommand()
        {
            this.DialogResult = new MappingDialogNavigationResult(false, true);
        }

        /// <summary>
        /// Executes the cancel <see cref="ICommand"/>
        /// </summary>
        protected override void ExecuteCancelCommand()
        {
            this.DialogResult = new MappingDialogNavigationResult(null, false);
        }

        /// <summary>
        /// The populate requirement specification rows.
        /// </summary>
        private void PopulateRows()
        {
            foreach (var map in this.thingFactory.SpecificationMap)
            {
                var row = new RequirementsSpecificationRowViewModel(map.Value, this.Session, null);
                this.PreviewRows.Add(row);
            }
            
            foreach (var keyValuePair in this.thingFactory.RelationGroupMap)
            {
                var row = new BinaryRelationshipRowViewModel(keyValuePair.Value, this.Session, null);
                this.PreviewRows.Add(row);
            }

            foreach (var keyValuePair in this.thingFactory.SpecRelationMap)
            {
                var row = new BinaryRelationshipRowViewModel(keyValuePair.Value, this.Session, null);
                this.PreviewRows.Add(row);
            }
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s
        /// </summary>
        private void InitializeCommands()
        {
            var canExecuteCommand = this.WhenAnyValue(
                vm => vm.Session,
                vm => vm.SelectedThing,
                (ses, selection) => ses != null && selection != null);

            this.BackCommand = ReactiveCommand.Create();
            this.BackCommand.Subscribe(_ => this.ExecuteBackCommand());

            this.OkCommand = ReactiveCommand.CreateAsyncTask(x => this.ExecuteOkCommand(), RxApp.MainThreadScheduler);
            
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
            {
                this.ErrorMessage = x.Message;
                this.IsBusy = false;
            });

            this.SaveMappingCommand = ReactiveCommand.Create();
            this.SaveMappingCommand.Subscribe(_ => this.SaveMappingCommandExecute());

            this.InspectCommand = ReactiveCommand.Create(canExecuteCommand);
            this.InspectCommand.Subscribe(_ => this.ExecuteInspectCommand());
        }

        /// <summary>
        /// Executes the <see cref="SaveMappingCommand"/>
        /// </summary>
        private void SaveMappingCommandExecute()
        {
            if (string.IsNullOrWhiteSpace(this.importMappingConfiguration.Name))
            {
                var saveDialog = new SavedConfigurationDialogViewModel<RequirementsModuleSettings>(
                    this.pluginSettingsService,
                    this.importMappingConfiguration,
                    ConverterExtensions.BuildConverters());

                this.dialogNavigationService.NavigateModal(saveDialog);
            }
            else
            {
                var settings = this.pluginSettingsService.Read<RequirementsModuleSettings>();
                
                var configurationToUpdateIndex = settings.SavedConfigurations.IndexOf(
                    settings.SavedConfigurations.Single(x => x.Id == this.importMappingConfiguration.Id));
                
                settings.SavedConfigurations[configurationToUpdateIndex] = this.importMappingConfiguration;

                this.pluginSettingsService.Write(settings, ConverterExtensions.BuildConverters());
            }
        }

        /// <summary>                                                                                                                                        = new SpecificationTypeMapConverter(this.reqIf, this.session, this.iteration);
        /// Execute the <see cref="InspectCommand"/>
        /// </summary>
        protected virtual void ExecuteInspectCommand()
        {
            this.ThingDialogNavigationService.Navigate(this.SelectedThing.Thing, null, this.Session, false, ThingDialogKind.Inspect, this.ThingDialogNavigationService, this.SelectedThing.Thing.Container);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected void Dispose(bool disposing)
        {
            foreach (var reqSpec in this.PreviewRows)
            {
                reqSpec.Dispose();
            }
        }
    }
}