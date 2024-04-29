// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelSelectionIterationSetupRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System.Globalization;
    using System.Linq;

    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    
    using ReactiveUI;

    /// <summary>
    /// the view model for <see cref="IterationSetup"/> displayed in the Tree
    /// </summary>
    public class ModelSelectionIterationSetupRowViewModel : IterationSetupRowViewModel
    {
        /// <summary>
        /// backing field for <see cref="SelectedDomain"/>
        /// </summary>
        private DomainOfExpertise selectedDomain;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSelectionIterationSetupRowViewModel"/> class. 
        /// </summary>
        /// <param name="iterationSetup">
        /// The <see cref="IterationSetup"/> this is associated to
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> tied to this model.
        /// </param>
        /// <param name="domains">
        /// The <see cref="DomainOfExpertise"/> list of allowed domains.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public ModelSelectionIterationSetupRowViewModel(IterationSetup iterationSetup, Participant participant, ISession session)
            : base(iterationSetup, session, null)
        {
            this.ActiveParticipant = participant;

            this.DomainOfExpertises = new ReactiveList<DomainOfExpertise>();

            if (participant.Domain.Count != 0)
            {
                this.DomainOfExpertises.AddRange(participant.Domain.OrderBy(x => x.ShortName)); 

                this.SelectedDomain = this.DomainOfExpertises.Contains(this.ActiveParticipant.Person.DefaultDomain)
                    ? this.ActiveParticipant.Person.DefaultDomain
                    : this.DomainOfExpertises.First();
            }

            this.Name = "iteration_" + this.Thing.IterationNumber.ToString(CultureInfo.InvariantCulture);

            this.FrozenOnDate = !this.Thing.FrozenOn.HasValue ? "Active" : this.Thing.FrozenOn.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the date at which the iteration was Frozen
        /// </summary>
        public string FrozenOnDate { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="DomainOfExpertise"/>
        /// </summary>
        public DomainOfExpertise SelectedDomain
        {
            get { return this.selectedDomain; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDomain, value); }
        }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/> of the logged-in Person in the current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public ReactiveList<DomainOfExpertise> DomainOfExpertises { get; private set; }

        /// <summary>
        /// Gets the <see cref="Participant"/> from the Logged-in <see cref="Person"/>
        /// </summary>
        public Participant ActiveParticipant { get; private set; }
    }
}