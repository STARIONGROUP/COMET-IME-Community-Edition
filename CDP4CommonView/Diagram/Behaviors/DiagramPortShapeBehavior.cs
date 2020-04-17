namespace CDP4CommonView.Diagram.Behaviors
{
    using System;
    using System.Windows;

    using CDP4CommonView.Diagram.ViewModels;
    using CDP4CommonView.Diagram.Views;

    using DevExpress.Diagram.Core;
    using DevExpress.Mvvm.UI.Interactivity;

    using ReactiveUI;

    public class DiagramPortShapeBehavior : Behavior<DiagramPortShape>
    {
        /// <summary>
        /// The command that can fire <see cref="PositionCommandExecute"/>
        /// </summary>
        public ReactiveCommand<object> PositionCommand { get; set; } 

        /// <summary>
        /// The Method the command <see cref="PositionCommand"/> can fire
        /// </summary>
        private void PositionCommandExecute()
        {
            this.AssociatedObject.Position = (this.AssociatedObject.DataContext as IDiagramPortViewModel).Position;
        }

        /// <summary>
        /// Get or set the property holding the associated object position
        /// </summary>
        public Point Position { get; set; }

        /// <summary>
        /// The on Attached event Handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            (this.AssociatedObject.DataContext as IDiagramPortViewModel).WhenPositionIsUpdated += this.WhenPositionIsUpdated;
            this.DeterminePortConnectorRotation();

            //this.WhenAnyValue(x => ((IDiagramPortViewModel)x.AssociatedObject.DataContext).Position).Subscribe(_ => this.PositionCommandExecute());
            //STore
        }

        /// <summary>
        /// Event handler of the event firing when Position property has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WhenPositionIsUpdated(object sender, EventArgs e)
        {
            this.AssociatedObject.Position = (this.AssociatedObject.DataContext as IDiagramPortViewModel).Position;
        }

        /// <summary>
        /// the on detaching event handler
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.PositionCommand.Dispose();
            (this.AssociatedObject.DataContext as IDiagramPortViewModel).WhenPositionIsUpdated -= this.WhenPositionIsUpdated;
        }

        /// <summary>
        /// Method use to set the correction orientation of the associate object connection point based on which side of the container it belongs
        /// </summary>
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
