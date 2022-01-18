// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelOpeningDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski.
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

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The ViewModel for the <see cref="DialogViewModelBase"/> Dialog
    /// </summary>
    public class ModelOpeningDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for the <see cref="IsModelScreen"/> property.
        /// </summary>
        private bool isModelScreen = true;

        /// <summary>
        /// Backing field for the <see cref="IsIterationScreen"/> property.
        /// </summary>
        private bool isIterationScreen;

        /// <summary>
        /// Backing field for the <see cref="Title"/> property.
        /// </summary>
        private string title = "Engineering Models";

        /// <summary>
        /// Backing field for the <see cref="SelectedRowSession"/> property.
        /// </summary>
        private ModelSelectionSessionRowViewModel selectedRowSession;

        /// <summary>
        /// Backing field for the <see cref="SelectedEngineeringModelSetup "/> property.
        /// </summary>
        private ModelSelectionEngineeringModelSetupRowViewModel selectedEngineeringModelSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelOpeningDialogViewModel"/> class. 
        /// </summary>
        /// <param name="sessionAvailable">
        /// The session Available.
        /// </param>
        public ModelOpeningDialogViewModel(IEnumerable<ISession> sessionAvailable)
        {
            this.SessionsAvailable = new DisposableReactiveList<ModelSelectionSessionRowViewModel>();
            this.SelectedIterations = new ReactiveList<IViewModelBase<Thing>>();
            this.SelectedIterations.ChangeTrackingEnabled = true;
            this.IsBusy = false;
            var canOk = this.WhenAnyValue(x => x.SelectedIterations.Count, count => count != 0);

            this.SelectCommand = ReactiveCommand.CreateAsyncTask(canOk, x => this.ExecuteSelect(), RxApp.MainThreadScheduler);

            this.SelectCommand.ThrownExceptions.Select(ex => ex).Subscribe(
                x =>
                {
                    this.ErrorMessage = x.Message;
                    this.IsBusy = false;
                });

            var canActiveOk = this.WhenAnyValue(x => x.SelectedEngineeringModelSetup.IterationSetupRowViewModels.Count, count => count != 0);
            this.SelectActiveIterationCommand = ReactiveCommand.CreateAsyncTask(canActiveOk, x => this.ExecuteSelectActiveIteration(), RxApp.MainThreadScheduler);

            this.SelectActiveIterationCommand.ThrownExceptions.Select(ex => ex).Subscribe(
                x =>
                {
                    this.ErrorMessage = x.Message;
                    this.IsBusy = false;
                });

            this.SelectIterationCommand = ReactiveCommand.Create();
            this.SelectIterationCommand.Subscribe(_ => this.ExecuteNext());

            this.BackCommand = ReactiveCommand.Create();
            this.BackCommand.Subscribe(_ => this.ExecuteBack());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.SelectedIterations.ChangeTrackingEnabled = true;
            this.SelectedIterations.ItemsAdded.Subscribe(this.FilterIterationSelectionItems);

            this.WhenAnyValue(x => x.IsModelScreen).Subscribe(x => this.IsIterationScreen = !x);

            this.PopulateSessionsRowViewModel(sessionAvailable);
        }

        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle => "Iteration Selection";

        /// <summary>
        /// Gets the title of the group
        /// </summary>
        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        /// <summary>
        /// Gets a value indicating whether the Model Screen is selected
        /// </summary>
        public bool IsModelScreen
        {
            get => this.isModelScreen;
            set => this.RaiseAndSetIfChanged(ref this.isModelScreen, value);
        }

        /// <summary>
        /// Gets a value indicating whether the Iteration Screen is selected
        /// </summary>
        public bool IsIterationScreen
        {
            get => this.isIterationScreen;
            set => this.RaiseAndSetIfChanged(ref this.isIterationScreen, value);
        }

        /// <summary>
        /// Gets the list of <see cref="ModelSelectionSessionRowViewModel"/> available
        /// </summary>
        public DisposableReactiveList<ModelSelectionSessionRowViewModel> SessionsAvailable { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="IterationSetup"/> selected
        /// </summary>
        public ReactiveList<IViewModelBase<Thing>> SelectedIterations { get; set; }

        /// <summary>
        /// Gets the SelectCommand <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit> SelectCommand { get; private set; }

        /// <summary>
        /// Gets the Select Active Iteration <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit> SelectActiveIterationCommand { get; private set; }

        /// <summary>
        /// Gets the CancelCommand <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the SelectIterationCommand <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> SelectIterationCommand { get; private set; }

        /// <summary>
        /// Gets the BackCommand <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> BackCommand { get; private set; }

        /// <summary>
        /// Gets or sets the selected row session.
        /// </summary>
        public ModelSelectionSessionRowViewModel SelectedRowSession
        {
            get => this.selectedRowSession;
            set => this.RaiseAndSetIfChanged(ref this.selectedRowSession, value);
        }

        /// <summary>
        /// Gets or sets the selected row model.
        /// </summary>
        public ModelSelectionEngineeringModelSetupRowViewModel SelectedEngineeringModelSetup
        {
            get => this.selectedEngineeringModelSetup;
            set => this.RaiseAndSetIfChanged(ref this.selectedEngineeringModelSetup, value);
        }

        /// <summary>
        /// Executes the Select command
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteSelect()
        {
            var tasks = new List<Task>();
            var stopWatch = Stopwatch.StartNew();

            // this.SelectedITeration should only contain ModelSelectionIterationSetupRowViewModel as the items are filtered
            foreach (var setupRow in this.SelectedIterations)
            {
                var iterationSetupRow = setupRow as ModelSelectionIterationSetupRowViewModel;

                if (iterationSetupRow == null)
                {
                    throw new NullReferenceException("The selected row was not of the correct type.");
                }

                if (!(iterationSetupRow.Thing.Container is EngineeringModelSetup modelSetup))
                {
                    throw new NullReferenceException($"The EngineeringModelSetup that iteration {iterationSetupRow.Thing.Iid} belongs to cannot be found.");
                }

                // Retrieve the Iteration from the IDal
                var session = iterationSetupRow.Session;

                var model = new EngineeringModel(modelSetup.EngineeringModelIid, session.Assembler.Cache, session.Credentials.Uri)
                    { EngineeringModelSetup = modelSetup };

                var iteration = new Iteration(iterationSetupRow.Thing.IterationIid, session.Assembler.Cache, session.Credentials.Uri);

                model.Iteration.Add(iteration);
                tasks.Add(session.Read(iteration, iterationSetupRow.SelectedDomain));
            }

            this.IsBusy = true;
            this.LoadingMessage = "Loading Iterations...";
            await Task.WhenAll(tasks);
            this.IsBusy = false;
            stopWatch.Stop();
            logger.Info("Loading model took {0}", stopWatch.Elapsed);

            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Executes the SelectActiveIteration command
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteSelectActiveIteration()
        {
            var stopWatch = Stopwatch.StartNew();
            var activeIterationSetupRow = this.SelectedEngineeringModelSetup.IterationSetupRowViewModels.FirstOrDefault(x => x.Thing.FrozenOn == null);

            this.IsBusy = true;
            this.LoadingMessage = "Loading Iteration...";

            if (activeIterationSetupRow != null)
            {
                if (!(activeIterationSetupRow.Thing.Container is EngineeringModelSetup modelSetup))
                {
                    throw new NullReferenceException($"The EngineeringModelSetup that iteration {activeIterationSetupRow.Thing.Iid} belongs to cannot be found.");
                }

                // Retrieve the Iteration from the IDal
                var session = activeIterationSetupRow.Session;

                var model = new EngineeringModel(modelSetup.EngineeringModelIid, session.Assembler.Cache, session.Credentials.Uri)
                    { EngineeringModelSetup = modelSetup };

                var iteration = new Iteration(activeIterationSetupRow.Thing.IterationIid, session.Assembler.Cache, session.Credentials.Uri);

                model.Iteration.Add(iteration);

                DomainOfExpertise initialDomain;

                if (session.ActivePerson.DefaultDomain != null && activeIterationSetupRow.DomainOfExpertises.Contains(session.ActivePerson.DefaultDomain))
                {
                    initialDomain = session.ActivePerson.DefaultDomain;
                }
                else
                {
                    initialDomain = activeIterationSetupRow.DomainOfExpertises.FirstOrDefault();
                }

                await session.Read(iteration, initialDomain);
            }

            this.IsBusy = false;
            stopWatch.Stop();
            logger.Info("Loading model took {0}", stopWatch.Elapsed);

            this.DialogResult = new BaseDialogResult(true);
        }

        /// <summary>
        /// Executes the cancel command
        /// </summary>
        private void ExecuteCancel()
        {
            foreach (var session in this.SelectedIterations.Select(sr => sr as ModelSelectionIterationSetupRowViewModel).Select(i => i.Session))
            {
                session.Cancel();
            }

            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Executes the next command
        /// </summary>
        private void ExecuteNext()
        {
            this.Title = "Model Iterations";
            this.IsModelScreen = false;
        }

        /// <summary>
        /// Executes the back command
        /// </summary>
        private void ExecuteBack()
        {
            this.Title = "Engineering Models";
            this.IsModelScreen = true;
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
                this.SessionsAvailable.Add(new ModelSelectionSessionRowViewModel(session.RetrieveSiteDirectory(), session));
            }

            this.SelectedRowSession = this.SessionsAvailable.FirstOrDefault();

            var modelSelectionSessionRowViewModel = this.SelectedRowSession;

            if (modelSelectionSessionRowViewModel != null)
            {
                this.SelectedEngineeringModelSetup = modelSelectionSessionRowViewModel.EngineeringModelSetupRowViewModels.FirstOrDefault();
            }
        }

        /// <summary>
        /// Filters the <see cref="SelectedIterations"/> added items to only select <see cref="IterationSetupRowViewModel"/>
        /// </summary>
        /// <param name="row">the added row-view-model</param>
        private void FilterIterationSelectionItems(IViewModelBase<Thing> row)
        {
            var iterationRow = row as ModelSelectionIterationSetupRowViewModel;

            if (iterationRow == null)
            {
                this.RemoveSelectedRowFromSelectedIterationList(row);
                return;
            }

            if (iterationRow.Session.OpenIterations.Keys.Any(it => it.Iid == iterationRow.IterationIid))
            {
                this.RemoveSelectedRowFromSelectedIterationList(row);
            }
        }

        /// <summary>
        /// Remove the <paramref name="row"/> from the current <see cref="SelectedIterations"/> list
        /// </summary>
        /// <param name="row">The row to remove</param>
        private void RemoveSelectedRowFromSelectedIterationList(IViewModelBase<Thing> row)
        {
            // Cant remove directly from the reactiveList: KeyNotFoundException
            var list = this.SelectedIterations.ToList();
            list.Remove(row);

            this.SelectedIterations.Clear();
            this.SelectedIterations.AddRange(list);
        }
    }
}
