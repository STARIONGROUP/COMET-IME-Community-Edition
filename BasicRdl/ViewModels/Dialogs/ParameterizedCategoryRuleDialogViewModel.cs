// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterizedCategoryRuleDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ParameterizedCategoryRuleDialogViewModel"/> is to allow an <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule"/> will result in an <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.ParameterizedCategoryRule)]
    public class ParameterizedCategoryRuleDialogViewModel : CDP4CommonView.ParameterizedCategoryRuleDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedCategoryRuleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParameterizedCategoryRuleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterizedCategoryRuleDialogViewModel"/> class.
        /// </summary>
        /// <param name="parameterizedCategoryRule">
        /// The <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="CDP4Common.CommonData.Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="ParameterizedCategoryRuleDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="ParameterizedCategoryRuleDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.
        /// </param>
        /// <param name="container">
        /// The Container <see cref="CDP4Common.CommonData.Thing"/> of the created <see cref="CDP4Common.SiteDirectoryData.ParameterizedCategoryRule"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ParameterizedCategoryRuleDialogViewModel(ParameterizedCategoryRule parameterizedCategoryRule, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(parameterizedCategoryRule, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => 
                    {
                        this.PopulatePossibleCategory();
                        this.PopulatePossibleParameterTypes();
                        this.UpdateOkCanExecute();
                    });
            this.WhenAnyValue(vm => vm.ParameterType.Count).Subscribe(_ => this.UpdateOkCanExecute());
        }

        /// <summary>
        /// Updates the <see cref="DialogViewModelBase{T}.OkCanExecute"/> property using validation rules
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.Container != null && this.SelectedCategory != null && this.ParameterType.Any();
        }

        /// <summary>
        /// Populates the <see cref="PopulatePossibleCategory"/> property
        /// </summary>
        protected override void PopulatePossibleCategory()
        {
            base.PopulatePossibleCategory();
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl == null)
            {
                return;
            }
            
            var allCategories = containerRdl.DefinedCategory;
            allCategories.AddRange(containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory));
            this.PossibleCategory.AddRange(allCategories.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.PopulatePossibleParameterTypes();
        }

        /// <summary>
        /// The populate possible parameter types.
        /// </summary>
        private void PopulatePossibleParameterTypes()
        {
            this.PossibleParameterType.Clear();
            var containerRdl = this.Container as ReferenceDataLibrary;
            if (containerRdl == null)
            {
                return;
            }

            var allParameterTypes = containerRdl.ParameterType.ToList();
            allParameterTypes.AddRange(containerRdl.GetRequiredRdls().SelectMany(rdl => rdl.ParameterType));
            this.PossibleParameterType.AddRange(this.ParameterType.OrderBy(c => c.ShortName));
            this.PossibleParameterType.AddRange(allParameterTypes.Except(this.ParameterType).OrderBy(c => c.ShortName));
        }
    }
}
