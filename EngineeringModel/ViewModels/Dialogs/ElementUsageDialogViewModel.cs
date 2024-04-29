﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageDialogViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4Dal.Operations;
    
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4EngineeringModel.ViewModels.Dialogs.Rows;

    using ReactiveUI;

    /// <summary>
    /// The dialog-view model to edit or inspect a <see cref="ElementUsage"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ElementUsage)]
    public class ElementUsageDialogViewModel : CDP4CommonView.ElementUsageDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IncludeOption"/> property.
        /// </summary>
        private ReactiveList<Option> includeOption;

        /// <summary>
        /// Backing field for the <see cref="ModelCode"/> property.
        /// </summary>
        private string modelCode;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ElementUsageDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageDialogViewModel"/> class
        /// </summary>
        /// <param name="elementUsage">
        /// The <see cref="ElementUsage"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="DialogViewModelBase{T}"/> is the root of all <see cref="DialogViewModelBase{T}"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        /// <param name="permissionService">
        /// The <see cref="IPermissionService"/> that is used to compute whether the current <see cref="ISession"/> can execute CRUD.
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="isNewContainer">
        /// Value indicating whether the container of the <see cref="Thing"/> that is being created is new
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ElementUsageDialogViewModel(ElementUsage elementUsage, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(elementUsage, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.IncludeOption)
                .Subscribe(
                    _ => this.ExcludeOption = new ReactiveList<Option>(this.PossibleExcludeOption.Except(this.IncludeOption)));
        }
        
        /// <summary>
        /// Gets or sets the list of selected <see cref="Option"/>s
        /// </summary>
        public ReactiveList<Option> IncludeOption
        {
            get { return this.includeOption; }
            set { this.RaiseAndSetIfChanged(ref this.includeOption, value); }
        }

        /// <summary>
        /// Gets or sets a value that represents the ModelCode of the current <see cref="ElementDefinition"/>
        /// </summary>
        public string ModelCode
        {
            get { return this.modelCode; }
            set { this.RaiseAndSetIfChanged(ref this.modelCode, value); }
        }
        
        /// <summary>
        /// Gets the <see cref="List{CategoryRowViewModel}"/> that have been applied to both the referenced <see cref="ElementDefinition"/>
        /// and the current <see cref="ElementUsage"/>
        /// </summary>
        public List<CategoryRowViewModel> AppliedCategories { get; private set; }

        /// <summary>
        /// Populate the possible <see cref="Category"/> for this <see cref="ElementUsage"/>
        /// </summary>
        private void PopulatePossibleCategories()
        {
            this.PossibleCategory.Clear();
            var container = this.Container as ElementDefinition;
            if (container == null)
            {
                throw new InvalidOperationException("the container is not set for this element usage.");
            }

            var model = (EngineeringModel)container.TopContainer;
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedCategories = new List<Category>(mrdl.DefinedCategory.Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));
            allowedCategories.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory)
                        .Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));

            this.PossibleCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Populate the possible <see cref="Option"/>s
        /// </summary>
        private void PopulatePossibleOptions()
        {
            this.PossibleExcludeOption.Clear();
            var container = this.Container as ElementDefinition;
            if (container == null)
            {
                throw new InvalidOperationException("the container is not set for this element usage.");
            }

            var iteration = container.Container as Iteration;
            this.PossibleExcludeOption.AddRange(iteration.Option);
        }

        /// <summary>
        /// Populate the <see cref="Category"/> instances of the referenced <see cref="ElementDefinition"/>
        /// </summary>
        private void PopulateAppliedCategories()
        {
            this.AppliedCategories = new List<CategoryRowViewModel>();

            foreach (var category in this.Thing.ElementDefinition.Category.OrderBy(x => x.ShortName))
            {
                var categoryRowViewModel = new CategoryRowViewModel(category, this.Thing.ElementDefinition);
                this.AppliedCategories.Add(categoryRowViewModel);
            }

            foreach (var category in this.Thing.Category.OrderBy(x => x.ShortName))
            {
                var categoryRowViewModel = new CategoryRowViewModel(category, this.Thing);
                this.AppliedCategories.Add(categoryRowViewModel);
            }
        }
        
        /// <summary>
        /// Initializes the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PopulatePossibleCategories();
            this.PopulatePossibleOptions();
            this.PopulateAppliedCategories();
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            
            this.ModelCode = this.Thing.ModelCode();
        }

        /// <summary>
        /// Populates the included options from the excluded ones.
        /// </summary>
        protected override void PopulateExcludeOption()
        {
            base.PopulateExcludeOption();
            this.IncludeOption = new ReactiveList<Option>(this.PossibleExcludeOption.Except(this.ExcludeOption));
        }

        /// <summary>
        /// Populates the <see cref="PossibleOwner"/>
        /// </summary>
        protected override void PopulatePossibleOwner()
        {
            base.PopulatePossibleOwner();
            var model = this.Thing.TopContainer as EngineeringModel;
            if (model == null)
            {
                throw new InvalidOperationException("The top container is not set for this usage");
            }

            this.PossibleOwner.AddRange(model.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name));
        }
    }
}