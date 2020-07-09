namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ReportingDataSourceRow<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        #region Hierarchy

        private readonly ReportingDataSourceRow<T> parent;

        public List<ReportingDataSourceRow<T>> Children { get; } = new List<ReportingDataSourceRow<T>>();

        #endregion

        #region Associated Element

        private readonly ElementBase elementBase;

        private ElementDefinition ElementDefinition =>
            (this.elementBase as ElementDefinition) ?? (this.elementBase as ElementUsage)?.ElementDefinition;

        private ElementUsage ElementUsage =>
            this.elementBase as ElementUsage;

        private string FullyQualifiedName => (this.parent != null)
            ? this.parent.FullyQualifiedName + "." + this.ElementUsage.ShortName
            : this.ElementDefinition.ShortName;

        #endregion

        #region Parameters

        private static readonly IEnumerable<FieldInfo> ParameterFields = typeof(T).GetFields()
            .Where(f => f.FieldType.IsSubclassOf(typeof(ReportingDataSourceParameter)));

        private readonly Dictionary<Type, ReportingDataSourceParameter> reportedParameters =
            new Dictionary<Type, ReportingDataSourceParameter>();

        #endregion

        #region Category filtering

        private readonly Category filterCategory;

        private bool IsVisible =>
            this.elementBase.Category.Contains(this.filterCategory);

        private bool IsRelevant =>
            this.IsVisible || this.Children.Any(child => child.IsRelevant);

        #endregion

        public ReportingDataSourceRow(
            ElementBase elementBase,
            CategoryHierarchy categoryHierarchy,
            ReportingDataSourceRow<T> parent = null)
        {
            this.filterCategory = categoryHierarchy.Category;

            this.parent = parent;

            this.elementBase = elementBase;

            foreach (var type in ParameterFields.Select(f => f.FieldType))
            {
                var parameter = type
                    .GetConstructor(Type.EmptyTypes)
                    .Invoke(new object[] { }) as ReportingDataSourceParameter;

                // set parameter row from here so no constructor declaration is needed
                typeof(ReportingDataSourceParameter)
                    .GetField("row", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(parameter, this as ReportingDataSourceRow<ReportingDataSourceRowRepresentation>);

                this.reportedParameters[type] = parameter;

                this.InitializeParameter(parameter);
            }

            if (categoryHierarchy.Child == null)
            {
                return;
            }

            foreach (var childUsage in this.ElementDefinition.ContainedElement)
            {
                var childRow = new ReportingDataSourceRow<T>(childUsage, categoryHierarchy.Child, this);

                if (childRow.IsRelevant)
                {
                    this.Children.Add(childRow);
                }
            }
        }

        private void InitializeParameter(ReportingDataSourceParameter reportedParameter)
        {
            var parameter = this.ElementDefinition.Parameter
                .SingleOrDefault(x => x.ParameterType.ShortName == reportedParameter.ShortName);

            if (parameter != null)
            {
                reportedParameter.Initialize(parameter.ValueSet);
            }

            var parameterOverride = this.ElementUsage?.ParameterOverride
                .SingleOrDefault(x => x.Parameter.ParameterType.ShortName == reportedParameter.ShortName);

            if (parameterOverride != null)
            {
                reportedParameter.Initialize(parameterOverride.ValueSet);
            }
        }

        public TP GetParameter<TP>() where TP : ReportingDataSourceParameter
        {
            return this.reportedParameters[typeof(TP)] as TP;
        }

        public List<T> GetTabularRepresentation()
        {
            var tabularRepresentation = new List<T>();

            tabularRepresentation.Add(this.GetRowTabularRepresentation());

            foreach (var row in this.Children)
            {
                tabularRepresentation.AddRange(row.GetTabularRepresentation());
            }

            return tabularRepresentation;
        }

        private T GetRowTabularRepresentation()
        {
            var row = new T();

            typeof(T).GetField("ElementName")
                .SetValue(row, this.FullyQualifiedName);

            if (!this.IsVisible)
            {
                return row;
            }

            foreach (var field in ParameterFields)
            {
                field.SetValue(row, this.reportedParameters[field.FieldType]);
            }

            return row;
        }
    }
}
