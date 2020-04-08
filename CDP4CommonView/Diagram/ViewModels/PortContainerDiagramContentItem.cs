namespace CDP4CommonView.Diagram.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using CDP4Common.CommonData;

    using CDP4CommonView.Diagram.Views;

    using CDP4Composition.Diagram;

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
            double split = ((double)this.PortCollection.Count - 1) / 4;
            var place = Math.Abs(split - Math.Truncate(split));
            if (place == 0)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Bottom;
                var presentPort = this.PortCollection.Count(p => p.PortContainerShapeSide == PortContainerShapeSide.Bottom) - 1;
                var portion = (20 - presentPort) / 100 * this.RenderSize.Width;
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(((presentPort + 1)  * portion)- (12.5), this.RenderSize.Height - (12.5)));
            }
            else if (place == .25)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Left;
                var presentPort = this.PortCollection.Count(p => p.PortContainerShapeSide == PortContainerShapeSide.Left) - 1;
                var portion = (20 - presentPort) / 100 * this.RenderSize.Height;
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(0 - (12.5), ((presentPort + 1) * portion) - (12.5)));
            }
            else if (place == .50)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Top;
                var presentPort = this.PortCollection.Count(p => p.PortContainerShapeSide == PortContainerShapeSide.Top) - 1;
                var portion = (20 - presentPort) / 100 * this.RenderSize.Width;
                this.Position = System.Windows.Point.Add(this.Position, new Vector(((presentPort + 1) * portion) - (12.5), 0 - (12.5)));
            }
            else if (place == .75)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Right;
                var presentPort = this.PortCollection.Count(p => p.PortContainerShapeSide == PortContainerShapeSide.Left) - 1;
                var portion = (20 - presentPort) / 100 * this.RenderSize.Height;
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(this.RenderSize.Width - (12.5), ((presentPort + 1) * portion) - (12.5)));
            }
            
        }
    }
}
