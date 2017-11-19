// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4CommonView.ViewModels;
    using CDP4Composition.Utilities;

    /// <summary>
    /// The purpose of the <see cref="RequirementDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="Requirement"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.Requirement)]
    public class RequirementDialogViewModel : CDP4CommonView.RequirementDialogViewModel, IThingDialogViewModel
    {
        #region private fields
        /// <summary>
        /// The Required Referance-Data-library for the current <see cref="Iteration"/>
        /// </summary>
        private ModelReferenceDataLibrary mRdl;

        /// <summary>
        /// Backing field for <see cref="SelectedSimpleParameterValue"/>
        /// </summary>
        private SimpleParameterValueRowViewModel selectedSimpleParameterValue;

        /// <summary>
        /// Backing field for <see cref="SelectedParametricConstraintExpression"/>
        /// </summary>
        private IRowViewModelBase<BooleanExpression> selectedParametricConstraintExpression;

        /// <summary>
        /// Backing field for <see cref="SelectedLanguageCode"/>
        /// </summary>
        private LanguageCodeUsage selectedLanguageCode;

        /// <summary>
        /// Backing field for <see cref="RequirementText"/>
        /// </summary>
        private string requirementText;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public RequirementDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementDialogViewModel"/> class.
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="container">The <see cref="RequirementsSpecificationDialogViewModel.container"/> for this <see cref="RequirementsSpecificationDialogViewModel.Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public RequirementDialogViewModel(Requirement requirement, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(requirement, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.SelectedOwner = this.Session.ActivePerson.DefaultDomain ?? this.PossibleOwner.First();

            this.WhenAnyValue(vm => vm.RequirementText).Subscribe(_ => this.UpdateOkCanExecute());
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the list of <see cref="SimpleParameterValue"/>
        /// </summary>
        public ReactiveList<SimpleParameterValueRowViewModel> SimpleParameterValue { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ParametricConstraint"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<BooleanExpression>> ParametricConstraintExpression { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="LanguageCodeUsage"/>
        /// </summary>
        public LanguageCodeUsage SelectedLanguageCode
        {
            get { return this.selectedLanguageCode; }
            set { this.RaiseAndSetIfChanged(ref this.selectedLanguageCode, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="LanguageCodeUsage"/>
        /// </summary>
        public ReactiveList<LanguageCodeUsage> PossibleLanguageCode { get; private set; }

        /// <summary>
        /// Gets or sets the requirement text to display
        /// </summary>
        public string RequirementText
        {
            get { return this.requirementText; }
            set { this.RaiseAndSetIfChanged(ref this.requirementText, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="ParametricConstraintRowViewModel"/>
        /// </summary>
        public IRowViewModelBase<BooleanExpression> SelectedParametricConstraintExpression
        {
            get { return this.selectedParametricConstraintExpression; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParametricConstraintExpression, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="SelectedSimpleParameterValue"/>
        /// </summary>
        public SimpleParameterValueRowViewModel SelectedSimpleParameterValue
        {
            get { return this.selectedSimpleParameterValue; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSimpleParameterValue, value); }
        }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<object> CreateSimpleParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<object> DeleteSimpleParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<object> EditSimpleParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<object> InspectSimpleParameterValueCommand { get; protected set; }
        #endregion

        /// <summary>
        /// Initializes the properties of this <see cref="Requirement"/>
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var iteration = (Iteration)this.Container.Container ?? this.ChainOfContainer.OfType<Iteration>().Single();
            var model = (EngineeringModel)iteration.Container;
            this.mRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            this.SimpleParameterValue = new ReactiveList<SimpleParameterValueRowViewModel>();
            this.ParametricConstraintExpression = new ReactiveList<IRowViewModelBase<BooleanExpression>>();
            this.PossibleLanguageCode = new ReactiveList<LanguageCodeUsage>();
            this.PopulateSimpleParameterValues();
            this.PopulateParametricConstraint();
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s of this dialog
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            var canExecuteCreateSimpleParameterValueCommand = this.WhenAnyValue(vm => vm.IsReadOnly, v => !v);
            var canExecuteInspectSelectedSimpleParameterValueCommand = this.WhenAny(vm => vm.SelectedSimpleParameterValue, v => v.Value != null);
            var canExecuteEditSelectedSimpleParameterValueCommand = this.WhenAny(vm => vm.SelectedSimpleParameterValue, v => v.Value != null && !this.IsReadOnly);

            this.CreateSimpleParameterValueCommand = ReactiveCommand.Create(canExecuteCreateSimpleParameterValueCommand);
            this.CreateSimpleParameterValueCommand.Subscribe(_ => this.ExecuteCreateCommand<SimpleParameterValue>(this.PopulateSimpleParameterValues));

            this.DeleteSimpleParameterValueCommand = ReactiveCommand.Create(canExecuteEditSelectedSimpleParameterValueCommand);
            this.DeleteSimpleParameterValueCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedSimpleParameterValue.Thing, this.PopulateSimpleParameterValues));

            this.EditSimpleParameterValueCommand = ReactiveCommand.Create(canExecuteEditSelectedSimpleParameterValueCommand);
            this.EditSimpleParameterValueCommand.Subscribe(_ => this.ExecuteEditCommand(this.SelectedSimpleParameterValue.Thing, this.PopulateSimpleParameterValues));

            this.InspectSimpleParameterValueCommand = ReactiveCommand.Create(canExecuteInspectSelectedSimpleParameterValueCommand);
            this.InspectSimpleParameterValueCommand.Subscribe(_ => this.ExecuteInspectCommand(this.SelectedSimpleParameterValue.Thing));

            this.WhenAnyValue(x => x.SelectedLanguageCode).Where(x => x != null).Subscribe(x => this.UpdateRequirementText());
        }

        /// <summary>
        /// Updates the property indicating whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        /// <remarks>
        /// The <see cref="DialogViewModelBase{T}.Container"/> may not be null and there may not be any Validation Errors
        /// </remarks>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && !string.IsNullOrEmpty(this.RequirementText);
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            var existingDefinition = this.Thing.Definition.SingleOrDefault(x => x.LanguageCode == this.SelectedLanguageCode.Name);
            if (existingDefinition == null && !string.IsNullOrWhiteSpace(this.RequirementText))
            {
                var newDefinition = new Definition();
                newDefinition.LanguageCode = this.SelectedLanguageCode.Name;
                newDefinition.Content = this.RequirementText;
                this.Thing.Definition.Add(newDefinition);

                this.transaction.Create(newDefinition, this.Thing);
            }
            else
            {
                if (existingDefinition.Content != this.RequirementText)
                {
                    var clone = existingDefinition.Clone(false);
                    clone.Content = this.RequirementText;
                    this.transaction.CreateOrUpdate(clone);
                }
            }
        }

        /// <summary>
        /// Populate the <see cref="PossibleCategory"/> and <see cref="Category"/> properties.
        /// </summary>
        protected override void PopulateCategory()
        {
            var categories = this.mRdl.DefinedCategory.Where(x => x.PermissibleClass.Contains(ClassKind.Requirement)).ToList();
            categories.AddRange(this.mRdl.GetRequiredRdls().SelectMany(x => x.DefinedCategory).Where(x => x.PermissibleClass.Contains(ClassKind.Requirement)));

            this.PossibleCategory.Clear();
            this.PossibleCategory.AddRange(categories);

            base.PopulateCategory();
        }

        /// <summary>
        /// Populates the <see cref="RequirementDialogViewModel.PossibleOwner"/>
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            this.PossibleOwner.AddRange(this.Session.RetrieveSiteDirectory().Domain);
        }

        /// <summary>
        /// Populates the <see cref="RequirementDialogViewModel.PossibleGroup"/>
        /// </summary>
        protected override void PopulatePossibleGroup()
        {
            base.PopulatePossibleGroup();
            var container = (RequirementsSpecification)this.Container;

            this.PossibleGroup.AddRange(container.GetAllContainedGroups());
        }

        /// <summary>
        /// Populates the <see cref="SimpleParameterValue"/> list with the parameter values of this <see cref="Requirement"/>.
        /// </summary>
        protected void PopulateSimpleParameterValues()
        {
            this.SimpleParameterValue.Clear();
            foreach (var thing in this.Thing.ParameterValue.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new SimpleParameterValueRowViewModel(thing, this.Session, this);
                this.SimpleParameterValue.Add(row);
            }
        }

        /// <summary>
        /// Populates the <see cref="ParametricConstraint"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected override void PopulateParametricConstraint()
        {
            this.ParametricConstraint.Clear();
            foreach (var thing in this.Thing.ParametricConstraint.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new ParametricConstraintRowViewModel(thing, this.Session, this);
                this.ParametricConstraint.Add(row);
                row.Index = this.Thing.ParametricConstraint.IndexOf(thing);
            }
        }

        /// <summary>
        /// Populate the language code following the definitions
        /// </summary>
        protected override void PopulateDefinition()
        {
            base.PopulateDefinition();
            this.UpdateLanguageCodes();
        }

        /// <summary>
        /// Update the language code
        /// </summary>
        private void UpdateLanguageCodes()
        {
            this.PossibleLanguageCode.Clear();
            
            var usedCodes = this.Thing.Definition.Select(x => x.LanguageCode);

            var languageCodeUsages = new List<LanguageCodeUsage>();            
            foreach (var usedCode in usedCodes)
            {
                var cultureInfo = CultureInfoUtility.CultureInfoAvailable.SingleOrDefault(x => x.Name == usedCode);

                if (cultureInfo == null)
                {
                    try
                    {
                        cultureInfo = new CultureInfo(usedCode);
                        var languageCodeUsage = new LanguageCodeUsage(cultureInfo, true);
                        languageCodeUsages.Add(languageCodeUsage);
                    }
                    catch (CultureNotFoundException ex)
                    {
                        var languageCodeUsage = new LanguageCodeUsage(usedCode, usedCode, true);
                        languageCodeUsages.Add(languageCodeUsage);
                        logger.Debug(ex, "The culture {0} could not be found and is ignored", usedCode);
                    }
                }
            }

            this.PossibleLanguageCode.AddRange(languageCodeUsages.OrderBy(x => x.FullName));
            
            foreach (var cultureInfo in CultureInfoUtility.CultureInfoAvailable)
            {
                if (languageCodeUsages.All(x => x.Name != cultureInfo.Name))
                {
                    var languageCodeUsage = new LanguageCodeUsage(cultureInfo, false);
                    this.PossibleLanguageCode.Add(languageCodeUsage);
                }
            }
            
            if (this.Thing.Definition.Count == 0)
            {
                this.SelectedLanguageCode = this.PossibleLanguageCode.Single(x => x.Name == CultureInfoUtility.DefaultCultureName);
                return;
            }

            var definition = this.Thing.Definition.SingleOrDefault(x => x.LanguageCode == CultureInfoUtility.DefaultCultureName) ?? this.Thing.Definition.First();
            
            this.SelectedLanguageCode = this.PossibleLanguageCode.Single(x => x.Name == definition.LanguageCode);
        }

        /// <summary>
        /// Update the requirement text
        /// </summary>
        private void UpdateRequirementText()
        {
            var definition = this.Thing.Definition.SingleOrDefault(x => x.LanguageCode == this.SelectedLanguageCode.Name);
            this.RequirementText = definition == null ? string.Empty : definition.Content;
        }
    }
}