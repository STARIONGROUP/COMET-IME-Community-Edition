﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementDialogViewModel.cs" company="Starion Group S.A.">
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;
    
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Mvvm.Types;
    using CDP4Composition.Utilities;

    using ReactiveUI;

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows.Input;

    using CDP4CommonView.ViewModels;
    
    /// <summary>
    /// The purpose of the <see cref="RequirementDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="Requirement"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.Requirement)]
    public class RequirementDialogViewModel : CDP4CommonView.RequirementDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="ShortName"/> property
        /// </summary>
        private string shortName;

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
            if (dialogKind == ThingDialogKind.Create)
            {
                var iteration = (Iteration)container.Container;

                if (this.Session.OpenIterations.TryGetValue(iteration, out var domainOfExpertiseParticipantTuple))
                {
                    this.SelectedOwner = domainOfExpertiseParticipantTuple.Item1 ?? this.PossibleOwner.First();
                }
            }
            
            this.WhenAnyValue(vm => vm.RequirementText).Subscribe(_ => this.UpdateOkCanExecute());
        }
        
        /// <summary>
        /// Gets or sets the list of <see cref="SimpleParameterValue"/>
        /// </summary>
        public DisposableReactiveList<SimpleParameterValueRowViewModel> SimpleParameterValue { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ParametricConstraint"/>
        /// </summary>
        public DisposableReactiveList<IRowViewModelBase<BooleanExpression>> ParametricConstraintExpression { get; protected set; }

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
        /// Gets or sets the ShortName
        /// </summary>
        [ValidationOverride(true, "RequirementShortName")]
        public override string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the Create <see cref="ICommand"/> to create a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> CreateSimpleParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Delete <see cref="ICommand"/> to delete a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteSimpleParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Edit <see cref="ICommand"/> to edit a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> EditSimpleParameterValueCommand { get; protected set; }

        /// <summary>
        /// Gets or sets the Inspect <see cref="ICommand"/> to inspect a SimpleParameterValue
        /// </summary>
        public ReactiveCommand<Unit, Unit> InspectSimpleParameterValueCommand { get; protected set; }
        
        /// <summary>
        /// Initializes the properties of this <see cref="Requirement"/>
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var iteration = (Iteration)this.Container.Container ?? this.ChainOfContainer.OfType<Iteration>().Single();
            var model = (EngineeringModel)iteration.Container;
            this.mRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            this.SimpleParameterValue = new DisposableReactiveList<SimpleParameterValueRowViewModel>();
            this.ParametricConstraintExpression = new DisposableReactiveList<IRowViewModelBase<BooleanExpression>>();
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

            this.CreateSimpleParameterValueCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<SimpleParameterValue>(this.PopulateSimpleParameterValues), canExecuteCreateSimpleParameterValueCommand);

            this.DeleteSimpleParameterValueCommand = ReactiveCommandCreator.Create(() => this.ExecuteDeleteCommand(this.SelectedSimpleParameterValue.Thing, this.PopulateSimpleParameterValues), canExecuteEditSelectedSimpleParameterValueCommand);

            this.EditSimpleParameterValueCommand = ReactiveCommandCreator.Create(() => this.ExecuteEditCommand(this.SelectedSimpleParameterValue.Thing, this.PopulateSimpleParameterValues), canExecuteEditSelectedSimpleParameterValueCommand);

            this.InspectSimpleParameterValueCommand = ReactiveCommandCreator.Create(() => this.ExecuteInspectCommand(this.SelectedSimpleParameterValue.Thing), canExecuteInspectSelectedSimpleParameterValueCommand);

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
            
            var engineeringModel = (EngineeringModel)this.Container.TopContainer;

            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwner.AddRange(domains);
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
            this.SimpleParameterValue.ClearAndDispose();
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