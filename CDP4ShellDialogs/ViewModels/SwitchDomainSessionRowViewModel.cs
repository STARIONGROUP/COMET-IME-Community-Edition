// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwitchDomainSessionRowViewModel.cs" company="Starion Group S.A.">
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
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="SwitchDomainSessionRowViewModel" /> is to represent a <see cref="Session" /> and its
    /// <see cref="SiteDirectory" />
    /// </summary>
    public class SwitchDomainSessionRowViewModel : SiteDirectoryRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchDomainSessionRowViewModel" /> class.
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory" /> that is represented by the row-view-model
        /// </param>
        /// <param name="session">
        /// The <see cref="CDP4Dal.Session" /> that is represented by the row-view-model
        /// </param>
        public SwitchDomainSessionRowViewModel(SiteDirectory siteDirectory, ISession session)
            : base(siteDirectory, session, null)
        {
            this.EngineeringModelSetupRowViewModels = new DisposableReactiveList<SwitchDomainEngineeringModelSetupRowViewModel>();

            var models = this.Session.OpenIterations.Select(i => i.Key).Select(o => o.Container).OfType<EngineeringModel>()
                .Select(em => em.EngineeringModelSetup).Distinct();

            foreach (var model in models.OrderBy(m => m.Name))
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
        /// Gets the <see cref="SwitchDomainEngineeringModelSetupRowViewModel" /> that are contained by the row-view-model
        /// </summary>
        public DisposableReactiveList<SwitchDomainEngineeringModelSetupRowViewModel> EngineeringModelSetupRowViewModels { get; private set; }

        /// <summary>
        /// Add a <see cref="SwitchDomainEngineeringModelSetupRowViewModel" /> to the list of <see cref="EngineeringModelSetup" />s
        /// </summary>
        /// <param name="engineeringModelSetup">The <see cref="EngineeringModelSetup" /> that is to be added</param>
        private void AddModelRowViewModel(EngineeringModelSetup engineeringModelSetup)
        {
            var row = new SwitchDomainEngineeringModelSetupRowViewModel(engineeringModelSetup, this.Session);
            this.EngineeringModelSetupRowViewModels.Add(row);
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.Name += string.Format(" ({0})", this.Session.Name);
        }
    }
}
