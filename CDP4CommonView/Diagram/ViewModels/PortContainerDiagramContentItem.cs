namespace CDP4CommonView.Diagram.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using CDP4Common.CommonData;

    using CDP4CommonView.Diagram.Views;

    using CDP4Composition.Diagram;

    using DevExpress.Xpf.Diagram;

    using NLog;

    using ReactiveUI;

    using Point = CDP4Common.DiagramData.Point;

    public class PortContainerDiagramContentItem : NamedThingDiagramContentItem
    {

        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public PortContainerDiagramContentItem(Thing thing) : base(thing)
        {
            this.PortCollection = new ReactiveList<IDiagramPortViewModel>();
            this.PortCollection.ItemsAdded.Subscribe(this.SetItemPosition);
        }
        
        private void SetItemPosition(IDiagramPortViewModel viewModel)
        {
            if (this.PortCollection.LastOrDefault() is IDiagramPortViewModel lastIn)
            {
                this.SetNextAvailablePosition(lastIn);
            }
        }


        public ReactiveList<IDiagramPortViewModel> PortCollection { get; set; }

        public void SetNextAvailablePosition(IDiagramPortViewModel lastIn)
        {
            var split = ((double)this.PortCollection.Count - 1) / 4;
            var place = Math.Abs(split - Math.Truncate(split));
            var diagramItem = (this.Parent as DiagramItem);
            if (place == 0)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Bottom;
                var xVector = this.CalculateVector(PortContainerShapeSide.Bottom, diagramItem.ActualWidth);
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(xVector, diagramItem.ActualHeight - (12.5)));
            }
            else if (place == .25)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Left;
                var yVector = this.CalculateVector(PortContainerShapeSide.Left, diagramItem.ActualHeight);
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(0 - (12.5), yVector));
            }
            else if (place == .50)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Top;
                var xVector = this.CalculateVector(PortContainerShapeSide.Top, diagramItem.ActualWidth);
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(xVector, 0 - (12.5)));
            }
            else if (place == .75)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Right;
                var yVector = this.CalculateVector(PortContainerShapeSide.Right, diagramItem.ActualHeight);
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(diagramItem.ActualWidth - (12.5), yVector));
            }
            
        }

        private double CalculateVector(PortContainerShapeSide side, double sideLength)
        {
            var presentPort = (double)this.PortCollection.Count(p => p.PortContainerShapeSide == side) - 1;
            var portion = ((20 - presentPort) / 100) * sideLength;
            return ((presentPort + 1) * portion) - (12.5);
        }
    }
}
