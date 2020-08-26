// -------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramContentItem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    
    using CDP4Common.CommonData;
    
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Represents a diagram content control class
    /// </summary>
    public class Cdp4DiagramContentItem : DiagramShape
    {
        /// <summary>
        /// The <see cref="Cdp4DiagramOrgChartBehavior"/> that manages the creation of the views
        /// </summary>
        private readonly Cdp4DiagramOrgChartBehavior behaviour;

        /// <summary>
        /// The dependency property that allows setting the content object
        /// </summary>
        public static readonly DependencyProperty ContentObjectProperty = DependencyProperty.Register("ContentObject", typeof(Thing), typeof(Cdp4DiagramContentItem), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="Cdp4DiagramContentItem"/> class.
        /// </summary>
        /// <param name="datacontext">
        /// The <see cref="IDiagramObjectViewModel"/> data-context
        /// </param>
        /// <param name="behaviour">The <see cref="Cdp4DiagramOrgChartBehavior"/></param>
        public Cdp4DiagramContentItem(IDiagramObjectViewModel datacontext, Cdp4DiagramOrgChartBehavior behaviour)
        {
            this.DataContext = datacontext;
            this.behaviour = behaviour;
            this.Initialize();
        }

        /// <summary>
        /// Gets or sets the content-object
        /// </summary>
        public Thing ContentObject
        {
            get { return (Thing)this.GetValue(ContentObjectProperty); }
            set { this.SetValue(ContentObjectProperty, value); }
        }

        /// <summary>
        /// Initializes the views and set the bindings
        /// </summary>
        private void Initialize()
        {
            var heightBinding = ViewUtils.CreateBinding(this.DataContext, "Height");
            BindingOperations.SetBinding(this, HeightProperty, heightBinding);

            var widthBinding = ViewUtils.CreateBinding(this.DataContext, "Width");
            BindingOperations.SetBinding(this, WidthProperty, widthBinding);

            var xPosBinding = ViewUtils.CreateBinding(this.DataContext, "Position");
            BindingOperations.SetBinding(this, PositionProperty, xPosBinding);

            var contentBinding = ViewUtils.CreateBinding(this.DataContext, "Thing.DepictedThing", BindingMode.OneWay);
            BindingOperations.SetBinding(this, ContentObjectProperty, contentBinding);

            this.Shape = Cdp4DiagramHelper.GetShape(((IDiagramObjectViewModel)this.DataContext).Thing.DepictedThing.ClassKind);
            this.Content = this.ContentObject.UserFriendlyShortName;

            var parent = (Cdp4DiagramControl)this.behaviour.AssociatedObject;
            this.Template = (ControlTemplate)parent.FindResource("DiagramObjectTemplate");
        }
    }
}