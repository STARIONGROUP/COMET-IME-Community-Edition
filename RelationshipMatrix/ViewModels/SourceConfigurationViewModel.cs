// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceConfigurationViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;
    using Settings;

    /// <summary>
    /// The purpose of the <see cref="SourceConfigurationViewModel"/> is to configure what kind of data
    /// is to be shown on the column and row of the relation matrix.
    /// </summary>
    public class SourceConfigurationViewModel : MatrixConfigurationViewModelBase
    {
        /// <summary>
        /// The possible choice of type of <see cref="Thing"/> to display in the matrix
        /// </summary>
        private readonly List<ClassKind> possibleClassKind = new List<ClassKind>();

        /// <summary>
        /// The possible choice of properties to display in the matrix axis
        /// </summary>
        private readonly List<DisplayKind> possibleDisplayKinds = new List<DisplayKind>();

        /// <summary>
        /// Backing field for <see cref="SelectedClassKind"/> property.
        /// </summary>
        private ClassKind? selectedClassKind;

        /// <summary>
        /// Backing field for <see cref="SelectedDisplayKind"/> property.
        /// </summary>
        private DisplayKind selectedDisplayKind;

        /// <summary>
        /// Backing field for <see cref="SelectedSortKind"/> property.
        /// </summary>
        private DisplayKind selectedSortKind;

        /// <summary>
        /// Backing field for <see cref="SelectedSortOrder"/> property.
        /// </summary>
        private SortOrder selectedSortOrder;

        /// <summary>
        /// Backing field for <see cref="SelectedCategories"/>
        /// </summary>
        private List<Category> selectedCategories;

        /// <summary>
        /// The possible choices of <see cref="CategoryBooleanOperatorKind"/>
        /// </summary>
        private readonly List<CategoryBooleanOperatorKind> possibleBooleanOperatorKinds = Enum.GetValues(typeof(CategoryBooleanOperatorKind)).Cast<CategoryBooleanOperatorKind>().ToList();

        /// <summary>
        /// Backing field for <see cref="SelectedCategoryBooleanOperatorKind"/>
        /// </summary>
        private CategoryBooleanOperatorKind selectedBooleanOperatorKind;

        /// <summary>
        /// Backing field for <see cref="IncludeSubcategories"/>
        /// </summary>
        private bool includeSubcategories;

        /// <summary>
        /// Backing field for <see cref="IsSourceYSelected" />
        /// </summary>
        private bool isSourceYSelected;

        /// <summary>
        /// Backing field for <see cref="IsSourceXSelected" />
        /// </summary>
        private bool isSourceXSelected;

        /// <summary>
        /// Backing field for <see cref="CategoriesString" />
        /// </summary>
        private string categoriesString;

        /// <summary>
        /// Backing field for <see cref="SelectedOwners"/> property
        /// </summary>
        private List<DomainOfExpertise> selectedOwners;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceConfigurationViewModel"/> class
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="iteration">The current iteration</param>
        /// <param name="onUpdateAction">The action to perform on update</param>
        /// <param name="onLightUpdateAction">The action to perform on update without need to rebuild relationship configuration</param>
        /// <param name="settings">The module settings</param>
        public SourceConfigurationViewModel(ISession session, Iteration iteration, Action onUpdateAction, Action onLightUpdateAction,
            RelationshipMatrixPluginSettings settings) : base(session, iteration, onUpdateAction, settings)
        {
            this.OnLightUpdateAction = onLightUpdateAction;

            this.possibleClassKind.AddRange(this.PluginSetting.PossibleClassKinds.OrderBy(x => x.ToString()));
            this.possibleDisplayKinds.AddRange(this.PluginSetting.PossibleDisplayKinds.OrderBy(x => x.ToString()));

            this.PossibleCategories = new ReactiveList<Category>();
            this.PossibleCategories.ChangeTrackingEnabled = true;
            this.SelectedCategories = new List<Category>();

            this.PossibleOwners = new ReactiveList<DomainOfExpertise>();
            this.PossibleOwners.ChangeTrackingEnabled = true;
            this.SelectedOwners= new List<DomainOfExpertise>();

            // default boolean operator is AND
            this.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.AND;
            this.IncludeSubcategories = true;

            this.WhenAnyValue(x => x.SelectedClassKind).Skip(1).Subscribe(_ =>
            {
                this.PopulatePossibleCategories();
                this.PopulatePossibleOwners();
                this.OnUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedDisplayKind).Skip(1).Subscribe(_ =>
            {
                this.OnLightUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedSortKind).Skip(1).Subscribe(_ =>
            {
                this.OnLightUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedSortOrder).Skip(1).Subscribe(_ =>
            {
                this.OnLightUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedCategories).Skip(1).Subscribe(_ =>
            {
                this.OnLightUpdateAction();
                this.SelectCategoriesThatIncludeSubcategories();
            });

            this.WhenAnyValue(x => x.IncludeSubcategories).Skip(1).Subscribe(_ =>
            {
                this.OnLightUpdateAction();
                this.SelectCategoriesThatIncludeSubcategories();
            });

            this.WhenAnyValue(x => x.SelectedBooleanOperatorKind).Skip(1).Subscribe(_ =>
            {
                this.OnLightUpdateAction();
                this.SelectCategoriesThatIncludeSubcategories();
            });

            this.WhenAnyValue(x => x.SelectedOwners).Skip(1).Subscribe(_ => this.OnLightUpdateAction());
            this.WhenAnyValue(x => x.SelectedCategories).Subscribe(x => this.IsSourceXSelected = x.Any());
            this.WhenAnyValue(x => x.SelectedCategories).Subscribe(x => this.IsSourceYSelected = x.Any());

            var categorySubscription = CDPMessageBus.Current
                .Listen<ObjectChangedEvent>(typeof(Category))
                .Where(objectChange => objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => {
                    this.PopulatePossibleCategories();
                    this.OnLightUpdateAction();
                    this.SelectCategoriesThatIncludeSubcategories();
                });

            this.Disposables.Add(categorySubscription);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceConfigurationViewModel"/> class
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="iteration">The current iteration</param>
        /// <param name="onUpdateAction">The action to perform on update</param>
        /// <param name="onLightUpdateAction">The action to perform on update without need to rebuild relationship configiguration</param>
        /// <param name="settings">The module settings</param>
        /// <param name="source">The source <see cref="SourceConfiguration"/></param>
        public SourceConfigurationViewModel(ISession session, Iteration iteration, Action onUpdateAction, Action onLightUpdateAction,
            RelationshipMatrixPluginSettings settings, SourceConfiguration source) : this(session, iteration, onUpdateAction, onLightUpdateAction, settings)
        {
            this.IncludeSubcategories = source.IncludeSubcategories;

            this.SelectedBooleanOperatorKind = source.SelectedBooleanOperatorKind;
            this.SelectedClassKind = source.SelectedClassKind;
            this.SelectedDisplayKind = source.SelectedDisplayKind;
            this.SelectedSortKind = source.SelectedSortKind;
            this.selectedSortOrder = source.SortOrder;

            this.OnLightUpdateAction = onLightUpdateAction;

            // populate selected categories
            var categories = new List<Category>();
            foreach (var iid in source.SelectedCategories)
            {
                var category = this.PossibleCategories.FirstOrDefault(c => c.Iid == iid);

                if (category != null)
                {
                    categories.Add(category);
                }
            }
            this.SelectedCategories = new List<Category>(categories);

            // populate selected owners
            var owners = new List<DomainOfExpertise>();
            foreach (var iid in source.SelectedOwners)
            {
                var domainOfExpertise = this.PossibleOwners.FirstOrDefault(d => d.Iid == iid);
                if (domainOfExpertise != null)
                {
                    owners.Add(domainOfExpertise);
                }
            }
            this.SelectedOwners = new List<DomainOfExpertise>(owners);
        }

        /// <summary>
        /// Gets the action that shall be performed when an instance of <see cref="MatrixConfigurationViewModelBase"/> is updated that does not need a relationship list rebuild
        /// </summary>
        protected Action OnLightUpdateAction { get; }

        /// <summary>
        /// Gets or sets the selected <see cref="ClassKind"/>
        /// </summary>
        public ClassKind? SelectedClassKind
        {
            get { return this.selectedClassKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedClassKind, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="DisplayKind"/>
        /// </summary>
        public DisplayKind SelectedDisplayKind
        {
            get { return this.selectedDisplayKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedDisplayKind, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="DisplayKind"/> for sorting
        /// </summary>
        public DisplayKind SelectedSortKind
        {
            get { return this.selectedSortKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSortKind, value); }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="SortOrder"/>
        /// </summary>
        public SortOrder SelectedSortOrder
        {
            get { return this.selectedSortOrder; }
            set { this.RaiseAndSetIfChanged(ref this.selectedSortOrder, value); }
        }

        /// <summary>
        /// Gets or sets the selected categories
        /// </summary>
        public List<Category> SelectedCategories
        {
            get { return this.selectedCategories; }
            set { this.RaiseAndSetIfChanged(ref this.selectedCategories, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="ClassKind"/> that may be displayed in the matrix
        /// </summary>
        public List<ClassKind> PossibleClassKinds => this.possibleClassKind;

        /// <summary>
        /// Gets the possible <see cref="DisplayKind"/> that may be displayed in the matrix
        /// </summary>
        public List<DisplayKind> PossibleDisplayKinds => this.possibleDisplayKinds;

        /// <summary>
        /// Gets the possible <see cref="Category"/> associated to the <see cref="SelectedClassKind"/>
        /// </summary>
        /// <remarks>
        /// If <see cref="SelectedClassKind"/> is null then all <see cref="Category"/> for <see cref="PossibleClassKinds"/> are used
        /// </remarks>
        public ReactiveList<Category> PossibleCategories { get; }

        /// <summary>
        /// Gets or sets the selected categories names
        /// </summary>
        public ReactiveList<string> SelectedCategoriesToDisplay { get; set; }

        /// <summary>
        /// Gets the possible <see cref="DomainOfExpertise"/> associated to the <see cref="SelectedClassKind"/>
        /// </summary>
        /// <remarks>
        /// If <see cref="SelectedClassKind"/> is null then all <see cref="DomainOfExpertise"/> are used.
        /// </remarks>
        public ReactiveList<DomainOfExpertise> PossibleOwners { get; }

        /// <summary>
        /// Gets or sets the selected owner <see cref="DomainOfExpertise"/>
        /// </summary>
        public List<DomainOfExpertise> SelectedOwners
        {
            get { return this.selectedOwners; }
            set { this.RaiseAndSetIfChanged(ref this.selectedOwners, value); }
        }

        /// <summary>
        /// Gets the possible <see cref="CategoryBooleanOperatorKind"/> that may be used to filter the categpries.
        /// </summary>
        public List<CategoryBooleanOperatorKind> PossibleBooleanOperatorKinds => this.possibleBooleanOperatorKinds;

        /// <summary>
        /// Gets or sets the selected <see cref="CategoryBooleanOperatorKind"/>
        /// </summary>
        public CategoryBooleanOperatorKind SelectedBooleanOperatorKind
        {
            get { return this.selectedBooleanOperatorKind; }
            set { this.RaiseAndSetIfChanged(ref this.selectedBooleanOperatorKind, value); }
        }

        /// <summary>
        /// Gets or sets the the value indicating whether subcategories should be included
        /// </summary>
        public bool IncludeSubcategories
        {
            get { return this.includeSubcategories; }
            set { this.RaiseAndSetIfChanged(ref this.includeSubcategories, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the source Category from X-Axis is selected
        /// </summary>
        public bool IsSourceXSelected
        {
            get { return this.isSourceXSelected; }
            private set { this.RaiseAndSetIfChanged(ref this.isSourceXSelected, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the source Category from Y-Axis is selected
        /// </summary>
        public bool IsSourceYSelected
        {
            get { return this.isSourceYSelected; }
            private set { this.RaiseAndSetIfChanged(ref this.isSourceYSelected, value); }
        }

        /// <summary>
        /// Gets or sets the selected categories names
        /// </summary>
        public string CategoriesString
        {
            get { return this.categoriesString; }
            private set { this.RaiseAndSetIfChanged(ref this.categoriesString, value); }
        }

        /// <summary>
        /// Populates the possible <see cref="Category"/>
        /// </summary>
        private void PopulatePossibleCategories()
        {
            this.PossibleCategories.Clear();

            if (this.SelectedClassKind.HasValue)
            {
                var categories = this.ReferenceDataLibraries.SelectMany(x => x.DefinedCategory)
                    .Where(x => x.PermissibleClass.Contains(this.SelectedClassKind.Value)).OrderBy(x => x.Name);
                this.PossibleCategories.AddRange(categories);
            }
            else
            {
                var categories = this.ReferenceDataLibraries.SelectMany(x => x.DefinedCategory)
                    .Where(x => x.PermissibleClass.Intersect(this.PossibleClassKinds).Any()).OrderBy(x => x.Name);
                this.PossibleCategories.AddRange(categories);
            }
        }

        /// <summary>
        /// Populates the possible <see cref="DomainOfExpertise"/> of the <see cref="PossibleOwners"/> property
        /// </summary>
        private void PopulatePossibleOwners()
        {
            this.PossibleOwners.Clear();

            var engineeringModel = this.Iteration.TopContainer as EngineeringModel;
            var domains = engineeringModel.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name);
            this.PossibleOwners.AddRange(domains);
        }

        /// <summary>
        /// Populates the selected <see cref="Category"/> name accordingly to the IncludeSubcategories value
        /// </summary>
        private void SelectCategoriesThatIncludeSubcategories()
        {
            var categories = new List<string>();

            if (this.IncludeSubcategories)
            {
                foreach (var item in this.SelectedCategories)
                {
                    var values = item.AllDerivedCategories().Select(x => x.Name);

                    categories.Add(values.Count() > 1 ? $"({item.Name} OR {string.Join(" OR ", item.AllDerivedCategories().Select(x => x.Name))})" : $"({item.Name})");
                }
            }
            else
            {
                categories.AddRange(this.SelectedCategories.Select(x => x.Name));
            }

            this.CategoriesString = string.Join($" {this.SelectedBooleanOperatorKind} ", categories);
        }
    }
}