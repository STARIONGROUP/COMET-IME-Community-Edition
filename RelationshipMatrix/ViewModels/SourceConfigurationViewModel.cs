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
        /// Backing field for <see cref="SelectedClassKind"/>
        /// </summary>
        private ClassKind? selectedClassKind;

        /// <summary>
        /// Backing field for <see cref="SelectedDisplay"/>
        /// </summary>
        private DisplayKind selectedDisplayKind;

        /// <summary>
        /// Backing field for <see cref="SelectedCategories"/>
        /// </summary>
        private List<Category> selectedCategories;

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

            this.WhenAnyValue(x => x.SelectedClassKind).Skip(1).Subscribe(_ =>
            {
                this.PopulatePossibleCategories();
                this.OnUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedDisplayKind).Skip(1).Subscribe(_ =>
            {
                this.OnUpdateAction();
            });

            this.WhenAnyValue(x => x.SelectedCategories).Skip(1).Subscribe(_ => this.OnUpdateAction());

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