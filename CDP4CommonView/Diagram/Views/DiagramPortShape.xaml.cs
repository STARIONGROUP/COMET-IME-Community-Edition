namespace CDP4CommonView.Diagram.Views
{
    using System;
    using System.Collections.Generic;
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
            this.Position = ((IDiagramPortViewModel) this.DataContext).Position;
            switch (datacontext.PortContainerShapeSide)
            {
                case PortContainerShapeSide.Top:
                    this.LayoutTransform = new RotateTransform(180);
                    break;
                case PortContainerShapeSide.Left:
                    this.LayoutTransform = new RotateTransform(90);
                    break;
                case PortContainerShapeSide.Right:
                    this.LayoutTransform = new RotateTransform(-90);
                    break;
                case PortContainerShapeSide.Bottom:
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
    }
}
