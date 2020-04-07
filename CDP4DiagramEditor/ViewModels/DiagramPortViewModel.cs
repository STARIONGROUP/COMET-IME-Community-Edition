using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDP4DiagramEditor.ViewModels
{
    using CDP4Common.DiagramData;

    using CDP4CommonView.Diagram;

    using CDP4Dal;

    public class DiagramPortViewModel : DiagramObjectViewModel, IDiagramPortViewModel
    {
        public DiagramPortViewModel(DiagramObject diagramObject, ISession session, DiagramEditorViewModel containerViewModel) : base(diagramObject, session, containerViewModel)
        {
        }
    }
}
