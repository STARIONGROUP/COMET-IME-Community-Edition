// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.Dialogs.Rows
{
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    /// <summary>
    ///  row-view-model used to represent a <see cref="Category"/>
    /// </summary>
    public class CategoryRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRowViewModel"/> class.
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/> that is represented by the current row-view-model
        /// </param>
        /// <param name="categorizableThing">
        ///  The <see cref="CategoryRowViewModel"/> that is a member of subject the <see cref="Category"/>.
        /// </param>
        public CategoryRowViewModel(Category category, ICategorizableThing categorizableThing)
        {
            this.Category = category;
            this.SuperCategories = (category.SuperCategory.Count == 0) ? string.Empty : string.Join(", ", category.SuperCategory.Select(x => x.ShortName));
            this.ContainerRdl = category.Container is ReferenceDataLibrary container ? container.ShortName : string.Empty;
            this.Level = categorizableThing is ElementDefinition ? "ED" : "EU";
        }

        /// <summary>
        /// The <see cref="Category"/> represented by the current row-view-model
        /// </summary>
        public Category Category { get; private set; }

        /// <summary>
        /// Gets the name of the <see cref="Categery"/>
        /// </summary>
        public string Name => this.Category.Name;

        /// <summary>
        /// Gets the shortname of the <see cref="Categery"/>
        /// </summary>
        public string ShortName => this.Category.ShortName;

        /// <summary>
        /// Gets the shortnames of the super categories of the <see cref="Categery"/> 
        /// </summary>
        public string SuperCategories { get; private set; }

        /// <summary>
        /// Gets the shortname of the container <see cref="ReferenceDataLibrary"/>
        /// </summary>
        public string ContainerRdl { get; private set; }

        /// <summary>
        /// Gets a string that represents wether the cattegory has been applied to an <see cref="ElementDefinition"/> 
        /// or an <see cref="ElementUsage"/>
        /// </summary>
        public string Level { get; private set; }
    }
}