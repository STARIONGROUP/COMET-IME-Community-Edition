// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    
    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="CategoryDialogViewModel"/> is to allow an <see cref="Category"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Category"/> will result in an <see cref="Category"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Category)]
    public class CategoryDialogViewModel : CDP4CommonView.CategoryDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public CategoryDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDialogViewModel"/> class.
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="CategoryDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="CategoryDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public CategoryDialogViewModel(Category category, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(category, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(vm => vm.PermissibleClass).Subscribe(_ => this.UpdateOkCanExecute());
            this.WhenAnyValue(vm => vm.Container).Subscribe(_ => this.PopulateSuperCategory());
        }

        /// <summary>
        /// Gets or sets the possible permissible class
        /// </summary>
        public ReactiveList<ClassKind> PossiblePermissibleClasses { get; set; }

        /// <summary>
        /// Gets or sets the list of possible super categories
        /// </summary>
        public ReactiveList<Category> PossibleSuperCategories { get; set; }

        /// <summary>
        /// Initializes the list of this dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PossiblePermissibleClasses = new ReactiveList<ClassKind>();
            this.PossibleSuperCategories = new ReactiveList<Category>();
        }

        /// <summary>
        /// Populates the <see cref="CategoryDialogViewModel.PermissibleClass"/> property
        /// </summary>
        protected override void PopulatePermissibleClass()
        {
            this.PossiblePermissibleClasses.Clear();

            var possiblePermissibleClasses = typeof(Thing).Assembly.GetTypes().Where(t => typeof(ICategorizableThing).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface).OrderBy(x => x.Name).Select(x => (ClassKind)Enum.Parse(typeof(ClassKind), x.Name));

            foreach (var possiblePermissableClass in possiblePermissibleClasses)
            {
                this.PossiblePermissibleClasses.Add(possiblePermissableClass);

                if (this.Thing.PermissibleClass.Contains(possiblePermissableClass))
                {
                    this.PermissibleClass.Add(possiblePermissableClass);
                }
            }
        }

        /// <summary>
        /// Populates the <see cref="CategoryDialogViewModel.SuperCategory"/> property
        /// </summary>
        protected override void PopulateSuperCategory()
        {
            this.PossibleSuperCategories.Clear();

            foreach (var possibleSuperCategory in this.PopulatePossibleSuperCategories())
            {
                this.PossibleSuperCategories.Add(possibleSuperCategory);

                if (this.Thing.SuperCategory.Contains(possibleSuperCategory))
                {
                    this.SuperCategory.Add(possibleSuperCategory);
                }
            }
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();
            this.OkCanExecute = this.OkCanExecute && this.PermissibleClass.Any();
        }

        /// <summary>
        /// Get the possible ordered super-<see cref="Category"/>
        /// </summary>
        /// <returns>The ordered super <see cref="Category"/>s</returns>
        private IEnumerable<Category> PopulatePossibleSuperCategories()
        {
            var rdlContainer = this.Container as ReferenceDataLibrary;
            if (rdlContainer == null)
            {
                return Enumerable.Empty<Category>();
            }

            var allPossibleSuperCategories = new List<Category>(rdlContainer.DefinedCategory);
            allPossibleSuperCategories.Remove(this.Thing);

            foreach (var rdl in rdlContainer.GetRequiredRdls())
            {
                allPossibleSuperCategories.AddRange(rdl.DefinedCategory);
            }

            var possibleSuperCategories = allPossibleSuperCategories.ToList();

            // TODO Deal with Update of Category, what happens when container is changed?? is it allowed?
            if (this.dialogKind != ThingDialogKind.Create)
            {
                possibleSuperCategories = possibleSuperCategories.Except(this.GetRdlSubCategories(this.Thing)).ToList();
            }

            return possibleSuperCategories.OrderBy(c => c.ShortName);
        }

        /// <summary>
        /// Gets the Sub-categories of a <see cref="Category"/> in the <see cref="ReferenceDataLibrary"/> it is contained in
        /// </summary>
        /// <param name="category">The <see cref="Category"/></param>
        /// <returns>The list of sub-<see cref="Category"/></returns>
        private IEnumerable<Category> GetRdlSubCategories(Category category)
        {
            var subCategories = new List<Category>();
            var rdl = (ReferenceDataLibrary)category.Container;

            foreach (var cat in rdl.DefinedCategory)
            {
                // Get the sub-categories for the current sub-category
                if (!cat.SuperCategory.Contains(category))
                {
                    continue;
                }

                subCategories.AddRange(this.GetRdlSubCategories(cat));
                subCategories.Add(cat);
            }

            return subCategories;
        }
    }
}
