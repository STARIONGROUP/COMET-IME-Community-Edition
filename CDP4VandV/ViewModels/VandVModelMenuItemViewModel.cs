// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVModelMenuItemViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4VandV.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4VandV.Rdl;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using DevExpress.Xpf.Core;

    using ReactiveUI;

    /// <summary>
    /// Represents a single open <see cref="EngineeringModel"/> in the "Set up V&amp;V" ribbon drop-down. Executing its
    /// command checks the model's chained reference data and, if anything the V&amp;V capability needs is missing, seeds
    /// it into the model's <see cref="ModelReferenceDataLibrary"/> after a one-click confirmation.
    /// </summary>
    public class VandVModelMenuItemViewModel : ReactiveObject
    {
        /// <summary>
        /// The caption shown in the confirmation and information message boxes.
        /// </summary>
        private const string Caption = "Set up V&V";

        /// <summary>
        /// The <see cref="EngineeringModel"/> this menu item acts on.
        /// </summary>
        private readonly EngineeringModel model;

        /// <summary>
        /// The <see cref="ISession"/> the <see cref="model"/> belongs to.
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The <see cref="VandVRdlService"/> that checks and seeds the reference data.
        /// </summary>
        private readonly VandVRdlService rdlService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVModelMenuItemViewModel"/> class.
        /// </summary>
        /// <param name="model">The <see cref="EngineeringModel"/> this menu item acts on.</param>
        /// <param name="session">The <see cref="ISession"/> the <paramref name="model"/> belongs to.</param>
        /// <param name="rdlService">The <see cref="VandVRdlService"/> that checks and seeds.</param>
        public VandVModelMenuItemViewModel(EngineeringModel model, ISession session, VandVRdlService rdlService)
        {
            this.model = model;
            this.session = session;
            this.rdlService = rdlService;

            this.SetUpVnVCommand = ReactiveCommandCreator.CreateAsyncTask(this.ExecuteSetUpVnV);
        }

        /// <summary>
        /// Gets the <see cref="EngineeringModel"/> this menu item represents.
        /// </summary>
        public EngineeringModel Model => this.model;

        /// <summary>
        /// Gets the label shown in the ribbon drop-down.
        /// </summary>
        public string MenuItemContent => this.model.EngineeringModelSetup?.Name ?? this.model.EngineeringModelSetup?.ShortName ?? "Model";

        /// <summary>
        /// Gets the secondary description shown under the label in the ribbon drop-down.
        /// </summary>
        public string Description => "Check and create the V&V reference data for this model";

        /// <summary>
        /// Gets the command that runs the check-and-seed flow.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SetUpVnVCommand { get; }

        /// <summary>
        /// Checks the model's chained reference data, and seeds the missing V&amp;V manifest items after confirmation.
        /// </summary>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteSetUpVnV()
        {
            var mrdl = this.model.EngineeringModelSetup.RequiredRdl.FirstOrDefault();

            if (mrdl == null)
            {
                return;
            }

            var check = this.rdlService.Check(mrdl);

            if (!check.HasMissingItems)
            {
                DXMessageBox.Show("The V&V reference data is already present in this model. Nothing to create.", Caption, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!this.rdlService.CanSeed(this.session, mrdl))
            {
                DXMessageBox.Show(
                    "You do not have permission to write to this model's reference data library.\n\nAsk a model or site administrator to create the following in the model or a site reference data library:\n\n" + check.BuildSummary(),
                    Caption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = DXMessageBox.Show(
                string.Format("The following reference data will be created in '{0}':\n\n{1}\n\nProceed?", mrdl.ShortName, check.BuildSummary()),
                Caption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await this.rdlService.Seed(this.session, mrdl, check);
                DXMessageBox.Show("The V&V reference data has been created. You can now create V&V items from the VCD browser.", Caption, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show("The V&V reference data could not be created:\n\n" + ex.Message, Caption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
