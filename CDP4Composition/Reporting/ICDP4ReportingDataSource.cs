namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;

    public interface ICDP4ReportingDataSource<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        ReportingDataSourceClass<T> CreateDataSource(Iteration interation);
    }
}
