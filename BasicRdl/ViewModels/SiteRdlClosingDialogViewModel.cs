// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlClosingDialogViewModel.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

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

    /// <summary>
    /// The ViewModel for the <see cref="Views.SiteRdlOpeningDialog"/> Dialog
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
            get => this.selectedSiteRdlToClose;
            set => this.RaiseAndSetIfChanged(ref this.selectedSiteRdlToClose, value);
        }

        /// <summary>
        /// Gets the Select <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CloseCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

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

            this.CloseCommand = ReactiveCommandCreator.CreateAsyncTask(
                this.ExecuteClose,
                this.WhenAnyValue(x => x.SelectedSiteRdlToClose).Select(x => x != null),
                RxApp.MainThreadScheduler);
            this.CloseCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
            {
                this.ErrorMessage = x.Message;
                this.IsBusy = false;
            });

            this.CancelCommand = ReactiveCommandCreator.Create();
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