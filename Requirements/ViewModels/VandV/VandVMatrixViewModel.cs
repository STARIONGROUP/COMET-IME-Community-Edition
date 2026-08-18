// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVMatrixViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Data;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Requirements.Services;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The RVM / VCRM coverage matrix panel: requirements down, stage gates across, each cell showing the method and
    /// status of the V&amp;V activities planned at that gate. Its purpose is to make uncovered requirements obvious.
    /// </summary>
    /// <remarks>
    /// The columns are dynamic (a project defines its own stage gates in the RDL), so the grid is fed a
    /// <see cref="DataTable"/> and generates its columns from it, the same approach the Reference Data Mapper uses.
    /// </remarks>
    public class VandVMatrixViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// The caption of the panel.
        /// </summary>
        private const string PanelCaption = "V&V Coverage Matrix (RVM)";

        /// <summary>
        /// The name of the column holding the requirement short-name.
        /// </summary>
        private const string RequirementColumn = "Requirement";

        /// <summary>
        /// The name of the column holding the requirement name.
        /// </summary>
        private const string RequirementNameColumn = "Requirement Name";

        /// <summary>
        /// The name of the column summarising whether the requirement is covered at all.
        /// </summary>
        private const string CoveredColumn = "Covered";

        /// <summary>
        /// Backing field for <see cref="MatrixTable"/>.
        /// </summary>
        private DataTable matrixTable;

        /// <summary>
        /// Backing field for <see cref="Summary"/>.
        /// </summary>
        private string summary;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVMatrixViewModel"/> class.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> to build the matrix for.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/>.</param>
        /// <param name="pluginSettingsService">The <see cref="IPluginSettingsService"/>.</param>
        public VandVMatrixViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = $"{PanelCaption}, iteration_{this.Thing.IterationSetup.IterationNumber}";
            this.ToolTip = $"{((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name}\n{this.Thing.IDalUri}\n{this.Session.ActivePerson.Name}";

            this.RefreshCommand = ReactiveCommandCreator.Create(this.BuildMatrix);

            this.BuildMatrix();
            this.AddSubscriptions();
        }

        /// <summary>
        /// Gets the matrix as a <see cref="DataTable"/>; the grid generates its columns from it.
        /// </summary>
        public DataTable MatrixTable
        {
            get => this.matrixTable;
            private set => this.RaiseAndSetIfChanged(ref this.matrixTable, value);
        }

        /// <summary>
        /// Gets the one-line coverage summary shown above the matrix.
        /// </summary>
        public string Summary
        {
            get => this.summary;
            private set => this.RaiseAndSetIfChanged(ref this.summary, value);
        }

        /// <summary>
        /// Gets the command that rebuilds the matrix.
        /// </summary>
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        /// <summary>
        /// Gets or sets the name of the layout group the panel docks into.
        /// </summary>
        public string TargetName { get; set; } = LayoutGroupNames.DocumentContainer;

        /// <summary>
        /// Rebuilds the matrix from the current model state.
        /// </summary>
        private void BuildMatrix()
        {
            var model = VandVCoverageQuery.Build(this.Thing);

            var table = new DataTable();
            table.Columns.Add(RequirementColumn, typeof(string));
            table.Columns.Add(RequirementNameColumn, typeof(string));

            foreach (var stage in model.Stages)
            {
                if (!table.Columns.Contains(stage))
                {
                    table.Columns.Add(stage, typeof(string));
                }
            }

            // a project may define a stage gate literally named "Covered"; adding it twice throws and takes the
            // whole panel down, so the fixed column is guarded exactly like the stage columns are
            if (!table.Columns.Contains(CoveredColumn))
            {
                table.Columns.Add(CoveredColumn, typeof(string));
            }

            foreach (var coverage in model.Coverages)
            {
                var row = table.NewRow();
                row[RequirementColumn] = coverage.Requirement.ShortName ?? string.Empty;
                row[RequirementNameColumn] = coverage.Requirement.Name ?? string.Empty;

                foreach (var stage in model.Stages)
                {
                    row[stage] = coverage.CellText(stage);
                }

                row[CoveredColumn] = coverage.VandVItems.Count > 0 ? "Yes" : "NO";
                table.Rows.Add(row);
            }

            this.MatrixTable = table;

            this.Summary = model.Coverages.Count == 0
                ? "No requirements in this iteration."
                : $"{model.Coverages.Count - model.UncoveredCount} of {model.Coverages.Count} requirements covered, {model.UncoveredCount} uncovered.";
        }

        /// <summary>
        /// Rebuilds the matrix whenever a V&amp;V item, its attributes, or a traceability relationship changes.
        /// </summary>
        private void AddSubscriptions()
        {
            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(BinaryRelationship))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.BuildMatrix()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Requirement))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.BuildMatrix()));

            this.Disposables.Add(
                this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(SimpleParameterValue))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.BuildMatrix()));
        }
    }
}
