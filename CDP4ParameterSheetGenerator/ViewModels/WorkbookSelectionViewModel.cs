// -------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSelectionViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4ParameterSheetGenerator.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Windows.Input;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using NetOffice.ExcelApi;
    using ReactiveUI;

    /// <summary>
    /// The view-model use to select the a workbook that is open in an excel application
    /// </summary>
    public class WorkbookSelectionViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedWorkbook"/> property.
        /// </summary>
        private WorkbookRowViewModel selectedWorkbook;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookSelectionViewModel"/> class.
        /// </summary>
        /// <param name="application">
        /// The Excel <see cref="Application"/> that contains <see cref="Workbook"/>s.
        /// </param>
        /// <param name="engineeringModelSetup">
        /// The <see cref="EngineeringModelSetup"/> that is being rebuild
        /// </param>
        /// <param name="iterationSetup">
        /// The <see cref="IterationSetup"/> that is being rebuild
        /// </param>
        /// <param name="domain">
        /// The <see cref="DomainOfExpertise"/> that is being rebuild
        /// </param>
        public WorkbookSelectionViewModel(Application application, EngineeringModelSetup engineeringModelSetup, IterationSetup iterationSetup, DomainOfExpertise domain)
        {
            this.Workbooks = new List<WorkbookRowViewModel>();
            this.DialogTitle = "Select the workbook to rebuild";
            this.Model = engineeringModelSetup;
            this.Iteration = iterationSetup;
            this.Domain = domain;
            this.PopulateWorkbooks(application);

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);

            var canOk = this.WhenAny(vm => vm.SelectedWorkbook, vm => vm.Value != null);
            this.OkCommand = ReactiveCommandCreator.Create(this.ExecuteOk, canOk);
        }

        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle { get; private set; }

        /// <summary>
        /// Gets the <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup Model { get; private set; }

        /// <summary>
        /// Gets the <see cref="IterationSetup"/>
        /// </summary>
        public IterationSetup Iteration { get; private set; }

        /// <summary>
        /// Gets the <see cref="DomainOfExpertise"/>
        /// </summary>
        public DomainOfExpertise Domain { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="WorkbookRowViewModel"/>s that is available in the current Excel application.
        /// </summary>
        public List<WorkbookRowViewModel> Workbooks { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="WorkbookRowViewModel"/>
        /// </summary>
        public WorkbookRowViewModel SelectedWorkbook
        {
            get { return this.selectedWorkbook; }
            set { this.RaiseAndSetIfChanged(ref this.selectedWorkbook, value); }
        }

        /// <summary>
        /// Gets the Select <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// populate the workbook row view-models that represent workbooks that are open in the
        /// excel application.
        /// </summary>
        /// <param name="application">
        /// The Excel <see cref="Application"/> that contains workbooks
        /// </param>
        private void PopulateWorkbooks(Application application)
        {
            foreach (var workbook in application.Workbooks)
            {
                var row = new WorkbookRowViewModel(workbook);
                this.Workbooks.Add(row);
            }
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
            this.DialogResult = new WorkbookSelectionDialogResult(true, this.selectedWorkbook.Workbook);
        }
    }
}
