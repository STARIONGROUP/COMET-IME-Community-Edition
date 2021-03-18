// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceExportViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4IME.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;

    using CDP4Composition.Exceptions;
    using CDP4Composition.Extensions;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Operations;
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
        /// The (injected) <see cref="DevExpress.Mvvm.IMessageBoxService"/>
        /// </summary>
        private readonly IMessageBoxService messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

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
                throw new ArgumentNullException(nameof(openSaveFileDialogService), "The openSaveFileDialogService may not be null.");
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
            var fileInfo = new FileInfo(this.Path);

            // check file exists and warn of override
            if (fileInfo.Exists)
            {
                var overrideMessageBox = this.messageBoxService.Show(caption: "File Exists", messageBoxText: "The selected output file already exists. Would you like to override?", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Question);

                if (overrideMessageBox == MessageBoxResult.No)
                {
                    return;
                }
            }

            // check directory exists
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                this.ErrorMessage = "The output directory does not exist.";
                return;
            }

            this.IsBusy = true;
            this.LoadingMessage = "Exporting...";

            try
            {
                var creds = new Credentials(this.selectedSession.Credentials.UserName, this.Password, new Uri(this.Path));

                var dal = this.dals.Single(x => x.Metadata == this.SelectedDal);
                var dalInstance = (IDal)Activator.CreateInstance(dal.Value.GetType());

                var fileExportSession = dalInstance.CreateSession(creds);

                // create write
                var operationContainers = new List<OperationContainer>();

                // TODO: GH#782 allow iteration setup selection by user
                var openIterations = this.selectedSession.OpenIterations.Select(x => x.Key).ToList();

                if (!openIterations.Any())
                {
                    throw new SessionHasNoIterationsException();
                }

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

            foreach (var dal in this.dals.Where(d => d.Metadata.DalType == DalType.File))
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
