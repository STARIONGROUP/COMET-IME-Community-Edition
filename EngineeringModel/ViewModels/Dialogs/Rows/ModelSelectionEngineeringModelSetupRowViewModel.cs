// -------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionEngineeringModelSetupRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Linq;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;

    /// <summary>
    /// The Row-view-model representing a <see cref="EngineeringModelSetup"/>
    /// </summary>
    public class ModelSelectionEngineeringModelSetupRowViewModel : CDP4CommonView.EngineeringModelSetupRowViewModel
    {
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
            var activeParticipant = this.Thing.Participant.Single(x => x.Person == this.Session.ActivePerson);

            var row = new ModelSelectionIterationSetupRowViewModel(iteration, activeParticipant, this.Session);
            this.IterationSetupRowViewModels.Add(row);
        }
    }
}