using CDP4Common.EngineeringModelData;

using System.Collections.Generic;

namespace CDP4Composition.Reporting
{
    public class ReportingDataSourceClass
    {
        private readonly ReportingDataSourceRow topRow;

        public ReportingDataSourceClass(Iteration iteration)
        {
            this.topRow = new ReportingDataSourceRow(iteration.TopElement);
        }

        public List<ReportingDataSourceRow> GetTabularRepresentation()
        {
            return this.topRow.GetTabularRepresentation();
        }
    }
}
