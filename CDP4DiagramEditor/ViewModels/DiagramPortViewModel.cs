using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDP4DiagramEditor.ViewModels
{
    using System.Windows;

    using CDP4Common.DiagramData;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.ViewModels;

    using CDP4Dal;

    public class DiagramPortViewModel : DiagramObjectViewModel, IDiagramPortViewModel
    {
        public event EventHandler WhenPositionIsUpdated;

        public DiagramPortViewModel(DiagramObject diagramObject, ISession session, DiagramEditorViewModel containerViewModel) : base(diagramObject, session, containerViewModel)
        {
            this.ContainerBounds = diagramObject.Bounds.FirstOrDefault();
            this.Position = new System.Windows.Point(this.ContainerBounds.X, this.ContainerBounds.Y);
        }

        public Bounds ContainerBounds { get; set; }

        public void WhenPositionIsUpdatedInvoke()
        {
            this.WhenPositionIsUpdated?.Invoke(this, null);
        }

        public PortContainerShapeSide PortContainerShapeSide { get; set; }
    }
}
