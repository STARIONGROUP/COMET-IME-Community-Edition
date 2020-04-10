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
        public DiagramPortShape()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramPortShape"/> class.
        /// </summary>
        /// <param name="datacontext">
        /// The <see cref="IDiagramPortViewModel"/> data-context
        /// </param>
        /// <param name="behaviour">The <see cref="Cdp4DiagramOrgChartBehavior"/></param>
        public DiagramPortShape(IDiagramPortViewModel datacontext, Cdp4DiagramOrgChartBehavior behaviour)
        {
            this.DataContext = datacontext;
            this.InitializeComponent();
        }
    }
}
