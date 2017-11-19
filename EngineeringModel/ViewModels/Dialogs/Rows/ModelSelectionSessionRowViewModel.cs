// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionSessionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Linq;

    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="SessionRowViewModel"/> is to represent a <see cref="Session"/> and its <see cref="SiteDirectory"/>
    /// </summary>
    public class ModelSelectionSessionRowViewModel : SiteDirectoryRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSelectionSessionRowViewModel"/> class. 
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory"/> that is represented by the row-view-model
        /// </param>
        /// <param name="session">
        /// The <see cref="CDP4Dal.Session"/> that is represented by the row-view-model
        /// </param>
        public ModelSelectionSessionRowViewModel(SiteDirectory siteDirectory, ISession session)
            : base(siteDirectory, session, null)
        {
            this.EngineeringModelSetupRowViewModels = new ReactiveList<ModelSelectionEngineeringModelSetupRowViewModel>();
            foreach (var model in this.Thing.Model.OrderBy(m => m.Name))
            {
                var isParticipant = model.Participant.Any(x => x.Person == this.Session.ActivePerson);
                if (isParticipant)
                {
                    this.AddModelRowViewModel(model);
                }
            }

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the <see cref="EngineeringModelSetupRowViewModel"/> that are contained by the row-view-model
        /// </summary>
        public ReactiveList<ModelSelectionEngineeringModelSetupRowViewModel> EngineeringModelSetupRowViewModels { get; private set; }
        
        /// <summary>
        /// Add a <see cref="EngineeringModelSetupRowViewModel"/> to the list of <see cref="EngineeringModelSetup"/>s
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModelSetup"/> that is to be added</param>
        private void AddModelRowViewModel(EngineeringModelSetup model)
        {
            var row = new ModelSelectionEngineeringModelSetupRowViewModel(model, this.Session);
            this.EngineeringModelSetupRowViewModels.Add(row);
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.Name += string.Format(" ({0})", this.IDalUri);
        }
    }
}
