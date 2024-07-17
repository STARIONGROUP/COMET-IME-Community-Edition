// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportGlossarySelectionDialogViewModel.cs" company="Starion Group S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;

    using BasicRdl.ReportGenerators;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CommonServiceLocator;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The view-model to export the <see cref="Glossary"/> to an html report.
    /// </summary>
    public class HtmlExportGlossarySelectionDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for the <see cref="SelectedGlossary"/> property.
        /// </summary>
        private Glossary selectedGlossary;

        /// <summary>
        /// Backing field for the <see cref="HtmlReportPath"/> property.
        /// </summary>
        private string htmlReportPath;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/> that is used to navigate to the File Open/Save dialog
        /// </summary>
        private readonly IOpenSaveFileDialogService openSaveFileDialogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlExportGlossarySelectionDialogViewModel"/> class. 
        /// </summary>
        /// <param name="openGlosarries">
        /// The list of open glossaries.
        /// </param>
        public HtmlExportGlossarySelectionDialogViewModel(List<Glossary> openGlosarries)
        {
            this.openSaveFileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

            this.InitializeReactiveLists();
            this.LoadGlossaries(openGlosarries);

            var canOk = this.WhenAnyValue<HtmlExportGlossarySelectionDialogViewModel, bool, string>(
                vm => vm.HtmlReportPath, 
                path => !string.IsNullOrEmpty(path));

            this.OkCommand = ReactiveCommandCreator.Create(this.ExecuteOk, canOk);

            this.BrowseCommand = ReactiveCommandCreator.Create(this.ExecuteBrowse);

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveList{T}"/> that contain the possible <see cref="Glossary"/>
        /// </summary>
        private void InitializeReactiveLists()
        {
            this.PossibleGlossaries = new ReactiveList<Glossary>();
        }

        /// <summary>
        /// Loads the <see cref="Glossary"/>s that can be selected from.
        /// </summary>
        /// <param name="openGlossaries">
        /// The list of open <see cref="Glossary"/>s.
        /// </param>
        private void LoadGlossaries(List<Glossary> openGlossaries)
        {
            this.PossibleGlossaries.AddRange(openGlossaries.OrderBy(p => p.ShortName));

            if (this.SelectedGlossary == null && this.PossibleGlossaries.Count > 0)
            {
                this.SelectedGlossary = this.PossibleGlossaries.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveList{Glossary}"/> that a selection can be made from.
        /// </summary>
        public ReactiveList<Glossary> PossibleGlossaries { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="Glossary"/> to export.
        /// </summary>
        public Glossary SelectedGlossary
        {
            get => this.selectedGlossary;
            set => this.RaiseAndSetIfChanged(ref this.selectedGlossary, value);
        }

        /// <summary>
        /// Gets or sets path to export the HTML report to.
        /// </summary>
        public string HtmlReportPath
        {
            get => this.htmlReportPath;
            set => this.RaiseAndSetIfChanged(ref this.htmlReportPath, value);
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the Browse Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> BrowseCommand { get; private set; }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteBrowse()
        {
            this.HtmlReportPath = this.openSaveFileDialogService.GetSaveFileDialog("Untitled", ".html", "HTML files (.html)|*.html", this.HtmlReportPath, 1);
        }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        private void ExecuteOk()
        {
            this.IsBusy = true;
            this.LoadingMessage = "Generating HTML Report...";
            this.ErrorMessage = string.Empty;

            var htmlReportGenerator = new HtmlGlossaryReportGenerator();

            try
            {
                logger.Debug("Start Rendering report for Glossary: {0}", this.selectedGlossary.Iid);

                var htmlReport = htmlReportGenerator.Render(this.selectedGlossary);

                logger.Debug("Glossary report rendered: {0}", this.selectedGlossary.Iid);
                logger.Trace("HTML Report: /r/n {0}", htmlReport);

                logger.Debug("Writing HTML Report to: {0}", this.HtmlReportPath);
                htmlReportGenerator.Write(htmlReport, this.HtmlReportPath);
                logger.Debug("HTML Report written to: {0}", this.HtmlReportPath);

                this.DialogResult = new BaseDialogResult(true);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;
            }
            finally
            {
                this.IsBusy = false;
            }
        }
    }
}