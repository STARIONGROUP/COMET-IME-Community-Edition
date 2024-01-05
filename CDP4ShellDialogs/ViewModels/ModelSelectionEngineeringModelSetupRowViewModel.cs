// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionEngineeringModelSetupRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
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
    using System.Linq;
    using System.Windows;

    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Services;

    using CDP4Dal;

    using CommonServiceLocator;

    using NLog;

    using Splat;

    using LogLevel = NLog.LogLevel;

    /// <summary>
    /// The Row-view-model representing a <see cref="EngineeringModelSetup"/>
    /// </summary>
    public class ModelSelectionEngineeringModelSetupRowViewModel : EngineeringModelSetupRowViewModel
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The INJECTED <see cref="IMessageBoxService"/>;
        /// </summary>
        private readonly IMessageBoxService messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSelectionEngineeringModelSetupRowViewModel"/> class
        /// </summary>
        /// <param name="engineeringModelSetup">
        /// The engineering Model Setup.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public ModelSelectionEngineeringModelSetupRowViewModel(EngineeringModelSetup engineeringModelSetup, ISession session)
            : base(engineeringModelSetup, session, null)
        {
            this.IterationSetupRowViewModels = new DisposableReactiveList<ModelSelectionIterationSetupRowViewModel>();
            this.InitializeContainers();
        }

        /// <summary>
        /// Gets the <see cref="IterationSetupRowViewModel"/> that are contained by the row-view-model
        /// </summary>
        public DisposableReactiveList<ModelSelectionIterationSetupRowViewModel> IterationSetupRowViewModels { get; private set; }

        /// <summary>
        /// Initializes containers
        /// </summary>
        private void InitializeContainers()
        {
            foreach (var iteration in this.Thing.IterationSetup.Where(x => !x.IsDeleted))
            {
                this.AddIteration(iteration);
            }
        }

        /// <summary>
        /// Add the <see cref="IterationSetup"/> to the contained <see cref="IterationSetupRowViewModels"/>
        /// </summary>
        /// <param name="iteration">
        /// the <see cref="IterationSetup"/> object that are to be added
        /// </param>
        private void AddIteration(IterationSetup iteration)
        {
            var participants = this.Thing.Participant.Where(x => x.Person == this.Session.ActivePerson).ToList();

            if (participants.Count > 1)
            {
                var message = $"User '{participants.First().Person.ShortName}' is found in multiple {nameof(Participant)}s in {nameof(EngineeringModelSetup)} '{this.Thing.Name}'.";

                logger.Log(LogLevel.Error, message);
                this.messageBoxService.Show(message, $"Multiple {nameof(Participant)}s found", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            var activeParticipant = participants.Single();

            var row = new ModelSelectionIterationSetupRowViewModel(iteration, activeParticipant, this.Session);
            this.IterationSetupRowViewModels.Add(row);
        }
    }
}
