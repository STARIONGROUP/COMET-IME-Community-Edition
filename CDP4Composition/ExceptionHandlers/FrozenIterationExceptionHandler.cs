// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrozenIterationExceptionHandler.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ExceptionHandlers
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Threading;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.ExceptionHandlerService;

    using CDP4Composition.Services;
    using CDP4Composition.Views;

    using CDP4Dal;
    using CDP4Dal.Exceptions;
    using CDP4Dal.Operations;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// The purpose of the <see cref="FrozenIterationExceptionHandler" /> is to check if the Exception contains data that indicate an error that the Iteration is Frozen and therefore cannot be editted.
    /// The user can choose to automatically close the Frozen Iteration and open the model's currently active Iteration.
    /// </summary>
    [Export(typeof(IExceptionHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FrozenIterationExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// The injected <see cref="IMessageBoxService"/>
        /// </summary>
        private IMessageBoxService messageboxService;

        /// <summary>
        /// Creates a new instance of the <see cref="FrozenIterationExceptionHandler"/> class
        /// </summary>
        /// <param name="messageboxService">The INJECTED <see cref="IMessageBoxService"/></param>
        [ImportingConstructor]
        public FrozenIterationExceptionHandler(IMessageBoxService messageboxService)
        {
            this.messageboxService = messageboxService;
        }

        /// <summary>
        /// Handles a specific <see cref="Exception"/> and enables the IME to start a process based on the ocntent or type of the <see cref="Exception"/>
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/></param>
        /// <param name="payload">A collection of objects that can be used for exception handling</param>
        /// <returns>a boolean value indicating if the <see cref="Exception"/> was handled or not, so it could be thrown again</returns>
        public bool HandleException(Exception exception, params object[] payload) 
        {
            if (exception is not DalWriteException dalException)
            {
                return false;
            }

            if (dalException.CDPErrorTag == "#FROZEN_ITERATION")
            {
                this.messageboxService.Show("It is not allowed to write data to a frozen IterationSetup.", "Frozen Iteration", MessageBoxButton.OK, MessageBoxImage.Error);

                if (this.messageboxService.Show("Do you want to try to close the current Iteration and open the Active Iteration?", "Open Active Iteration", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.DispatchAction(() => this.StartCloseCurrentAndReopenActiveIteration(payload));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Start an <see cref="Action"/> on the main/UI thread
        /// </summary>
        /// <param name="action">The <see cref="Action"/></param>
        private void DispatchAction(Action action)
        {
            if (Application.Current?.MainWindow == null)
            {
                Dispatcher.CurrentDispatcher.Invoke(
                    action,
                    DispatcherPriority.Background);
            }
            else
            {
                Application.Current.Dispatcher.InvokeAsync(
                    action,
                    DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Start the process of closing the current frozen Iteration and open the model's current
        /// </summary>
        /// <param name="payload">The payload that was added to the generic call to the <see cref="ExceptionHandlerService"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task StartCloseCurrentAndReopenActiveIteration(object[] payload)
        {
            DXSplashScreen.Show<LoadingView>();
            this.DispatchAction(() => DXSplashScreen.SetState("Reopening Iteration..."));

            var newWasOpened = false;

            try
            {
                var session = payload.OfType<ISession>().FirstOrDefault();
                var operationContainer = payload.OfType<OperationContainer>().FirstOrDefault();
                
                if (session != null && operationContainer != null)
                {
                    if (this.TryGetIterationIidFromContext(operationContainer, out var iterationIid))
                    {
                        if (session.Assembler.Cache.Values.Select(x => x.Value).SingleOrDefault(x => x.Iid == iterationIid) is Iteration iteration)
                        {
                            var engineeringModel = iteration.TopContainer as EngineeringModel;
                            var engineeringModelSetup = engineeringModel?.EngineeringModelSetup;

                            this.DispatchAction(() => DXSplashScreen.SetState("Refreshing Session..."));

                            await session.Refresh();

                            var openIterations = session.OpenIterations.Where(x => x.Key == iteration).ToList();

                            foreach (var kvp in openIterations)
                            {
                                var iterationSetup = kvp.Key.IterationSetup;

                                var domain = kvp.Value.Item1;
                                this.DispatchAction(() => DXSplashScreen.SetState($"Closing Iteration {iterationSetup.IterationNumber}..."));

                                await session.CloseIterationSetup(iterationSetup);

                                var newActiveIterationSetup = engineeringModelSetup?.IterationSetup.SingleOrDefault(x => x.FrozenOn == null);

                                if (newActiveIterationSetup != null)
                                {
                                    var model = new EngineeringModel(engineeringModelSetup.EngineeringModelIid, session.Assembler.Cache, session.Credentials.Uri)
                                        { EngineeringModelSetup = engineeringModelSetup };

                                    var newIteration = new Iteration(newActiveIterationSetup.IterationIid, session.Assembler.Cache, session.Credentials.Uri);

                                    model.Iteration.Add(newIteration);
                                    this.DispatchAction(() => DXSplashScreen.SetState($"Reading Iteration {newActiveIterationSetup.IterationNumber}..."));

                                    await session.Read(newIteration, domain);
                                }
                            }

                            newWasOpened = true;

                            DXSplashScreen.Close();

                            this.messageboxService.Show($"{openIterations.Count} active Iterations opened successfully.", "Reopen Iterations", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            finally
            {
                if (!newWasOpened)
                {
                    DXSplashScreen.Close();
                    this.messageboxService.Show("Iteration could not be detected. Please close the Frozen Iteration and open the Active Iteration manually.", "Reopen Iteration", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        /// <summary>
        /// Tries to get the Iteration's Iid from an <see cref="OperationContainer"/>
        /// </summary>
        /// <param name="operationContainer">The <see cref="OperationContainer"/></param>
        /// <param name="iterationIid">The Iid of the Iteration</param>
        /// <returns>a boolean value indicating if the Iteration's Iid was found using the OperationContainer</returns>
        private bool TryGetIterationIidFromContext(OperationContainer operationContainer, out Guid iterationIid)
        {
            iterationIid = Guid.Empty;

            //Operation Container Context example: "/EngineeringModel/fc6f8c69-b7b4-4e30-9ef6-c792d21d7264/iteration/6739f4c8-1bb2-4234-8891-55c277e8e140"
            var contextArray = operationContainer.Context.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

            if (contextArray.Length > 3 && contextArray[2] == "iteration")
            {
                iterationIid = Guid.Parse(contextArray[3]);
                return true;
            }

            return false;
        }
    }
}
