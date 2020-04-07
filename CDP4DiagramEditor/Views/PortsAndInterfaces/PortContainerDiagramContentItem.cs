namespace CDP4DiagramEditor.Views.PortsAndInterfaces
{
    using System;
    using System.Windows.Media;

    using CDP4Common.CommonData;

    using CDP4Composition.Diagram;

    using NLog;

    using ReactiveUI;

    class PortContainerDiagramContentItem : NamedThingDiagramContentItem
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public PortContainerDiagramContentItem(Thing thing) : base(thing)
        {
            this.PortShapeCollection = new ReactiveList<DiagramPortShape>();
            this.PortShapeCollection.ItemsAdded.Subscribe(this.DistributePortsArroundTheShape);
            this.LeftPortShapeCollection = new ReactiveList<DiagramPortShape>();
            this.TopPortShapeCollection = new ReactiveList<DiagramPortShape>();
            this.RightPortShapeCollection = new ReactiveList<DiagramPortShape>();
            this.BottomPortShapeCollection = new ReactiveList<DiagramPortShape>();
        }

        private void DistributePortsArroundTheShape(DiagramPortShape portShape)
        {
            if (this.LeftPortShapeCollection.Count > 3)
            {
                if (this.TopPortShapeCollection.Count > 3)
                {
                    if (this.RightPortShapeCollection.Count > 3)
                    {
                        if (this.BottomPortShapeCollection.Count > 3)
                        {
                            Logger.Info("No more room for another port");
                        }
                        else
                        {
                            this.BottomPortShapeCollection.Add(portShape);
                        }
                    }
                    else
                    {
                        portShape.LayoutTransform = new RotateTransform(-90);
                        this.RightPortShapeCollection.Add(portShape);
                    }
                }
                else
                {
                    portShape.LayoutTransform = new RotateTransform(180);
                    this.TopPortShapeCollection.Add(portShape);
                }
            }
            else
            {
                portShape.LayoutTransform = new RotateTransform(90);
                this.LeftPortShapeCollection.Add(portShape);
            }
        }

        public ReactiveList<DiagramPortShape> PortShapeCollection { get; set; }

        public ReactiveList<DiagramPortShape> LeftPortShapeCollection { get; private set; }
        public ReactiveList<DiagramPortShape> TopPortShapeCollection { get; private set; }
        public ReactiveList<DiagramPortShape> RightPortShapeCollection { get; private set; }
        public ReactiveList<DiagramPortShape> BottomPortShapeCollection { get; private set; }
    }
}
