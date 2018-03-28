// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceExportViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;
    using Microsoft.Practices.ServiceLocation;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="DataSourceExportViewModel"/> is to allow a user to select an <see cref="IDal"/> implementation
    /// and provide Credentials to export a given session.
    /// </summary>
    public class DataSourceExportViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The available <see cref="IDal"/> combined with <see cref="INameMetaData"/>
        /// </summary>
        private List<Lazy<IDal, IDalMetaData>> dals;

        /// <summary>
        /// The selected <see cref="Session"/> that will be exported.
        /// </summary>
        private ISession selectedSession;

        /// <summary>
        /// Backing field for the <see cref="SelectedDal"/> property.
        /// </summary>
        private IDalMetaData selectedDal;

        /// <summary>
        /// Backing field for the <see cref="Password"/> property.
        /// </summary>
        private string password;

        /// <summary>
        /// Backing field for the <see cref="PasswordRetype"/> property.
        /// </summary>
        private string passwordRetype;

        /// <summary>
        /// Backing field for the <see cref="Uri"/> property.
        /// </summary>
        private string path;

        /// <summary>
        /// The available dals.
        /// </summary>
        private List<IDalMetaData> availableDals;

        /// <summary>
        /// The open sessions.
        /// </summary>
        private ReactiveList<ISession> openSessions;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        protected readonly IDialogNavigationService DialogNavigationService;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private readonly IOpenSaveFileDialogService openSaveFileDialogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceExportViewModel"/> class.
        /// </summary>
        /// <param name="sessions">
        /// The sessions that encapsulate <see cref="IDal"/>s and the associated <see cref="Assembler"/>.
        /// </param>
        /// <param name="openSaveFileDialogService">
        /// The file Dialog Service.
        /// </param>
        public DataSourceExportViewModel(IEnumerable<ISession> sessions, IOpenSaveFileDialogService openSaveFileDialogService)
        {
            if (openSaveFileDialogService == null)
            {
                throw new ArgumentNullException("The openSaveFileDialogService may not be null", "openSaveFileDialogService");
            }

            this.openSaveFileDialogService = openSaveFileDialogService;
            this.AvailableDals = new List<IDalMetaData>();
            this.OpenSessions = new ReactiveList<ISession>(sessions);
            this.DialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            this.IsBusy = false;

            this.OpenSessions.ChangeTrackingEnabled = true;

            var canOk = this.WhenAnyValue(
                vm => vm.PasswordRetype,
                vm => vm.Password,
                vm => vm.SelectedDal,
                vm => vm.SelectedSession,
                vm => vm.Path,
                (passwordRetype, password, selecteddal, selectedsession, path) => selecteddal != null && selectedsession != null 
                    && !string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(password) && password == passwordRetype);
            
            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.BrowseCommand = ReactiveCommand.Create();
            this.BrowseCommand.Subscribe(_ => this.ExecuteBrowse());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.ResetProperties();
        }
        
        /// <summary>
        /// Gets or sets the selected Data-Source Kind
        /// </summary>
        public IDalMetaData SelectedDal
        {
            get
            {
                return this.selectedDal;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedDal, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Session"/> to export.
        /// </summary>
        public ISession SelectedSession
        {
            get
            {
                return this.selectedSession;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedSession, value);
            }
        }

        /// <summary>
        /// Gets or sets the retyped password
        /// </summary>
        public string PasswordRetype
        {
            get
            {
                return this.passwordRetype;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.passwordRetype, value);
            }
        }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.password, value);
            }
        }

        /// <summary>
        /// Gets or sets the Password
        /// </summary>
        public string Path
        {
            get
            {
                return this.path;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.path, value);
            }
        }

        /// <summary>
        /// Gets the available data source kinds
        /// </summary>
        public List<IDalMetaData> AvailableDals
        {
            get
            {
                return this.availableDals;
            }

            private set
            {
                this.RaiseAndSetIfChanged(ref this.availableDals, value);
            }
        }

        /// <summary>
        /// Gets the available sessions
        /// </summary>
        public ReactiveList<ISession> OpenSessions
        {
            get
            {
                return this.openSessions;
            }

            private set
            {
                this.RaiseAndSetIfChanged(ref this.openSessions, value);
            }
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the Browse Command
        /// </summary>
        public ReactiveCommand<object> BrowseCommand { get; private set; }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        private async void ExecuteOk()
        {
            this.IsBusy = true;
            this.LoadingMessage = "Exporting...";

            try
            {
                var creds = new Credentials(this.selectedSession.Credentials.UserName, this.Password, new Uri(this.Path));
                
                // TODO: change this to allow (file) dal selection for export
                var dal = this.dals.Single(x => x.Metadata.DalType == DalType.File);
                var dalInstance = (IDal)Activator.CreateInstance(dal.Value.GetType());
                
                var fileExportSession = new Session(dalInstance, creds);

                // create write 
                var operationContainers = new List<OperationContainer>();

                // TODO: allow iteration setup selection by user
                var openIterations = this.selectedSession.OpenIterations.Select(x => x.Key);
                foreach (var iteration in openIterations)
                {
                    var transactionContext = TransactionContextResolver.ResolveContext(iteration);
                    var operationContainer = new OperationContainer(transactionContext.ContextRoute());
                    var dto = iteration.ToDto();
                    var operation = new Operation(null, dto, OperationKind.Create);
                    operationContainer.AddOperation(operation);
                    operationContainers.Add(operationContainer);
                }

                var result = await dalInstance.Write(operationContainers);
                this.DialogResult = new BaseDialogResult(true);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteBrowse()
        {
            this.Path = this.openSaveFileDialogService.GetSaveFileDialog("Untitled", ".zip", "ZIP Archives (.zip)|*.zip", this.Path, 1);
        }

        /// <summary>
        /// Resets all the properties and populates the <see cref="AvailableDals"/> List
        /// and sets the <see cref="SelectedDal"/> to the first in the <see cref="AvailableDals"/>
        /// </summary>
        private void ResetProperties()
        {
            this.AvailableDals = new List<IDalMetaData>();
#if DEBUG
            this.PasswordRetype = "pass";
            this.Password = "pass";
            this.Path = "C:\\test\\doubletest.zip";
#else
            this.PasswordRetype = string.Empty;
            this.Password = string.Empty;
            this.Path = string.Empty;
#endif
            this.ErrorMessage = string.Empty;

            this.dals = ServiceLocator.Current.GetInstance<AvailableDals>().DataAccessLayerKinds;

            foreach (var dal in this.dals)
            {
                this.AvailableDals.Add(dal.Metadata);
            }

            if (this.AvailableDals.Any())
            {
                this.SelectedDal = this.AvailableDals.First(); 
            }

            if (this.OpenSessions.Any())
            {
                this.SelectedSession = this.OpenSessions.First(); 
            }
        }
    }
}
