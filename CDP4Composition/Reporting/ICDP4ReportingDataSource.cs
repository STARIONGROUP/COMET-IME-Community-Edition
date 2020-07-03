using CDP4Common.EngineeringModelData;

namespace CDP4Composition.Reporting
{
    public interface ICDP4ReportingDataSource
    {
        ReportingDataSourceClass CreateDataSource(Iteration interation);
    }
}
