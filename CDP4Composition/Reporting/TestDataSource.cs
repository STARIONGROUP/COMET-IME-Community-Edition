namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;

    using System.Linq;

    public class TestDataSource : ICDP4ReportingDataSource
    {
        [ParameterTypeShortName("m")]
        public class MassParameter : ReportingDataSourceParameter
        {
            public double? ParseValue()
            {
                if (double.TryParse(this.Value, out var result))
                {
                    return result;
                }

                return null;
            }
        }

        [ParameterTypeShortName("total_mass")]
        public class TotalMassParameter : ReportingDataSourceParameter
        {
            public double? ParseValue()
            {
                var children = this.GetChildren<TotalMassParameter>();

                if (children.Any())
                {
                    return children.Sum(parameter => parameter.ParseValue());
                }

                return this.GetSibling<MassParameter>().ParseValue();
            }
        }

        public ReportingDataSourceClass CreateDataSource(Iteration iteration)
        {
            //var categoryHierarchy = new CategoryHierarchy
            //        .Builder(iteration, "Project")
            //    .AddLevel("module_group")
            //    .AddLevel("SYS")
            //    .AddLevel("Module")
            //    .AddLevel("SS")
            //    .AddLevel("Assembly")
            //    .AddLevel("EQT")
            //    .Build();

            var dataSource = new ReportingDataSourceClass(iteration);
            var rows = dataSource.GetTabularRepresentation();

            return dataSource;
        }
    }
}
