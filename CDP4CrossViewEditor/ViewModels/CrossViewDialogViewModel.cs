// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

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
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// ViewModel that corresponds to the element selector area
        /// </summary>
        public ThingSelectorViewModel ElementSelectorViewModel { get; private set; }

        /// <summary>
        /// ViewModel that corresponds to the element selector area
        /// </summary>
        public ThingSelectorViewModel ParameterSelectorViewModel { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="WorkbookRowViewModel"/>s that is available in the current Excel application.
        /// </summary>
        private List<WorkbookRowViewModel> Workbooks { get; set; }

        /// <summary>
        /// Gets or sets the selected <see cref="WorkbookRowViewModel"/>
        /// </summary>
        private WorkbookRowViewModel SelectedWorkbook
        {
            get => this.selectedWorkbook;
            set => this.RaiseAndSetIfChanged(ref this.selectedWorkbook, value);
        }

        /// <summary>
        ///
        /// </summary>
        private Dictionary<string, string> ManuallyFilledValues = new Dictionary<string, string>();

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
            this.DialogTitle = "Select equipments and parameters";
            this.Iteration = iteration;
            this.Session = session;
            this.InitWorkbooks(application, activeWorkbook);
            this.InitModels();

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.OkCommand = ReactiveCommand.Create();
            this.OkCommand.Subscribe(_ => this.ExecuteOk());
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
        [ExcludeFromCodeCoverage]
        private void InitModels()
        {
            CrossviewWorkbookData preservedData = null;

            if (this.SelectedWorkbook != null)
            {
                var workbookDataDal = new CrossviewWorkbookDataDal(this.SelectedWorkbook?.Workbook);
                preservedData = workbookDataDal.Read();
                this.ManuallyFilledValues = preservedData?.ManuallySavedValues ?? new Dictionary<string, string>();
            }

            this.ElementSelectorViewModel = new ElementDefinitionSelectorViewModel(
                this.Iteration,
                this.Session,
                preservedData?.ElementDefinitionList?.Select(ed => ed.Iid).ToList());

            this.ParameterSelectorViewModel = new ParameterTypeSelectorViewModel(
                this.Iteration,
                this.Session,
                preservedData?.ParameterTypeList?.Select(pt => pt.Iid).ToList());
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
            if (this.ElementSelectorViewModel is ElementDefinitionSelectorViewModel elementDefinitionViewModel &&
                this.ParameterSelectorViewModel is ParameterTypeSelectorViewModel parameterTypeViewModel)
            {
                this.DialogResult = new WorkbookSelectionDialogResult(true, this.SelectedWorkbook?.Workbook, elementDefinitionViewModel.ElementDefinitionTargetList.Select(e => e.Thing),
                    parameterTypeViewModel.ParameterTypeTargetList.Select(p => p.Thing),
                    this.ManuallyFilledValues);
            }
        }
    }
}
