using System;
using System.Collections.Generic;
using System.Linq;

using CDP4Common.EngineeringModelData;
using CDP4Common.Types;

namespace CDP4Composition.Reporting
{
    public class ReportingDataSourceParameter
    {
        public static ParameterTypeShortNameAttribute GetParameterAttribute(Type type)
        {
            var attr = Attribute
                .GetCustomAttributes(type)
                .First(attribute => attribute is ParameterTypeShortNameAttribute);

            return attr as ParameterTypeShortNameAttribute;
        }

        private readonly ReportingDataSourceRow row;

        internal readonly string ShortName;

        internal string Value;

        /// <summary>
        /// Constructors are called auto-magically by the row, this should never be called directly
        /// </summary>
        public ReportingDataSourceParameter()
        {
            throw new NotImplementedException();
        }

        public ReportingDataSourceParameter(ReportingDataSourceRow row)
        {
            this.row = row;

            this.ShortName = ReportingDataSourceParameter
                .GetParameterAttribute(this.GetType()).ShortName;
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

        public T GetSibling<T>() where T : ReportingDataSourceParameter
        {
            return this.row.GetParameter<T>();
        }

        public IEnumerable<T> GetChildren<T>() where T : ReportingDataSourceParameter
        {
            return this.row.Children.Select(child => child.GetParameter<T>());
        }
    }
}
