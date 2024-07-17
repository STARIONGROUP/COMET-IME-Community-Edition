// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementSpecificationMappingDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Dal;
    using CDP4Dal.Operations;
    
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.ViewModels;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.Settings.JsonConverters;

    using CommonServiceLocator;

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
        public ReactiveCommand<Unit, Unit> BackCommand { get; private set; }

        /// <summary>
        /// Gets the "Ok" <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Save mapping configuration <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveMappingCommand { get; private set; }

        /// <summary>
        /// Gets or sets the Inspect Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectCommand { get; protected set; }

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
                var clonedExternalIdentifierMap = externalIdentifierMap.Clone(false);
                transaction.CreateOrUpdate(clonedExternalIdentifierMap);
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

            this.BackCommand = ReactiveCommandCreator.Create(this.ExecuteBackCommand);

            this.OkCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteOkCommand);
            
            this.OkCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
            {
                this.ErrorMessage = x.Message;
                this.IsBusy = false;
            });

            this.SaveMappingCommand = ReactiveCommandCreator.Create(this.SaveMappingCommandExecute);

            this.InspectCommand = ReactiveCommandCreator.Create(this.ExecuteInspectCommand, canExecuteCommand);
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
                    ReqIfJsonConverterUtility.BuildConverters());

                this.dialogNavigationService.NavigateModal(saveDialog);
            }
            else
            {
                var settings = this.pluginSettingsService.Read<RequirementsModuleSettings>();
                
                var configurationToUpdateIndex = settings.SavedConfigurations.IndexOf(
                    settings.SavedConfigurations.Single(x => x.Id == this.importMappingConfiguration.Id));
                
                settings.SavedConfigurations[configurationToUpdateIndex] = this.importMappingConfiguration;

                this.pluginSettingsService.Write(settings, ReqIfJsonConverterUtility.BuildConverters());
            }
        }

        /// <summary>
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