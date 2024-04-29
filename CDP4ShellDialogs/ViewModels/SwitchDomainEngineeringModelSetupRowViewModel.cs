// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwitchDomainEngineeringModelSetupRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Omar Elebiary
// 
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;

    using System.Linq;

    /// <summary>
    /// The Row-view-model representing a <see cref="EngineeringModelSetup" />
    /// </summary>
    public class SwitchDomainEngineeringModelSetupRowViewModel : EngineeringModelSetupRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchDomainEngineeringModelSetupRowViewModel" /> class
        /// </summary>
        /// <param name="engineeringModelSetup">
        /// The engineering Model Setup.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public SwitchDomainEngineeringModelSetupRowViewModel(EngineeringModelSetup engineeringModelSetup, ISession session)
            : base(engineeringModelSetup, session, null)
        {
            this.IterationSetupRowViewModels = new DisposableReactiveList<SwitchDomainIterationSetupRowViewModel>();
            this.InitializeContainedRows();
        }

        /// <summary>
        /// Gets the <see cref="SwitchDomainIterationSetupRowViewModel" /> that are contained by the row-view-model
        /// </summary>
        public DisposableReactiveList<SwitchDomainIterationSetupRowViewModel> IterationSetupRowViewModels { get; private set; }

        /// <summary>
        /// Initializes contained rows
        /// </summary>
        private void InitializeContainedRows()
        {
            var iterationSetups = this.Session.OpenIterations.Keys.Select(i => i.IterationSetup).Where(i => i.Container.Iid.Equals(this.Thing.Iid));

            foreach (var iterationSetup in iterationSetups)
            {
                this.AddIteration(iterationSetup);
            }
        }

        /// <summary>
        /// Add the <see cref="IterationSetup" /> to the contained <see cref="SwitchDomainIterationSetupRowViewModel" />
        /// </summary>
        /// <param name="iterationSetup">
        /// the <see cref="IterationSetup" /> object that are to be added
        /// </param>
        private void AddIteration(IterationSetup iterationSetup)
        {
            var activeParticipant = this.Thing.Participant.Single(x => x.Person == this.Session.ActivePerson);

            var row = new SwitchDomainIterationSetupRowViewModel(iterationSetup, activeParticipant, this.Session);
            this.IterationSetupRowViewModels.Add(row);
        }
    }
}
