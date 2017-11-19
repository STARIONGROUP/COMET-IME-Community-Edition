// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportGlossarySelectionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using Microsoft.Practices.ServiceLocation;
    using NLog;
    using ReactiveUI;
    using ReportGenerators;

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

            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.BrowseCommand = ReactiveCommand.Create();
            this.BrowseCommand.Subscribe(_ => this.ExecuteBrowse());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
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
            get { return this.selectedGlossary; }
            set { this.RaiseAndSetIfChanged(ref this.selectedGlossary, value); }
        }

        /// <summary>
        /// Gets or sets path to export the HTML report to.
        /// </summary>
        public string HtmlReportPath
        {
            get { return this.htmlReportPath; }
            set { this.RaiseAndSetIfChanged(ref this.htmlReportPath, value); }
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Gets the Browse Command
        /// </summary>
        public ReactiveCommand<object> BrowseCommand { get; private set; }

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