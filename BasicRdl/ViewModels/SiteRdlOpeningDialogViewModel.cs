// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlOpeningDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using ReactiveUI;
    using Views;

    /// <summary>
    /// The ViewModel for the <see cref="SiteRdlOpeningDialog"/> Dialog
    /// </summary>
    public class SiteRdlOpeningDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlOpeningDialogViewModel"/> class. 
        /// </summary>
        /// <param name="sessionAvailable">The sessions Available</param>
        public SiteRdlOpeningDialogViewModel(IEnumerable<ISession> sessionAvailable)
        {
            this.IsBusy = false;

            this.SessionsAvailable = new DisposableReactiveList<SiteRdlSessionRowViewModel>();
            this.SelectedSiteRdls = new ReactiveList<object>();
            this.SelectedSiteRdls.ChangeTrackingEnabled = true;

            this.InitializeReactiveCommands();

            this.SelectedSiteRdls.ItemsAdded.Subscribe(this.FilterSiteRdlSelectionItems);

            this.PopulateSessionsRowViewModel(sessionAvailable);
        }

        /// <summary>
        /// Gets the list of <see cref="SiteRdlSessionRowViewModel"/> available
        /// </summary>
        public DisposableReactiveList<SiteRdlSessionRowViewModel> SessionsAvailable { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="SiteRdlRowViewModel"/> selected
        /// </summary>
        public ReactiveList<object> SelectedSiteRdls { get; private set; }

        /// <summary>
        /// Gets the Select <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit> OpenCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// The initialize reactive commands.
        /// </summary>
        private void InitializeReactiveCommands()
        {
            var canOk = this.WhenAnyValue(x => x.SelectedSiteRdls.Count, count => count != 0);

            this.OpenCommand = ReactiveCommand.CreateAsyncTask(canOk, x => this.ExecuteOpen(), RxApp.MainThreadScheduler);
            this.OpenCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
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
        private async Task ExecuteOpen()
        {
            var tasks = new List<Task>();

            this.IsBusy = true;
            this.LoadingMessage = "Opening...";

            var siteRdlRows = new List<SiteRdlRowViewModel>(this.SelectedSiteRdls.Cast<SiteRdlRowViewModel>().ToList());
            
            foreach (var siteRdlrow in siteRdlRows)
            {
                // Open this SiteRDL
                var siteDir = siteRdlrow.Thing.Container as SiteDirectory;
                var session = this.SessionsAvailable.Select(s => s.Session).Single(x => x.RetrieveSiteDirectory() == siteDir);

                if (session.OpenReferenceDataLibraries.Any(x => x.Iid.Equals(siteRdlrow.Thing.Iid)))
                {
                    continue;
                }

                var rdl = new SiteReferenceDataLibrary { Iid = siteRdlrow.Thing.Iid, Name = siteRdlrow.Name, ShortName = siteRdlrow.ShortName, Container = siteDir };

                tasks.Add(session.Read(rdl));
            }

            await Task.WhenAll(tasks);
            this.IsBusy = false;
            this.DialogResult = new SiteRdlSelectionDialogResult(true);
        }

        /// <summary>
        /// Executes the cancel command
        /// </summary>
        private void ExecuteCancel()
        {
            foreach (var siteDirectory in this.SelectedSiteRdls.Select(sr => sr as SiteRdlRowViewModel).Select(s => s.Thing.Container as SiteDirectory))
            {
                var session = this.SessionsAvailable.Select(s => s.Session).Single(x => x.RetrieveSiteDirectory() == siteDirectory);
                session.Cancel();
            }

            this.DialogResult = new SiteRdlSelectionDialogResult(false);
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
                this.SessionsAvailable.Add(new SiteRdlSessionRowViewModel(session.RetrieveSiteDirectory(), session, null));
            }
        }

        /// <summary>
        /// Filters the <see cref="SelectedSiteRdls"/> added items to only select <see cref="SiteRdlRowViewModel"/>s
        /// </summary>
        /// <param name="row">the added row-view-model</param>
        private void FilterSiteRdlSelectionItems(object row)
        {
            var siteRdlRow = row as SiteRdlRowViewModel;
            if (siteRdlRow != null)
            {
                return;
            }

            // Can't remove directly from the reactiveList: KeyNotFoundException
            var list = this.SelectedSiteRdls.ToList();
            list.Remove(row);

            this.SelectedSiteRdls.Clear();
            this.SelectedSiteRdls.AddRange(list);
        }
    }
}