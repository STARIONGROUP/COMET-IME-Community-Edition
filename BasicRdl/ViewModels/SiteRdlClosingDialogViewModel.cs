// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlClosingDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using ReactiveUI;
    using Views;

    /// <summary>
    /// The ViewModel for the <see cref="SiteRdlOpeningDialog"/> Dialog
    /// </summary>
    public class SiteRdlClosingDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="SelectedSiteRdlToClose"/>
        /// </summary>
        private IRowViewModelBase<Thing> selectedSiteRdlToClose;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlOpeningDialogViewModel"/> class. 
        /// </summary>
        /// <param name="sessionAvailable">The sessions Available</param>
        public SiteRdlClosingDialogViewModel(IEnumerable<ISession> sessionAvailable)
        {
            this.IsBusy = false;

            this.SessionsAvailable = new DisposableReactiveList<ClosingSiteRdlSessionRowViewModel>();
            this.InitializeReactiveCommands();
            this.PopulateSessionsRowViewModel(sessionAvailable);
        }

        /// <summary>
        /// Gets the list of <see cref="SiteRdlRowViewModel"/> available
        /// </summary>
        public DisposableReactiveList<ClosingSiteRdlSessionRowViewModel> SessionsAvailable { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="SiteReferenceDataLibrary"/> to close
        /// </summary>
        public IRowViewModelBase<Thing> SelectedSiteRdlToClose
        {
            get { return this.selectedSiteRdlToClose; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSiteRdlToClose, value); }
        }

        /// <summary>
        /// Gets the Select <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit> CloseCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// The initialize reactive commands.
        /// </summary>
        private void InitializeReactiveCommands()
        {
            this.WhenAnyValue(vm => vm.SelectedSiteRdlToClose).Subscribe(row =>
            {
                var siteRdlRow = row as SiteRdlRowViewModel;
                if (siteRdlRow == null || !siteRdlRow.CanClose)
                {
                    this.SelectedSiteRdlToClose = null;
                }
            });

            this.CloseCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.SelectedSiteRdlToClose).Select(x => x!= null), x => this.ExecuteClose(), RxApp.MainThreadScheduler);
            this.CloseCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
            {
                this.ErrorMessage = x.Message;
                this.IsBusy = false;
            });

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
        }

        /// <summary>
        /// Executes the Open command
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteClose()
        {
            this.IsBusy = true;
            this.LoadingMessage = "Closing...";

            var siteRdlrow = (SiteRdlRowViewModel)this.SelectedSiteRdlToClose;

            var session = siteRdlrow.Session;
            await session.CloseRdl(siteRdlrow.Thing);

            this.IsBusy = false;
            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Executes the cancel command
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Populates <see cref="SessionsAvailable"/> from the list of <see cref="Session"/>s available
        /// </summary>
        /// <param name="sessions">
        /// The sessions.
        /// </param>
        private void PopulateSessionsRowViewModel(IEnumerable<ISession> sessions)
        {
            foreach (var session in sessions)
            {
                this.SessionsAvailable.Add(new ClosingSiteRdlSessionRowViewModel(session.RetrieveSiteDirectory(), session, null));
            }
        }
    }
}