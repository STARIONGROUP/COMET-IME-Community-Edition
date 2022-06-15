// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelClosingDialogViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The ViewModel for the <see cref="ModelSelection"/> Dialog
    /// </summary>
    public class ModelClosingDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelClosingDialogViewModel"/> class. 
        /// </summary>
        /// <param name="sessionAvailable">
        /// The session Available.
        /// </param>
        public ModelClosingDialogViewModel(IEnumerable<ISession> sessionAvailable)
        {
            this.SessionsAvailable = new DisposableReactiveList<ModelSelectionSessionRowViewModel>();
            this.InitializeReactiveCommands();

            this.PopulateSessionsRowViewModel(sessionAvailable);
        }

        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle => "Iteration Selection";

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
            this.SelectedIterations = new ReactiveList<IViewModelBase<Thing>>();
            this.IsBusy = false;
            var canOk = this.WhenAnyValue(x => x.SelectedIterations.Count, count => count != 0);
            this.CloseCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteClose, canOk, RxApp.MainThreadScheduler);

            this.CloseCommand.ThrownExceptions.Select(ex => ex).Subscribe(
                x =>
                {
                    this.ErrorMessage = x.Message;
                    this.IsBusy = false;
                });

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);
            this.CancelCommand.Subscribe();

            this.SelectedIterations.ItemsAdded.Subscribe(this.FilterIterationSelectionItems);
        }

        /// <summary>
        /// Executes the Close command
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task ExecuteClose()
        {
            this.IsBusy = true;
            this.LoadingMessage = "Closing...";

            foreach (var iteration in this.SelectedIterations)
            {
                var modelrow = (ModelSelectionIterationSetupRowViewModel)iteration;
                var session = modelrow.Session;

                Lazy<Thing> cachedIteration;

                if (session.Assembler.Cache.TryGetValue(
                        new CacheKey(iteration.Thing.Iid, null),
                        out cachedIteration))
                {
                    var modelSetup = (EngineeringModelSetup)modelrow.Thing.Container;
                    await session.CloseIterationSetup((IterationSetup)iteration.Thing);

                    if (session.OpenIterations.Keys.Count(it => it.IterationSetup.Container == modelSetup) == 1)
                    {
                        var modelRdl = modelSetup.RequiredRdl.Single();
                        await session.CloseModelRdl(modelRdl);
                    }
                }
            }

            // In order to give the user the idea something is happening we delay here such that
            // the close window stays visible for 2 seconds after the iteration has been removed.
            await Task.Delay(2000);

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
            foreach (var session in sessions.Where(session => session.Assembler.Cache.Select(x => x.Value).Any(item => item.Value.ClassKind == ClassKind.Iteration)))
            {
                this.SessionsAvailable.Add(new ModelSelectionSessionRowViewModel(session.RetrieveSiteDirectory(), session));
            }

            foreach (var availableSession in this.SessionsAvailable)
            {
                var openIterationIids = availableSession.Session.OpenIterations.Keys.Select(x => x.Iid).ToList();

                // remove iteration rows
                foreach (var model in availableSession.EngineeringModelSetupRowViewModels)
                {
                    var rowToRemove =
                        model.IterationSetupRowViewModels.Where(x => !openIterationIids.Contains(x.IterationIid)).ToList();

                    foreach (var modelSelectionIterationSetupRowViewModel in rowToRemove)
                    {
                        model.IterationSetupRowViewModels.RemoveAndDispose(modelSelectionIterationSetupRowViewModel);
                    }
                }

                // remove model rows that don't have any open iteration
                availableSession.EngineeringModelSetupRowViewModels.RemoveAllAndDispose(availableSession.EngineeringModelSetupRowViewModels.ToList().Where(x => !x.IterationSetupRowViewModels.Any()));
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
                // Cant remove directly from the reactiveList: KeyNotFoundException
                var list = this.SelectedIterations.ToList();
                list.Remove(row);

                this.SelectedIterations.Clear();
                this.SelectedIterations.AddRange(list);
            }
        }
    }
}
