namespace CDP4CommonView.Diagram.Views
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;

    using CDP4Common.DTO;

    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.ViewModels;

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.Native;

    using Bounds = CDP4Common.DiagramData.Bounds;
    using Point = System.Windows.Point;
    using Thing = CDP4Common.CommonData.Thing;

    /// <summary>
    /// Interaction logic for DiagramPortShape.xaml
    /// </summary>
    public partial class DiagramPortShape
    {
        /// <summary>
        /// The <see cref="Cdp4DiagramOrgChartBehavior"/> that manages the creation of the views
        /// </summary>
        private readonly Cdp4DiagramOrgChartBehavior behaviour;

        /// <summary>
        /// The dependency property that allows setting the content object
        /// </summary>
        public static readonly DependencyProperty ContentObjectProperty = DependencyProperty.Register("ContentObject", typeof(Thing), typeof(Cdp4DiagramContentItem), new FrameworkPropertyMetadata(null));

        public DiagramPortShape()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cdp4DiagramContentItem"/> class.
        /// </summary>
        /// <param name="datacontext">
        /// The <see cref="IDiagramObjectViewModel"/> data-context
        /// </param>
        /// <param name="behaviour">The <see cref="Cdp4DiagramOrgChartBehavior"/></param>
        public DiagramPortShape(IDiagramPortViewModel datacontext, Cdp4DiagramOrgChartBehavior behaviour)
        {
            this.DataContext = datacontext;
            this.behaviour = behaviour;
            this.InitializeComponent();
            this.DetermineRotation();
        }

        private void DetermineRotation()
        {
            var datacontext = ((IDiagramPortViewModel) this.DataContext);
            this.Position = new Point(datacontext.ContainerBounds.X, datacontext.ContainerBounds.Y);
            switch (datacontext.PortContainerShapeSide)
            {
                case PortContainerShapeSide.Top:
                    this.Background = Brushes.Blue;
                    this.LayoutTransform = new RotateTransform(180);
                    this.Anchors = Sides.Top;
                    this.Position = Point.Add(this.Position, new Vector(datacontext.ContainerBounds.Width/2 - (this.Width / 2), 0 - (this.Width / 2)));
                    break;
                case PortContainerShapeSide.Left:
                    this.Background = Brushes.BlueViolet;
                    this.LayoutTransform = new RotateTransform(90);
                    this.Position = Point.Add(this.Position, new Vector(0 - (this.Width / 2), datacontext.ContainerBounds.Height / 2 - (this.Width / 2)));
                    break;
                case PortContainerShapeSide.Right:
                    this.Background = Brushes.OrangeRed;
                    this.LayoutTransform = new RotateTransform(-90);
                    this.Position = Point.Add(this.Position, new Vector(datacontext.ContainerBounds.Width - (this.Width / 2), datacontext.ContainerBounds.Height / 2 - (this.Width / 2)));
                    break;
                case PortContainerShapeSide.Bottom:
                    this.Background = Brushes.ForestGreen;
                    this.Position = Point.Add(this.Position, new Vector(datacontext.ContainerBounds.Width / 2 - (this.Width / 2), datacontext.ContainerBounds.Height - (this.Width/2)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets or sets the content-object
        /// </summary>
        public Thing ContentObject
        {
            get => (Thing)this.GetValue(ContentObjectProperty);
            set => this.SetValue(ContentObjectProperty, value);
        }

        public void MoveToTheClosestAllowed(Point desired)
        {
            var bounds = new Point(this.Bounds().X, this.Bounds().Y);
            var viewModel = ((IDiagramPortViewModel) this.DataContext);
            var containerBounds = viewModel.ContainerBounds;

            if (viewModel.PortContainerShapeSide == PortContainerShapeSide.Bottom || viewModel.PortContainerShapeSide == PortContainerShapeSide.Top)
            {
                if (desired.X < containerBounds.X)
                {
                    bounds.X = containerBounds.X;
                }
                else if (desired.X > containerBounds.X && desired.X < containerBounds.X + containerBounds.Width)
                {
                    bounds.X = desired.X;
                }
                else
                {
                    bounds.X = containerBounds.X + containerBounds.Width;
                }
            }
            else
            {
                if (desired.Y < containerBounds.Y)
                {
                    bounds.Y = containerBounds.Y;
                }
                else if (desired.Y > containerBounds.Y && desired.Y < containerBounds.Y + containerBounds.Height)
                {
                    bounds.Y = desired.Y;
                }
                else
                {
                    bounds.Y = containerBounds.Y + containerBounds.Height;
                }
            }

            this.RenderTransform = new TranslateTransform(Math.Abs(bounds.X - this.Bounds().X), Math.Abs(bounds.Y - this.Bounds().Y));
        }
    }
}
