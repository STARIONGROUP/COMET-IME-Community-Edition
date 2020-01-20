// -------------------------------------------------------------------------------------------------
// <copyright file="ModelOpeningDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
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
    /// The ViewModel for the <see cref="ModelSelection"/> Dialog
    /// </summary>
    public class ModelOpeningDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
            this.SelectCommand.ThrownExceptions.Select(ex => ex).Subscribe(x =>
            {
                this.ErrorMessage = x.Message;
                this.IsBusy = false;
            });

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.SelectedIterations.ChangeTrackingEnabled = true;
            this.SelectedIterations.ItemsAdded.Subscribe(this.FilterIterationSelectionItems);

            this.PopulateSessionsRowViewModel(sessionAvailable);
        }

        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle
        {
            get { return "Iteration Selection"; }
        }

        /// <summary>
        /// Gets the list of <see cref="BaseRowViewModel"/> available
        /// </summary>
        public DisposableReactiveList<ModelSelectionSessionRowViewModel> SessionsAvailable { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="IterationSetup"/> selected
        /// </summary>
        public ReactiveList<IViewModelBase<Thing>> SelectedIterations { get; private set; }

        /// <summary>
        /// Gets the Select <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit> SelectCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

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

                var modelSetup = iterationSetupRow.Thing.Container as EngineeringModelSetup;

                if (modelSetup == null)
                {
                    throw new NullReferenceException(string.Format("The EngineeringModelSetup that iteration {0} belongs to cannot be found.", iterationSetupRow.Thing.Iid));
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