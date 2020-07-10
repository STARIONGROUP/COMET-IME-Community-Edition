namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;

    using System.Linq;

    [ParameterTypeShortName("m")]
    public class MassParameter : ReportingDataSourceParameter<RowRepresentation>
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
    public class TotalMassParameter : ReportingDataSourceParameter<RowRepresentation>
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

    public class RowRepresentation : ReportingDataSourceRowRepresentation
    {
        public readonly MassParameter mass;
        public readonly TotalMassParameter totalMass;
    }

    public class TestDataSource : ICDP4ReportingDataSource<RowRepresentation>
    {
        public ReportingDataSourceClass<RowRepresentation> CreateDataSource(Iteration iteration)
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

            var categoryHierarchy = new CategoryHierarchy
                    .Builder(iteration, "SYS")
                .Build();

            var dataSource = new ReportingDataSourceClass<RowRepresentation>(
                iteration,
                categoryHierarchy);

            var rows = dataSource.GetTabularRepresentation();

            return dataSource;
        }
    }
}
