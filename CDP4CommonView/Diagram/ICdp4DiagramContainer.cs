using DevExpress.Xpf.Diagram;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDP4CommonView.Diagram
{
    public interface ICdp4DiagramContainer
    {
        /// <summary>
        /// Gets or sets the behaviour.
        /// </summary>
        ICdp4DiagramOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// Get or set the <see cref="DiagramItem"/> item that is selected.
        /// </summary>
        DiagramItem SelectedItem { get; set; }

        /// <summary>
        /// Get or set the collection of <see cref="DiagramItem"/> items that are selected.
        /// </summary>
        ReactiveList<DiagramItem> SelectedItems { get; set; }

        bool CanAddPort { get; set; }

        /// <summary>
        /// Removes items provided by the behavior.
        /// </summary>
        /// <param name="oldItems">The list of items to be removed.</param>
        void RemoveItems(IList oldItems);
    }
}
