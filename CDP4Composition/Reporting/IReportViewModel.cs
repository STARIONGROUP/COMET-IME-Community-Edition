using System.Collections.Generic;

namespace CDP4Composition.Reporting
{
    public interface IReportViewModel<T> where T : IReportViewModel<T>
    {
        T Parent { get; set; }

        List<T> Children { get; set; }

        string ReportOrder { get; set; }

        T GetEmptyOrderedRow();
    }
}
