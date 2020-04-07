namespace CDP4DiagramEditor.Views.PortsAndInterfaces
{
    using System.Windows;

    using CDP4Common.CommonData;

    using CDP4CommonView.Diagram;

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
