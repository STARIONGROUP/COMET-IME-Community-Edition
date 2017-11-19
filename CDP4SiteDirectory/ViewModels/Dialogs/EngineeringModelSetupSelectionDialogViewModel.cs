// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupSelectionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The ViewModel that supports the selection of an <see cref="EngineeringModelSetup"/>
    /// </summary>
    public class EngineeringModelSetupSelectionDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedSession"/> property.
        /// </summary>
        private ISession selectedSession;

        /// <summary>
        /// Backing field for the <see cref="SelectedEngineeringModelSetup"/> property.
        /// </summary>
        private EngineeringModelSetup selectedEngineeringModelSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupSelectionDialogViewModel"/> class.
        /// </summary>
        /// <param name="availableSessions">
        /// The available sessions.
        /// </param>
        public EngineeringModelSetupSelectionDialogViewModel(IEnumerable<ISession> availableSessions)
        {
            this.AvailableSessions = availableSessions;
            if (this.AvailableSessions.Count() == 1)
            {
                this.SelectedSession = this.AvailableSessions.Single();
            }

            this.PossibleEngineeringModelSetups = new ReactiveList<EngineeringModelSetup>();
            this.WhenAnyValue(x => x.SelectedSession).Subscribe(this.PopulateEngineeringModelSetups);

            var canOk = this.WhenAnyValue(vm => vm.SelectedSession, vm => vm.SelectedEngineeringModelSetup, (session, model) => session != null && model != null);
            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
        }

        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle
        {
            get { return "Select an Engineering Model"; }
        }

        /// <summary>
        /// Gets the list of available sessions
        /// </summary>
        public IEnumerable<ISession> AvailableSessions { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="ISession"/>
        /// </summary>
        public ISession SelectedSession
        {
            get { return this.selectedSession; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSession, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup SelectedEngineeringModelSetup
        {
            get { return this.selectedEngineeringModelSetup; }
            set { this.RaiseAndSetIfChanged(ref this.selectedEngineeringModelSetup, value); }
        }

        /// <summary>
        /// Gets the Possible <see cref="EngineeringModelSetup"/>s that can be chosen from.
        /// </summary>
        public ReactiveList<EngineeringModelSetup> PossibleEngineeringModelSetups { get; private set; }

        /// <summary>
        /// Gets the Select <see cref="ReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Executes the cancel <see cref="ReactiveCommand"/>
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new EngineeringModelSetupSelectionResult(false, null, null);
        }

        /// <summary>
        /// Executes the ok <see cref="ReactiveCommand"/>
        /// </summary>
        private void ExecuteOk()
        {
            this.DialogResult = new EngineeringModelSetupSelectionResult(true, this.selectedSession, this.SelectedEngineeringModelSetup);
        }

        /// <summary>
        /// Populates the <see cref="PossibleEngineeringModelSetups"/> based on the selected <see cref="ISession"/>
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the <see cref="PossibleEngineeringModelSetups"/> needs to be loaded.
        /// </param>
        private void PopulateEngineeringModelSetups(ISession session)
        {
            if (session == null)
            {
                return;
            }

            var retrieveSiteDirectory = session.Assembler.RetrieveSiteDirectory();
            this.PossibleEngineeringModelSetups.AddRange(retrieveSiteDirectory.Model);
        }
    }
}
