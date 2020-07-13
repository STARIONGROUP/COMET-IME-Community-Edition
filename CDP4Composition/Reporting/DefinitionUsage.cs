using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CDP4Common.EngineeringModelData;
using CDP4Common.SiteDirectoryData;

namespace CDP4Composition.Reporting
{
    public class DefinitionUsage<T> where T : class, IReportViewModel<T>
    {
        public string GroupOrder { get; private set; }

        //Some data in ParameterRow is used for calculations (summaries), but must not be visible in the report.
        //In this case the combination of isCalulated and IsVisible is used to return the ParameterRow in its getter, or to return NULL (not visible in report).
        public bool IsCalculated
        {
            get => this.isCalculated;
            set
            {
                if (value)
                {
                    this.ParameterRow.Children = this.DefinitionUsages.Select(x => x.ParameterRow).ToList();

                    foreach (var child in this.ParameterRow.Children)
                    {
                        child.Parent = this.ParameterRow;
                    }

                    foreach (var usage in this.DefinitionUsages)
                    {
                        usage.IsCalculated = true;
                    }
                }

                this.isCalculated = value;
            }
        }

        private bool IsVisible => (this.ElementUsage as ElementBase ?? this.ElementDefinition) != null;

        public CategoryHierarchy CategoryHierarchy { get; private set; }

        public ElementDefinition ElementDefinition { get; private set; }

        public ElementUsage ElementUsage { get; private set; }

        private Dictionary<string, PropertyInfo> ParameterShortNameList { get; set; }

        public DefinitionUsage(CategoryHierarchy categoryHierarchy, ElementDefinition elementDefinition, ElementUsage elementUsage = null, int groupOrder = 0)
        {
            this.Initialize(categoryHierarchy, elementDefinition, elementUsage, groupOrder);
        }

        private void Initialize(CategoryHierarchy categoryHierarchy, ElementDefinition elementDefinition, ElementUsage elementUsage, int groupOrder)
        {
            this.CategoryHierarchy = categoryHierarchy;
            this.ElementDefinition = elementDefinition;
            this.ElementUsage = elementUsage;
            this.DefinitionUsages = new List<DefinitionUsage<T>>();
            this.ParameterTypeValues = new Dictionary<ParameterType, string>();
            this.GroupOrder = $"{categoryHierarchy.GroupLevel}_{groupOrder}";

            this.ParameterShortNameList = typeof(T)
                .GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(ParameterTypeShortNameAttribute), true).Any())
                .ToDictionary(
                    x => x.GetCustomAttributes(typeof(ParameterTypeShortNameAttribute), true).Cast<ParameterTypeShortNameAttribute>().First().ShortName,
                    x => x);

            this.parameterRow = Activator.CreateInstance<T>();

            this.FillParameterData(elementUsage as ElementBase ?? elementDefinition);
        }

        public List<DefinitionUsage<T>> DefinitionUsages { get; private set; }

        private T emptyParameterRow;
        private T parameterRow;
        private bool isCalculated;

        public T ParameterRow
        {
            get
            {
                if (this.IsCalculated && !this.IsVisible)
                {
                    if (this.emptyParameterRow == null)
                    {
                        this.emptyParameterRow = this.parameterRow.GetEmptyOrderedRow();
                    }

                    return this.emptyParameterRow;
                }
                else
                {
                    return this.parameterRow;
                }
            }
        }

        public Dictionary<ParameterType, string> ParameterTypeValues { get; private set; }

        private void FillParameterData(ElementBase elementBase)
        {
            if (elementBase == null)
            {
                return;
            }

            var dataRowDictionary = new Dictionary<string, object>();
            var iteration = elementBase.TopContainer;
            var parameterTypes = iteration.RequiredRdls.SelectMany(x => x.ParameterType).Where(x => this.ParameterShortNameList.Select(y => y.Key).Contains(x.ShortName)).ToList();

            foreach (var parameterType in parameterTypes)
            {
                var columnName = parameterType.ShortName;
                object value = null;

                var elementDefinition = elementBase as ElementDefinition;
                var elementUsage = elementBase as ElementUsage;

                if (elementUsage != null)
                {
                    elementDefinition = elementUsage.ElementDefinition;
                }

                var parameter = elementDefinition?.Parameter.SingleOrDefault(x => x.ParameterType == parameterType);

                if (parameter != null)
                {
                    //TODO Options, Finite States, and Array parameter types
                    value = parameter.ValueSet.First().ActualValue.First();
                }

                var parameterOverride = elementUsage?.ParameterOverride.SingleOrDefault(x => x.Parameter.ParameterType == parameterType);

                if (parameterOverride != null)
                {
                    //TODO Options, Finite States, and Array parameter types
                    value = parameterOverride.ValueSet.First().ActualValue.First();
                }

                dataRowDictionary.Add(columnName, value);
            }

            foreach (var keyValuePair in this.ParameterShortNameList)
            {
                if (dataRowDictionary.ContainsKey(keyValuePair.Key))
                {
                    keyValuePair.Value.SetValue(this.ParameterRow, dataRowDictionary[keyValuePair.Key]);

                    this.ParameterTypeValues.Add(parameterTypes.Single(x => x.ShortName == keyValuePair.Key), (string)dataRowDictionary[keyValuePair.Key]);
                }
            }
        }
    }
}
