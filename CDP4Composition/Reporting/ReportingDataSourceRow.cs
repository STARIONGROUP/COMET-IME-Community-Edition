namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ReportingDataSourceRow
    {
        #region Hierarchy

        private readonly ReportingDataSourceRow parent;

        public List<ReportingDataSourceRow> Children { get; } = new List<ReportingDataSourceRow>();

        #endregion

        #region Associated Element

        private readonly ElementBase elementBase;

        internal ElementDefinition ElementDefinition
            => (this.elementBase as ElementDefinition) ?? (this.elementBase as ElementUsage)?.ElementDefinition;

        internal ElementUsage ElementUsage
            => this.elementBase as ElementUsage;

        #endregion

        #region Parameters

        private readonly List<ReportingDataSourceParameter> reportedParameters = new List<ReportingDataSourceParameter>();

        #endregion

        public ReportingDataSourceRow(ElementBase elementBase, ReportingDataSourceRow parent = null)
        {
            this.parent = parent;

            this.elementBase = elementBase;

            foreach (var type in ParameterStore.GetInstance().DeclaredParameters.Values)
            {
                var parameter = type
                    .GetConstructor(Type.EmptyTypes)
                    .Invoke(new object[] { }) as ReportingDataSourceParameter;

                // set parameter row from here so no constructor declaration is needed
                typeof(ReportingDataSourceParameter)
                    .GetField("row", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(parameter, this);

                this.InitializeParameter(parameter);
            }

            foreach (var childUsage in this.ElementDefinition.ContainedElement)
            {
                this.Children.Add(new ReportingDataSourceRow(childUsage, this));
            }
        }

        private void InitializeParameter(ReportingDataSourceParameter reportedParameter)
        {
            this.reportedParameters.Add(reportedParameter);

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

        public T GetParameter<T>() where T : ReportingDataSourceParameter
        {
            return this.reportedParameters.First(parameter => parameter is T) as T;
        }

        public List<ReportingDataSourceRow> GetTabularRepresentation()
        {
            var tabularRepresentation = new List<ReportingDataSourceRow>();

            tabularRepresentation.Add(this);

            foreach (var row in this.Children)
            {
                tabularRepresentation.AddRange(row.GetTabularRepresentation());
            }

            return tabularRepresentation;
        }
    }
}
