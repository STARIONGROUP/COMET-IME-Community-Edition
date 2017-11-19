// -------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportRequirementsSpecificationSelectionDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;

    using CDP4Requirements.ViewModels.HtmlReport;

    using NLog;
    using ReactiveUI;
    
    /// <summary>
    /// The view-model to export the <see cref="RequirementsSpecification"/>s of an <see cref="Iteration"/>
    /// </summary>
    public class HtmlExportRequirementsSpecificationSelectionDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for the <see cref="SelectedEngineeringModel"/> property.
        /// </summary>
        private EngineeringModel selectedEngineeringModel;

        /// <summary>
        /// Backing field for the <see cref="SelectedIteration"/> property.
        /// </summary>
        private Iteration selectedIteration;

        /// <summary>
        /// Backing field for the <see cref="SelectedRequirementsSpecification"/> property.
        /// </summary>
        private RequirementsSpecification selectedRequirementsSpecification;
        
        /// <summary>
        /// Backing field for the <see cref="HtmlReportPath"/> property.
        /// </summary>
        private string htmlReportPath;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/> that is used to navigate to the File Open/Save dialog
        /// </summary>
        private readonly IOpenSaveFileDialogService openSaveFileDialogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlExportRequirementsSpecificationSelectionDialogViewModel"/>
        /// </summary>
        /// <param name="iterations">
        /// The <see cref="Iteration"/>s from which a <see cref="RequirementsSpecification"/> can be selected
        /// </param>
        /// <param name="openSaveFileDialogService">
        /// The <see cref="IOpenSaveFileDialogService"/> that can be used to select a file(path) to open or save
        /// </param>
        public HtmlExportRequirementsSpecificationSelectionDialogViewModel(IEnumerable<Iteration> iterations, IOpenSaveFileDialogService openSaveFileDialogService)
        {
            this.openSaveFileDialogService = openSaveFileDialogService;
            
            this.InitializeReactiveLists();

            this.WhenAnyValue(vm => vm.SelectedEngineeringModel).Where(em => em != null).Subscribe(this.LoadIterations);
            this.WhenAnyValue(vm => vm.SelectedIteration).Where(i => i != null).Subscribe(this.LoadRequirementsSpecifications);

            var canOk = this.WhenAnyValue(
                vm => vm.HtmlReportPath,
                vm => vm.SelectedIteration,
                (path, iteration) => !string.IsNullOrEmpty(path) && iteration != null);

            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.BrowseCommand = ReactiveCommand.Create();
            this.BrowseCommand.Subscribe(_ => this.ExecuteBrowse());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.LoadEngineeringModels(iterations);
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveList{T}"/> that contain the possible <see cref="EngineeringModel"/>, <see cref="Iteration"/> and <see cref="RequirementsSpecification"/>
        /// </summary>
        private void InitializeReactiveLists()
        {
            this.PossibleEngineeringModels = new ReactiveList<EngineeringModel>();
            this.PossibleIterations = new ReactiveList<Iteration>();
            this.PossibleRequirementsSpecifications = new ReactiveList<RequirementsSpecification>();
        }

        /// <summary>
        /// Loads the <see cref="EngineeringModel"/>s
        /// </summary>
        /// <param name="iterations">
        /// The <see cref="Iteration"/>s from which the <see cref="EngineeringModel"/>s have to be derived
        /// </param>
        private void LoadEngineeringModels(IEnumerable<Iteration> iterations)
        {
            var engineeringModels = new List<EngineeringModel>();

            foreach (var iteration in iterations)
            {
                var engineeringModel = (EngineeringModel)iteration.Container;
                if (!this.PossibleEngineeringModels.Contains(engineeringModel))
                {
                    engineeringModels.Add(engineeringModel);                    
                }
            }

            var sortedModels = engineeringModels.OrderBy(em => em.EngineeringModelSetup.ShortName);
            this.PossibleEngineeringModels.AddRange(sortedModels);

            if (this.PossibleEngineeringModels.Count == 1)
            {
                this.SelectedEngineeringModel = this.PossibleEngineeringModels.First();
            }
        }

        /// <summary>
        /// Loads the <see cref="Iteration"/>s from which a <see cref="RequirementsSpecification"/> can be selected from.
        /// </summary>
        /// <param name="engineeringModel">
        /// The <see cref="EngineeringModel"/>s that contains the <see cref="Iteration"/>s
        /// </param>
        private void LoadIterations(EngineeringModel engineeringModel)
        {
            var iterations = new List<Iteration>();
            foreach (var iteration in engineeringModel.Iteration)
            {
                iterations.Add(iteration);
            }

            var sortedIterations = iterations.OrderBy(i => i.IterationSetup.IterationNumber);
            
            this.PossibleIterations.Clear();
            this.PossibleIterations.AddRange(sortedIterations);

            if (this.PossibleIterations.Count == 1)
            {
                this.SelectedIteration = this.PossibleIterations.First();
            }
        }

        /// <summary>
        /// Loads the <see cref="RequirementsSpecification"/>s that can be selected from.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>s that contains the <see cref="RequirementsSpecification"/>s
        /// </param>
        private void LoadRequirementsSpecifications(Iteration iteration)
        {
            var requirementsSpecifications = new List<RequirementsSpecification>();
            foreach (var requirementsSpecification in iteration.RequirementsSpecification)
            {
                requirementsSpecifications.Add(requirementsSpecification);
            }

            var sortedRequirementsSpecifications = requirementsSpecifications.OrderBy(rs => rs.ShortName);

            this.PossibleRequirementsSpecifications.Clear();
            this.PossibleRequirementsSpecifications.AddRange(sortedRequirementsSpecifications);

            if (this.PossibleRequirementsSpecifications.Count == 1)
            {
                this.SelectedRequirementsSpecification = this.PossibleRequirementsSpecifications.Single();
            }
        }

        /// <summary>
        /// Gets the <see cref="ReactiveList{EngineeringModel}"/> that a selection can be made from.
        /// </summary>
        public ReactiveList<EngineeringModel> PossibleEngineeringModels { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveList{Iteration}"/> that a selection can be made from.
        /// </summary>
        public ReactiveList<Iteration> PossibleIterations { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveList{RequirementsSpecification}"/> that a selection can be made from.
        /// </summary>
        public ReactiveList<RequirementsSpecification> PossibleRequirementsSpecifications { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="EngineeringModel"/> to export a <see cref="RequirementsSpecification"/> from.
        /// </summary>
        public EngineeringModel SelectedEngineeringModel
        {
            get { return this.selectedEngineeringModel; }
            set { this.RaiseAndSetIfChanged(ref this.selectedEngineeringModel, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="Iteration"/> to export a <see cref="RequirementsSpecification"/> from.
        /// </summary>
        public Iteration SelectedIteration
        {
            get { return this.selectedIteration; }
            set { this.RaiseAndSetIfChanged(ref this.selectedIteration, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="RequirementsSpecification"/> to export.
        /// </summary>
        public RequirementsSpecification SelectedRequirementsSpecification
        {
            get { return this.selectedRequirementsSpecification; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRequirementsSpecification, value); }
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
            this.LoadingMessage = "Generating HTMl Report...";
            this.ErrorMessage = string.Empty;

            var htmlReportGenerator = new HtmlReportGenerator();

            try
            {
                logger.Debug("Start Rendering report for RequirementsSpecification: {0}", this.selectedRequirementsSpecification.Iid);

                var htmlReport = htmlReportGenerator.Render(this.selectedRequirementsSpecification);

                logger.Debug("RequirementsSpecification report rendered: {0}", this.selectedRequirementsSpecification.Iid);
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
