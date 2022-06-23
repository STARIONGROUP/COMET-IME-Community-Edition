// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportRibbonViewModel.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CommonServiceLocator;
    
    using ReactiveUI;
    
    using CDP4Dal;
    using CDP4Dal.Events;
    
    /// <summary>
    /// The view-model for the ribbon
    /// </summary>
    public class HtmlExportRibbonViewModel : ReactiveObject
    {
        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private readonly IOpenSaveFileDialogService openSaveFileDialogService;

        /// <summary>
        /// Backing field for <see cref="CanExport"/>
        /// </summary>
        private bool canExport;

        /// <summary>
        /// Creates a new instance of the <see cref="HtmlExportRibbonViewModel"/> class.
        /// </summary>
        public HtmlExportRibbonViewModel()
        {
            this.dialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            this.openSaveFileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

            this.Sessions = new ReactiveList<ISession>();
            this.Iterations = new ReactiveList<Iteration>();

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);
            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Iteration)).Subscribe(this.IterationEventHandler);

            this.Iterations.CountChanged.Subscribe(
                x =>
                    {
                        this.CanExport = x > 0;
                    });

            var isExportEnabled = this.WhenAnyValue(x => x.CanExport);

            this.ExportCommand = ReactiveCommandCreator.Create(this.ExecuteExportCommand, isExportEnabled);
        }

        /// <summary>
        /// Gets the List of <see cref="ISession"/> that are open
        /// </summary>
        public ReactiveList<ISession> Sessions { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="Iteration"/>s that are open
        /// </summary>
        public ReactiveList<Iteration> Iterations { get; private set; }

        /// <summary>
        /// Gets the HTML export <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> ExportCommand { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the export command is enabled
        /// </summary>
        public bool CanExport
        {
            get { return this.canExport; }
            set { this.RaiseAndSetIfChanged(ref this.canExport, value); }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Iteration"/>s added
        /// </summary>
        /// <param name="iterationEvent">the <see cref="ObjectChangedEvent"/></param>
        private void IterationEventHandler(ObjectChangedEvent iterationEvent)
        {
            if (iterationEvent.EventKind == EventKind.Added)
            {
                var iteration = (Iteration)iterationEvent.ChangedThing;
                var session = this.Sessions.SingleOrDefault(s => s.Assembler.Cache == iteration.Cache);

                if (session == null)
                {
                    throw new InvalidOperationException("There is no ISession associated with an Iteration.");
                }

                this.Iterations.Add(iteration);
            }
            else if (iterationEvent.EventKind == EventKind.Removed)
            {
                var iteration = (Iteration)iterationEvent.ChangedThing;
                this.Iterations.Remove(iteration);
            }
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
                this.Sessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.Sessions.Remove(sessionChange.Session);
                var iterationsToRemove = this.Iterations.Where(x => x.Cache == sessionChange.Session.Assembler.Cache);

                foreach (var iterationToRemove in iterationsToRemove)
                {
                    this.Iterations.Remove(iterationToRemove);
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="ExportCommand"/>
        /// </summary>
        private void ExecuteExportCommand()
        {
            var dialogViewModel = new HtmlExportRequirementsSpecificationSelectionDialogViewModel(this.Iterations, this.openSaveFileDialogService);
            var dialogResult = this.dialogNavigationService.NavigateModal(dialogViewModel);
        }
    }
}
