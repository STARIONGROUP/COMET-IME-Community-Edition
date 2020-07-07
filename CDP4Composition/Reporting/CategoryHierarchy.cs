namespace CDP4Composition.Reporting
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.EngineeringModelData;

    using System.Linq;

    public class CategoryHierarchy
    {
        internal class Builder
        {
            private Iteration iteration;

            private CategoryHierarchy top, current;

            internal Builder(Iteration iteration, string topLevelCategoryShortName)
            {
                this.iteration = iteration;
                this.AddLevel(topLevelCategoryShortName);
            }

            private Category GetCategoryByShortName(string shortName)
            {
                return this.iteration.Cache
                    .Select(x => x.Value.Value)
                    .OfType<Category>()
                    .Single(x => x.ShortName == shortName);
            }

            public Builder AddLevel(string categoryShortName)
            {
                var category = this.GetCategoryByShortName(categoryShortName);

                var newCategoryHierarchy = new CategoryHierarchy(category, this.current);

                if (this.current == null)
                {
                    this.top = this.current = newCategoryHierarchy;
                }
                else
                {
                    this.current.Child = newCategoryHierarchy;
                    this.current = this.current.Child;
                }

                return this;
            }

            public CategoryHierarchy Build()
            {
                return this.top;
            }
        }

        #region Hierarchy

        private CategoryHierarchy Parent;

        public CategoryHierarchy Child { get; private set; }

        #endregion

        public Category Category { get; }

        private CategoryHierarchy(Category category, CategoryHierarchy parent)
        {
            this.Category = category;

            this.Parent = parent;
        }
    }
}
