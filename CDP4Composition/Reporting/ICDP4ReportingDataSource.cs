namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;

    public interface ICDP4ReportingDataSource
    {
        ReportingDataSourceClass CreateDataSource(Iteration interation);
    }
}
