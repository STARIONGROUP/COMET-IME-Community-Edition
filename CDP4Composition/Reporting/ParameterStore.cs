namespace CDP4Composition.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class ParameterStore
    {
        private static ParameterStore instance;

        public static ParameterStore GetInstance()
        {
            return instance ?? (instance = new ParameterStore());
        }

        public readonly Dictionary<string, Type> DeclaredParameters;

        private ParameterStore()
        {
            this.DeclaredParameters = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ReportingDataSourceParameter)))
                .ToDictionary(
                    type => ReportingDataSourceParameter.GetParameterAttribute(type).ShortName,
                    type => type);
        }
    }
}
