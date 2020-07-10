namespace CDP4Composition.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    public class ReportingDataSourceParameter<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        public static ParameterTypeShortNameAttribute GetParameterAttribute(Type type)
        {
            var attr = Attribute
                .GetCustomAttributes(type)
                .First(attribute => attribute is ParameterTypeShortNameAttribute);

            return attr as ParameterTypeShortNameAttribute;
        }

        // set with reflection to avoid the user-declared constructor having to see it
        private readonly ReportingDataSourceRow<T> row;

        internal readonly string ShortName;

        protected string Value { get; private set; }

        protected ReportingDataSourceParameter()
        {
            this.ShortName = GetParameterAttribute(this.GetType()).ShortName;
        }

        internal void Initialize(ContainerList<ParameterValueSet> valueSet)
        {
            // TODO Options, Finite States, and Array parameter types
            this.Value = valueSet.First().ActualValue.First();
        }

        internal void Initialize(ContainerList<ParameterOverrideValueSet> valueSet)
        {
            // TODO Options, Finite States, and Array parameter types
            this.Value = valueSet.First().ActualValue.First();
        }

        public TP GetSibling<TP>() where TP : ReportingDataSourceParameter<T>
        {
            return this.row.GetParameter<TP>();
        }

        public IEnumerable<TP> GetChildren<TP>() where TP : ReportingDataSourceParameter<T>
        {
            return this.row.Children.Select(child => child.GetParameter<TP>());
        }
    }
}
