using CDP4Common.EngineeringModelData;

namespace CDP4Composition.Reporting
{
    public interface ICDP4ObjectDataSource
    {
        object CreateDataSource(Iteration iteration);
    }
}
