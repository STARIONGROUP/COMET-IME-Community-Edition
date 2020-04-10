namespace CDP4CommonView.Diagram.Behaviors
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4CommonView.Diagram.ViewModels;
    using CDP4CommonView.Diagram.Views;

    using DevExpress.Diagram.Core;
    using DevExpress.Mvvm.UI.Interactivity;

    using ReactiveUI;

    public class DiagramPortShapeBehavior : Behavior<DiagramPortShape>
    {
        public ReactiveCommand<object> PositionCommand { get; set; }

        public DiagramPortShapeBehavior()
        {
            
        }

        private void PositionCommandExecute()
        {
            this.AssociatedObject.Position = (this.AssociatedObject.DataContext as IDiagramPortViewModel).Position;
        }

        public Point Position { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();

            (this.AssociatedObject.DataContext as IDiagramPortViewModel).WhenPositionIsUpdated += this.WhenPositionIsUpdated;
            this.DeterminePortConnectorRotation();

            //this.WhenAnyValue(x => ((IDiagramPortViewModel)x.AssociatedObject.DataContext).Position).Subscribe(_ => this.PositionCommandExecute());
            //STore
        }

        private void WhenPositionIsUpdated(object sender, EventArgs e)
        {
            this.AssociatedObject.Position = (this.AssociatedObject.DataContext as IDiagramPortViewModel).Position;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.PositionCommand.Dispose();
            (this.AssociatedObject.DataContext as IDiagramPortViewModel).WhenPositionIsUpdated -= this.WhenPositionIsUpdated;
        }

        public void DeterminePortConnectorRotation()
        {
            var datacontext = ((IDiagramPortViewModel)this.AssociatedObject.DataContext);
            this.AssociatedObject.Position = datacontext.Position;
            switch (datacontext.PortContainerShapeSide)
            {
                case PortContainerShapeSide.Top:
                    this.AssociatedObject.ConnectionPoints = new DiagramPointCollection(new[] { new Point(0.5, 0) });
                    break;
                case PortContainerShapeSide.Left:
                    this.AssociatedObject.ConnectionPoints = new DiagramPointCollection(new[] { new Point(0, 0.5) });
                    break;
                case PortContainerShapeSide.Right:
                    this.AssociatedObject.ConnectionPoints = new DiagramPointCollection(new[] { new Point(1, 0.5) });
                    break;
                case PortContainerShapeSide.Bottom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
