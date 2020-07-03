using CDP4Common.CommonData;
using CDP4Common.SiteDirectoryData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CDP4Composition.Reporting
{
    public class ParameterStore
    {
        private static ParameterStore instance;

        public static ParameterStore GetInstance(TopContainer topContainer)
        {
            return instance ?? (instance = new ParameterStore(topContainer));
        }

        public readonly Dictionary<string, Type> DeclaredParameters;
        public readonly Dictionary<string, ParameterType> ParameterTypes;

        private ParameterStore(TopContainer topContainer)
        {
            this.DeclaredParameters = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ReportingDataSourceParameter)))
                .ToDictionary(
                    type => ReportingDataSourceParameter.GetParameterAttribute(type).ShortName,
                    type => type);

            this.ParameterTypes = topContainer.RequiredRdls
                .SelectMany(x => x.ParameterType)
                .Where(x => this.DeclaredParameters.ContainsKey(x.ShortName))
                .ToDictionary(
                    x => x.ShortName,
                    x => x);
        }
    }
}
