// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSelectionRibbonViewModel.cs" company="RHEA System S.A.">
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
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows.Input;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The View-Model for <see cref="Views.SiteRdlSelectionRibbon"/>
    /// </summary>
    public class SiteRdlSelectionRibbonViewModel : ReactiveObject
    {
        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        private readonly IThingDialogNavigationService thingDialogNavigationService = ServiceLocator.Current.GetInstance<IThingDialogNavigationService>();

        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private ObservableAsPropertyHelper<bool> hasSession;

        /// <summary>
        /// Backing field for <see cref="HasOpenSiteRdl"/>
        /// </summary>
        private bool hasOpenSiteRdl; 

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlSelectionRibbonViewModel"/> class.
        /// </summary>
        public SiteRdlSelectionRibbonViewModel()
        {
            this.thingDialogNavigationService = ServiceLocator.Current.GetInstance<IThingDialogNavigationService>();

            this.OpenSessions = new ReactiveList<ISession>();
            this.OpenSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession);
            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.OpenSiteRdlSelectorCommand = ReactiveCommandCreator.Create(this.WhenAnyValue(x => x.HasSession));
            this.OpenSiteRdlSelectorCommand.Subscribe(_ => this.ExecuteOpenSiteRdlSelectorCommand());

            this.CloseSiteRdlCommand = ReactiveCommandCreator.Create(this.WhenAnyValue(x => x.HasOpenSiteRdl));
            this.CloseSiteRdlCommand.Subscribe(_ => this.ExecuteCloseSiteRdlSelectorCommand());
        }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSession
        {
            get { return this.hasSession.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether there are open <see cref="SiteReferenceDataLibrary"/> in any <see cref="ISession"/>
        /// </summary>
        public bool HasOpenSiteRdl
        {
            get { return this.hasOpenSiteRdl; }
            private set { this.RaiseAndSetIfChanged(ref this.hasOpenSiteRdl, value); }
        }

        /// <summary>
        /// Gets a list of open <see cref="ISession"/>s
        /// </summary>
        public ReactiveList<ISession> OpenSessions { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to select and open a site RDL Selection Window
        /// </summary>
        public ReactiveCommand<Unit, Unit> OpenSiteRdlSelectorCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to select and close Site Rdls
        /// </summary>
        public ReactiveCommand<Unit, Unit> CloseSiteRdlCommand { get; private set; }
        
        /// <summary>
        /// Executes the <see cref="OpenSiteRdlSelectorCommand"/> command
        /// </summary>
        private void ExecuteOpenSiteRdlSelectorCommand()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            var viewmodel = new SiteRdlOpeningDialogViewModel(this.OpenSessions);
            
            dialogService.NavigateModal(viewmodel);
        }

        /// <summary>
        /// Executes the <see cref="CloseSiteRdlCommand"/> command
        /// </summary>
        private void ExecuteCloseSiteRdlSelectorCommand()
        {
            var viewmodel = new SiteRdlClosingDialogViewModel(this.OpenSessions);
            this.dialogNavigationService.NavigateModal(viewmodel);
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Session"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (sessionChange.Status == SessionStatus.Open)
            {
                this.OpenSessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.OpenSessions.Remove(sessionChange.Session);
            }

            this.HasOpenSiteRdl = this.OpenSessions.SelectMany(x => x.OpenReferenceDataLibraries).Any();
        }
    }
}