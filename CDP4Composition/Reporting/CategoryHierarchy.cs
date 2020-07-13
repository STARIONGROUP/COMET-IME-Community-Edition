using System;
using System.Collections.Generic;
using System.Linq;

using CDP4Common.EngineeringModelData;
using CDP4Common.SiteDirectoryData;

namespace CDP4Composition.Reporting
{
    public class CategoryHierarchy
    {
        private CategoryHierarchy(Category category, int groupLevel)
        {
            this.Category = category;
            this.GroupLevel = groupLevel;
            this.ChildCategoryHierarchies = new List<CategoryHierarchy>();
        }

        public int GroupLevel { get; }

        public Category Category { get; }

        public List<CategoryHierarchy> ChildCategoryHierarchies { get; }

        public string GroupFooterText { get; private set; }

        public bool IsGroupFooterVisible { get; private set; }

        public CategoryHierarchy WithGroupFooterText(string groupFooterText)
        {
            this.GroupFooterText = groupFooterText;
            this.IsGroupFooterVisible = true;
            return this;
        }

        public CategoryHierarchy AddChildCategory(Category category)
        {
            if (this.ChildCategoryHierarchies.Any(x => x.Category == category))
            {
                throw new ArgumentException($"{category.Name} was already added to the list");
            }

            var newCategoryHierarchy = new CategoryHierarchy(category, this.ChildCategoryHierarchies.Count + 1);
            this.ChildCategoryHierarchies.Add(newCategoryHierarchy);
            return newCategoryHierarchy;
        }

        public void FillDefinitionUsages<T>(ElementBase elementBase, DefinitionUsage<T> definitionUsage) where T : class, IReportViewModel<T>
        {
            ElementDefinition elementDefinition = null;
            ElementUsage elementUsage = null;

            if (elementBase is ElementDefinition definition)
            {
                elementDefinition = definition;
            }

            if (elementBase is ElementUsage usage)
            {
                elementDefinition = usage.ElementDefinition;
                elementUsage = usage;
            }

            if (elementDefinition == null)
            {
                return;
            }

            bool isCategoryFound;

            if (elementUsage?.Category?.Any() ?? false)
            {
                isCategoryFound = elementUsage.Category.Contains(this.Category);
            }
            else
            {
                isCategoryFound = elementDefinition.Category.Contains(this.Category);
            }

            if (isCategoryFound)
            {
                var childDefinitionUsage = new DefinitionUsage<T>(this, elementDefinition, elementUsage, definitionUsage.DefinitionUsages.Count + 1);
                definitionUsage.DefinitionUsages.Add(childDefinitionUsage);

                foreach (var childUsage in elementDefinition.ContainedElement)
                {
                    foreach (var childCategoryHierarchy in this.ChildCategoryHierarchies)
                    {
                        childCategoryHierarchy.FillDefinitionUsages(childUsage, childDefinitionUsage);
                    }
                }
            }
            else if (elementDefinition.Category.Contains(this.Category))
            {
                var childDefinitionUsage = new DefinitionUsage<T>(this, elementDefinition, elementUsage, definitionUsage.DefinitionUsages.Count + 1);
                definitionUsage.DefinitionUsages.Add(childDefinitionUsage);

                foreach (var childUsage in elementDefinition.ContainedElement)
                {
                    foreach (var childCategoryHierarchy in this.ChildCategoryHierarchies)
                    {
                        childCategoryHierarchy.FillDefinitionUsages(childUsage, childDefinitionUsage);
                    }
                }
            }
            else
            {
                var childDefinitionUsage = new DefinitionUsage<T>(this, null, null, definitionUsage.DefinitionUsages.Count + 1);

                foreach (var childCategoryHierarchy in this.ChildCategoryHierarchies)
                {
                    childCategoryHierarchy.FillDefinitionUsages(elementUsage, childDefinitionUsage);
                }

                if (childDefinitionUsage.DefinitionUsages.Any())
                {
                    definitionUsage.DefinitionUsages.Add(childDefinitionUsage);
                }
            }
        }

        public static Category GetCategoryByShortName(Iteration iteration, string shortName)
        {
            return iteration.Cache
                .Select(x => x.Value.Value)
                .OfType<Category>()
                .Single(x => x.ShortName == shortName);
        }

        public static CategoryHierarchy CreateTopLevelCategoryHierarchy(Category category)
        {
            return new CategoryHierarchy(category, 0);
        }
    }
}
