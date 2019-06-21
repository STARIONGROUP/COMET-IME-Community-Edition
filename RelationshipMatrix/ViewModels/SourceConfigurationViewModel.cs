// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceConfigurationViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
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
    /// A view-model for dynamic column definition
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
        /// Backing field for <see cref="SelectedDisplay"/> property.
        /// </summary>
        private DisplayKind selectedDisplayKind;

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
        /// Backing field for <see cref="SelectedBooleanOperatorKind"/>
        /// </summary>
        private CategoryBooleanOperatorKind _selectedBooleanOperatorKind;

        /// <summary>
        /// Backing field for <see cref="IncludeSubcategories"/>
        /// </summary>
        private bool includeSubcategories;

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceConfigurationViewModel"/> class
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="iteration">The current iteration</param>
        /// <param name="onUpdateAction">The action to perform on update</param>
        /// <param name="settings">The module settings</param>
        public SourceConfigurationViewModel(ISession session, Iteration iteration, Action onUpdateAction,
            RelationshipMatrixPluginSettings settings) : base(session, iteration, onUpdateAction, settings)
        {
            this.possibleClassKind.AddRange(this.PluginSetting.PossibleClassKinds.OrderBy(x => x.ToString()));
            this.possibleDisplayKinds.AddRange(this.PluginSetting.PossibleDisplayKinds.OrderBy(x => x.ToString()));

            this.PossibleCategories = new ReactiveList<Category>();
            this.PossibleCategories.ChangeTrackingEnabled = true;
            this.SelectedCategories = new List<Category>();

            // default boolean operator is AND
            this.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.AND;
            this.IncludeSubcategories = true;

            this.WhenAnyValue(x => x.SelectedClassKind).Skip(1).Subscribe(_ =>
            {
                this.PopulatePossibleCategories();
                this.OnUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedDisplayKind).Skip(1).Subscribe(_ =>
            {
                this.OnUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedSortOrder).Skip(1).Subscribe(_ =>
            {
                this.OnUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedCategories).Skip(1).Subscribe(_ => this.OnUpdateAction());
            this.WhenAnyValue(x => x.SelectedBooleanOperatorKind).Skip(1).Subscribe(_ => this.OnUpdateAction());
            this.WhenAnyValue(x => x.IncludeSubcategories).Skip(1).Subscribe(_ => this.OnUpdateAction());

            var categorySubscription = CDPMessageBus.Current
                .Listen<ObjectChangedEvent>(typeof(Category))
                .Where(objectChange => objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => {
                    this.PopulatePossibleCategories();
                    this.OnUpdateAction();
                });

            this.Disposables.Add(categorySubscription);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceConfigurationViewModel"/> class
        /// </summary>
        /// <param name="session">The current session</param>
        /// <param name="iteration">The current iteration</param>
        /// <param name="onUpdateAction">The action to perform on update</param>
        /// <param name="settings">The module settings</param>
        /// <param name="source">The source <see cref="SourceConfiguration"/></param>
        public SourceConfigurationViewModel(ISession session, Iteration iteration, Action onUpdateAction,
            RelationshipMatrixPluginSettings settings, SourceConfiguration source) : this(session, iteration, onUpdateAction, settings)
        {
            this.IncludeSubcategories = source.IncludeSubcategories;

            this.SelectedBooleanOperatorKind = source.SelectedBooleanOperatorKind;
            this.SelectedClassKind = source.SelectedClassKind;
            this.SelectedDisplayKind = source.SelectedDisplayKind;
            this.selectedSortOrder = source.SortOrder;

            var categories = new List<Category>();

            foreach (var sourceSelectedCategoryIid in source.SelectedCategories)
            {
                var category = this.PossibleCategories.FirstOrDefault(c => c.Iid == sourceSelectedCategoryIid);

                if (category != null)
                {
                    categories.Add(category);
                }
            }

            this.SelectedCategories = new List<Category>(categories);
        }

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
        /// Gets the possible <see cref="CategoryBooleanOperatorKind"/> that may be used to filter the categpries.
        /// </summary>
        public List<CategoryBooleanOperatorKind> PossibleBooleanOperatorKinds => this.possibleBooleanOperatorKinds;

        /// <summary>
        /// Gets or sets the selected <see cref="CategoryBooleanOperatorKind"/>
        /// </summary>
        public CategoryBooleanOperatorKind SelectedBooleanOperatorKind
        {
            get { return this._selectedBooleanOperatorKind; }
            set { this.RaiseAndSetIfChanged(ref this._selectedBooleanOperatorKind, value); }
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
    }
}