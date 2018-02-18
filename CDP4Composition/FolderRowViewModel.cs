// -------------------------------------------------------------------------------------------------
// <copyright file="FolderRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using CDP4Common.CommonData;
    using CDP4Common;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// Used by the TreeListControl through hierarchyDataTemplate to arrange rows of the same type in a folder
    /// </summary>
    public class FolderRowViewModel : RowViewModelBase<NotThing>, IDeprecatableThing
    {
        /// <summary>
        /// Backing field for the <see cref="IsDeprecated"/> property
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderRowViewModel"/> class
        /// </summary>
        /// <param name="shortname">The short-name for this folder</param>
        /// <param name="name">The Name of the folder</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The view-model that contains this row</param>
        public FolderRowViewModel(string shortname, string name, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(new NotThing(name), session, containerViewModel)
        {
            this.ShortName = shortname;
            this.Name = name;
            this.Description = string.Empty;
        }

        /// <summary>
        /// Gets the short-name of the <see cref="FolderRowViewModel"/> that is represented by the current row-view-model
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// Gets the name of the <see cref="FolderRowViewModel"/> that is represented by the current row-view-model
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets description of the <see cref="FolderRowViewModel"/> that is represented by the current row-view-model
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whehter the row is deprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
        }
    }
}
