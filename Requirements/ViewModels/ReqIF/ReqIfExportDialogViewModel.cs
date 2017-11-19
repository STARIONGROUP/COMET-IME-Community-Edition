// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Requirements.ReqIFDal;
    using NLog;
    using ReactiveUI;
    using ReqIFSharp;

    /// <summary>
    /// The view-model to export the <see cref="RequirementsSpecification"/>s of an <see cref="Iteration"/>
    /// </summary>
    public class ReqIfExportDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for <see cref="Path"/>
        /// </summary>
        private string path;

        /// <summary>
        /// Backing field for <see cref="SelectedIteration"/>
        /// </summary>
        private ReqIfExportIterationRowViewModel selectedIteration;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private IOpenSaveFileDialogService fileDialogService;

        /// <summary>
        /// The <see cref="IReqIFSerializer"/>
        /// </summary>
        private readonly IReqIFSerializer serializer;

        /// <summary>
        /// Backing field for <see cref="IsDetailExpanded"/>
        /// </summary>
        private bool isDetailExpanded;

        /// <summary>
        /// Backing field for <see cref="ErrorDetailMessage"/>
        /// </summary>
        private string errorDetailMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfExportDialogViewModel"/> class
        /// </summary>
        /// <param name="sessions">The list of <see cref="ISession"/> available</param>
        /// <param name="iterations">The list of <see cref="Iteration"/> available</param>
        /// <param name="fileDialogService">The <see cref="IOpenSaveFileDialogService"/></param>
        /// <param name="serializer">The <see cref="IReqIFSerializer"/></param>
        public ReqIfExportDialogViewModel(IEnumerable<ISession> sessions, IEnumerable<Iteration> iterations, IOpenSaveFileDialogService fileDialogService, IReqIFSerializer serializer)
        {
            if (sessions == null)
            {
                throw new ArgumentNullException("sessions");
            }

            if (iterations == null)
            {
                throw new ArgumentNullException("iterations");
            }

            if (fileDialogService == null)
            {
                throw new ArgumentNullException("fileDialogService");
            }

            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }

            this.Sessions = sessions.ToList();
            this.Iterations = new ReactiveList<ReqIfExportIterationRowViewModel>();
            this.fileDialogService = fileDialogService;
            this.serializer = serializer;

            foreach (var iteration in iterations)
            {
                this.Iterations.Add(new ReqIfExportIterationRowViewModel(iteration));
            }

            var canOk = this.WhenAnyValue(
                vm => vm.Path,
                vm => vm.SelectedIteration,
                (path, iteration) => iteration != null && !string.IsNullOrEmpty(path));

            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.BrowseCommand = ReactiveCommand.Create();
            this.BrowseCommand.Subscribe(_ => this.ExecuteBrowse());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the error message detail should be displayed
        /// </summary>
        public bool IsDetailExpanded
        {
            get { return this.isDetailExpanded; }
            set { this.RaiseAndSetIfChanged(ref this.isDetailExpanded, value); }
        }

        /// <summary>
        /// Gets the detail of the error message
        /// </summary>
        public string ErrorDetailMessage
        {
            get { return this.errorDetailMessage; }
            private set { this.RaiseAndSetIfChanged(ref this.errorDetailMessage, value); }
        }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        public List<ISession> Sessions { get; private set; }

        /// <summary>
        /// Gets the <see cref="Iteration"/> row-representation
        /// </summary>
        public ReactiveList<ReqIfExportIterationRowViewModel> Iterations { get; private set; }

        /// <summary>
        /// Gets or sets the selected iteration to export
        /// </summary>
        public ReqIfExportIterationRowViewModel SelectedIteration
        {
            get { return this.selectedIteration; }
            set { this.RaiseAndSetIfChanged(ref this.selectedIteration, value); }
        }

        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        public string Path
        {
            get { return this.path; }
            set { this.RaiseAndSetIfChanged(ref this.path, value); }
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
        /// Executes the Ok Command
        /// </summary>
        private async void ExecuteOk()
        {
            this.IsBusy = true;
            this.LoadingMessage = "Exporting...";
            this.ErrorMessage = string.Empty;

            try
            {
                if (!this.CheckModelValidity(this.SelectedIteration.Iteration))
                {
                    this.ErrorMessage = "The model contains errors that need to be fixed before exporting the requirements.";
                    return;
                }

                var session =
                    this.Sessions.Single(x => x.Assembler.Cache == this.SelectedIteration.Iteration.Cache);

                var reqifBuilder = new ReqIFBuilder();
                var reqif = reqifBuilder.BuildReqIF(session, this.SelectedIteration.Iteration);

                //todo use validation 
                this.serializer.Serialize(reqif, this.Path, (sender, args) => {});

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

        /// <summary>
        /// Check the validity of the model
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> to check</param>
        /// <returns>True if no violations related to the exported data were found</returns>
        private bool CheckModelValidity(Iteration iteration)
        {
            var model = iteration.Container as EngineeringModel;
            if (model == null)
            {
                this.ErrorMessage = "The container of the selected Iteration is not set.";
                return false;
            }

            var rules = model.RequiredRdls.SelectMany(x => x.Rule).OfType<ParameterizedCategoryRule>();
            var violations = new List<RuleViolation>();

            var thingsToCheck = new List<Guid>();
            this.AddThingsToCheck(iteration, thingsToCheck);

            foreach (var parameterizedCategoryRule in rules)
            {
                violations.AddRange(parameterizedCategoryRule.Verify(iteration).Where(v => v.ViolatingThing.Intersect(thingsToCheck).Any()));
            }

            this.ErrorDetailMessage = string.Join(Environment.NewLine, violations.Select(v => v.Description));
            return !violations.Any();
        }

        /// <summary>
        /// Populate the <paramref name="thingsToCheck"/>
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="thingsToCheck">The collection of <see cref="Guid"/></param>
        private void AddThingsToCheck(Iteration iteration, List<Guid> thingsToCheck)
        {
            var relationships = iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(
                    x =>
                        (x.Source.ClassKind == ClassKind.Requirement || x.Source.ClassKind == ClassKind.RequirementsSpecification || x.Source.ClassKind == ClassKind.RequirementsGroup) &&
                        (x.Target.ClassKind == ClassKind.Requirement || x.Target.ClassKind == ClassKind.RequirementsSpecification || x.Target.ClassKind == ClassKind.RequirementsGroup));

            foreach (var requirementsSpecification in iteration.RequirementsSpecification)
            {
                thingsToCheck.Add(requirementsSpecification.Iid);
                this.AddThingsToVerify(requirementsSpecification, thingsToCheck);
                foreach (var requirement in requirementsSpecification.Requirement)
                {
                    thingsToCheck.Add(requirement.Iid);
                }
            }

            thingsToCheck.AddRange(relationships.Select(x => x.Iid));

        }

        /// <summary>
        /// Populate the <paramref name="thingsToCheck"/>
        /// </summary>
        /// <param name="reqContainer">The <see cref="RequirementsContainer"/></param>
        /// <param name="thingsToCheck">The collection of <see cref="Guid"/> to populate</param>
        private void AddThingsToVerify(RequirementsContainer reqContainer, List<Guid> thingsToCheck)
        {
            foreach (var group in reqContainer.Group)
            {
                thingsToCheck.Add(group.Iid);
                this.AddThingsToVerify(group, thingsToCheck);
            }
        }

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
            this.Path = this.fileDialogService.GetSaveFileDialog("Untitled", ".reqif", "ReqIF files (.reqif)|*.reqif", this.Path, 1);
        }
    }
}