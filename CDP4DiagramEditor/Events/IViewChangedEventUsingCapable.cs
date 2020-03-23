using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDP4DiagramEditor.Events
{
    public interface IViewChangedEventUsingCapable
    {
        ViewChangedEvent ViewChangedEvent { get; }
    }
}
