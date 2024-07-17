// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
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

namespace CDP4CrossViewEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reactive;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using CDP4OfficeInfrastructure.OfficeDal;

    using NetOffice.ExcelApi;

    using ReactiveUI;

    /// <summary>
    /// The view-model to managing cross view workbook output.
    /// </summary>
    public class CrossViewDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedWorkbook"/> property.
        /// </summary>
        private WorkbookRowViewModel selectedWorkbook;

        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle { get; private set; }

        /// <summary>
        /// Gets the <see cref="IterationSetup"/>
        /// </summary>
        private Iteration Iteration { get; set; }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        private ISession Session { get; set; }

        /// <summary>
        /// Gets the Select <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// ViewModel that corresponds to the element selector area
        /// </summary>
        public ElementDefinitionSelectorViewModel ElementSelectorViewModel { get; private set; }

        /// <summary>
        /// ViewModel that corresponds to the element selector area
        /// </summary>
        public ParameterTypeSelectorViewModel ParameterSelectorViewModel { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="WorkbookRowViewModel"/>s that is available in the current Excel application.
        /// </summary>
        private List<WorkbookRowViewModel> Workbooks { get; set; }

        /// <summary>
        /// Backing field for <see cref="PersistValues"/>
        /// </summary>
        private bool persistValues;

        /// <summary>
        /// Gets or sets the selected <see cref="WorkbookRowViewModel"/>
        /// </summary>
        private WorkbookRowViewModel SelectedWorkbook
        {
            get => this.selectedWorkbook;
            set => this.RaiseAndSetIfChanged(ref this.selectedWorkbook, value);
        }

        /// <summary>
        /// Dictionary that contains manuallys aved values
        /// </summary>
        private Dictionary<string, string> ManuallyFilledValues = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets a value indicating metadata sheet persistence
        /// </summary>
        public bool PersistValues
        {
            get => this.persistValues;
            set => this.RaiseAndSetIfChanged(ref this.persistValues, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossViewDialogViewModel"/> class.
        /// </summary>
        /// <param name="application">
        /// The Excel <see cref="Application"/> that contains workbooks <see cref="Workbook"/>
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is currently opened
        /// </param>
        /// <param name="session">
        /// Current user session <see cref="ISession"/>
        /// </param>
        /// <param name="activeWorkbook">
        /// The Excel active workbook <see cref="Workbook"/> that might contain Crossview worksheet
        /// </param>
        public CrossViewDialogViewModel(Application application, Iteration iteration, ISession session, Workbook activeWorkbook)
        {
            this.Workbooks = new List<WorkbookRowViewModel>();
            this.Iteration = iteration;
            this.Session = session;
            this.PersistValues = true;

            this.InitWorkbooks(application, activeWorkbook);
            this.InitModels();

            this.DialogTitle = $"Select {this.ElementSelectorViewModel.ThingClassKind}s and {this.ParameterSelectorViewModel.ThingClassKind}s";

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);

            this.OkCommand = ReactiveCommandCreator.Create(this.ExecuteOk);
        }

        /// <summary>
        /// Populate the workbook row view-models that represent workbooks that are open in the
        /// excel application.
        /// </summary>
        /// <param name="application">
        /// The Excel <see cref="Application"/> that contains workbooks
        /// </param>
        /// <param name="activeWorkbook">
        /// The Excel active workbook <see cref="Workbook"/> that might contain Crossview worksheet
        /// </param>
        [ExcludeFromCodeCoverage]
        private void InitWorkbooks(Application application, Workbook activeWorkbook)
        {
            if (application == null)
            {
                return;
            }

            foreach (var workbook in application.Workbooks)
            {
                var row = new WorkbookRowViewModel(workbook);
                this.Workbooks.Add(row);

                if (activeWorkbook == workbook)
                {
                    this.SelectedWorkbook = row;
                }
            }

            if (this.SelectedWorkbook == null)
            {
                this.SelectedWorkbook = this.Workbooks[0];
            }
        }

        /// <summary>
        /// Init referenced view models
        /// </summary>
        private void InitModels()
        {
            CrossviewWorkbookData preservedData = null;

            if (this.SelectedWorkbook?.Workbook != null)
            {
                var workbookDataDal = new CrossviewWorkbookDataDal(this.SelectedWorkbook?.Workbook);
                preservedData = workbookDataDal.Read();
            }

            this.ManuallyFilledValues = preservedData?.ManuallySavedValues ?? new Dictionary<string, string>();

            this.ElementSelectorViewModel = new ElementDefinitionSelectorViewModel(
                this.Iteration,
                this.Session,
                preservedData?.SavedElementDefinitions.ToList());

            this.ParameterSelectorViewModel = new ParameterTypeSelectorViewModel(
                this.Iteration,
                this.Session,
                preservedData?.SavedParameterTypes.ToList());
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOk()
        {
            this.DialogResult = new WorkbookSelectionDialogResult(
                true,
                this.SelectedWorkbook?.Workbook,
                this.ElementSelectorViewModel?.ElementDefinitionTargetList.Select(e => e.Thing),
                this.ParameterSelectorViewModel?.ParameterTypeTargetList.Select(p => p.Thing),
                this.ManuallyFilledValues,
                this.PersistValues);
        }
    }
}
