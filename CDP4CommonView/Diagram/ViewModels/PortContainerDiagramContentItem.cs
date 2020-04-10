namespace CDP4CommonView.Diagram.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using CDP4Common.CommonData;

    using CDP4CommonView.Diagram.Views;

    using CDP4Composition.Diagram;

    using DevExpress.Data.Helpers;
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

        public ReactiveList<IDiagramPortViewModel> PortCollection { get; private set; }

        public PortContainerDiagramContentItem(Thing thing) : base(thing)
        {
            this.PortCollection = new ReactiveList<IDiagramPortViewModel>();
            //this.PortCollection.ItemsAdded.Subscribe(this.SetItemPosition);
            this.PortCollection.Changed.Subscribe(this.PortCollectionChanged);
        }

        private void PortCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChanged)
        {
            // set sides for any
            if (notifyCollectionChanged.NewItems != null)
            { 
                foreach (IDiagramPortViewModel port in notifyCollectionChanged.NewItems)
                {
                    port.PortContainerShapeSide = this.GetAvailableSide();
                }
            }
           
            this.RecalculatePortsPosition();
        }

        private void RecalculatePortsPosition()
        {
            var diagramItem = (this.Parent as DiagramItem);

            var bottomSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Bottom).ToArray();
            var portion = this.CalculatePortion(PortContainerShapeSide.Bottom);

            for (var index = 0; index < bottomSide.Count(); index++)
            {
                var vector = portion * (index + 1) - 10;
                bottomSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(vector, diagramItem.ActualHeight - (10)));
                bottomSide[index].WhenPositionIsUpdatedInvoke();
            }

            var leftSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Left).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Left);

            for (var index = 0; index < leftSide.Count(); index++)
            {
                var vector = portion * (index + 1) - 10;
                leftSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(0 - (10), vector));
                leftSide[index].WhenPositionIsUpdatedInvoke();
            }

            var topSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Top).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Top);

            for (var index = 0; index < topSide.Count(); index++)
            {
                var vector = portion * (index + 1) - 10;
                topSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(vector, 0 - (10)));
                topSide[index].WhenPositionIsUpdatedInvoke();
            }

            var rightSide = this.PortCollection.Where(p => p.PortContainerShapeSide == PortContainerShapeSide.Right).ToArray();
            portion = this.CalculatePortion(PortContainerShapeSide.Right);

            for (var index = 0; index < rightSide.Count(); index++)
            {
                var vector = portion * (index + 1) - 10;
                rightSide[index].Position = System.Windows.Point.Add(diagramItem.Position, new Vector(diagramItem.ActualWidth - (10), vector));
                rightSide[index].WhenPositionIsUpdatedInvoke();
            }
        }

        private PortContainerShapeSide GetAvailableSide()
        {
            var split = ((double)this.PortCollection.Count(p => p.PortContainerShapeSide > PortContainerShapeSide.Undefined)) / 4;
            var place = Math.Abs(split - Math.Truncate(split)) * 100;
            return (PortContainerShapeSide)place;
        }
        private double CalculatePortion(PortContainerShapeSide side)
        {
            var presentPort = (double)this.PortCollection.Count(p => p.PortContainerShapeSide == side);
            var sideLength = side == PortContainerShapeSide.Left || side == PortContainerShapeSide.Right ? (this.Parent as DiagramItem).ActualHeight : (this.Parent as DiagramItem).ActualWidth;
            var portion = ((100 / (presentPort + 1)) / 100) * sideLength;
            return portion;
        }

        private void SetItemPosition(IDiagramPortViewModel viewModel)
        {
            if (this.PortCollection.LastOrDefault() is IDiagramPortViewModel lastIn)
            {
                this.SetNextAvailablePosition(lastIn);
            }
        }

        public void SetNextAvailablePosition(IDiagramPortViewModel lastIn)
        {
            //Determine what side should the lastIn port drawn on
            var split = ((double)this.PortCollection.Count - 1) / 4;
            var place = Math.Abs(split - Math.Truncate(split));

            var diagramItem = (this.Parent as DiagramItem);
            if (place == 0)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Bottom;
                var xVector = this.CalculateVector(PortContainerShapeSide.Bottom, diagramItem.ActualWidth);
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(xVector, diagramItem.ActualHeight - (10)));
            }
            else if (place == .25)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Left;
                var yVector = this.CalculateVector(PortContainerShapeSide.Left, diagramItem.ActualHeight);
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(0 - (10), yVector));
            }
            else if (place == .50)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Top;
                var xVector = this.CalculateVector(PortContainerShapeSide.Top, diagramItem.ActualWidth);
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(xVector, 0 - (10)));
            }
            else if (place == .75)
            {
                lastIn.PortContainerShapeSide = PortContainerShapeSide.Right;
                var yVector = this.CalculateVector(PortContainerShapeSide.Right, diagramItem.ActualHeight);
                lastIn.Position = System.Windows.Point.Add(lastIn.Position, new Vector(diagramItem.ActualWidth - (10), yVector));
            }
            
        }

        private double CalculateVector(PortContainerShapeSide side, double sideLength)
        {
            var presentPort = (double)this.PortCollection.Count(p => p.PortContainerShapeSide == side) - 1;
            var portion = ((20 - presentPort) / 100) * sideLength;
            return ((presentPort + 1) * portion) - (10);
        }

        public void UpdatePortLayout()
        {
            this.RecalculatePortsPosition();
        }
    }
}
