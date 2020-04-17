namespace CDP4CommonView.Diagram.Views
{
    using CDP4CommonView.Diagram;
    using CDP4CommonView.Diagram.ViewModels;

    /// <summary>
    /// Interaction logic for DiagramPortShape.xaml
    /// </summary>
    public partial class DiagramPortShape
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramPortShape"/> class.
        /// </summary>
        public DiagramPortShape()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramPortShape"/> class.
        /// </summary>
        /// <param name="datacontext">
        /// The <see cref="IDiagramPortViewModel"/> data-context
        public DiagramPortShape(IDiagramPortViewModel datacontext)
        {
            this.DataContext = datacontext;
            this.InitializeComponent();
        }
    }
}
