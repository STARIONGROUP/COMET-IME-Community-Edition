namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;

    using System.Collections.Generic;

    public class ReportingDataSourceClass<T> where T : ReportingDataSourceRowRepresentation, new()
    {
        private readonly ReportingDataSourceRow<T> topRow;

        public ReportingDataSourceClass(Iteration iteration, CategoryHierarchy categoryHierarchy)
        {
            this.topRow = new ReportingDataSourceRow<T>(iteration.TopElement, categoryHierarchy);
        }

        public List<T> GetTabularRepresentation()
        {
            return this.topRow.GetTabularRepresentation();
        }
    }
}
