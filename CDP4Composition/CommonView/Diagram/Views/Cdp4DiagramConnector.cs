// -------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramConnector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using CDP4Composition.Diagram;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Represents a diagram connector control
    /// </summary>
    public class Cdp4DiagramConnector : DiagramConnector
    {
        #region DependencyProperties
        /// <summary>
        /// The dependency property that allows setting the source to the view-model representing a diagram object
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(Cdp4DiagramConnector), new FrameworkPropertyMetadata(DiagramSourceChanged));

        /// <summary>
        /// The dependency property that allows setting the source to the view-model representing a diagram object
        /// </summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(object), typeof(Cdp4DiagramConnector), new FrameworkPropertyMetadata(DiagramTargetChanged));


        /// <summary>
        /// Initializes static members of the <see cref="Cdp4DiagramConnector"/> class.
        /// </summary>
        static Cdp4DiagramConnector()
        {
            BeginItemProperty.OverrideMetadata(typeof(Cdp4DiagramConnector), new FrameworkPropertyMetadata(null, OnBeginItemChanged));
            EndItemProperty.OverrideMetadata(typeof(Cdp4DiagramConnector), new FrameworkPropertyMetadata(null, OnEndItemChanged));
        }

        /// <summary>
        /// Called when the <see cref="Target"/> is changed
        /// </summary>
        /// <param name="d">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void DiagramTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var connectorView = (Cdp4DiagramConnector)d;
            var oldSource = e.OldValue;
            var newValue = e.NewValue;

            if (newValue != oldSource)
            {
                connectorView.SetTarget(newValue);
            }
        }

        /// <summary>
        /// Called when the <see cref="Source"/> is changed
        /// </summary>
        /// <param name="d">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void DiagramSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var connectorView = (Cdp4DiagramConnector)d;
            var oldSource = e.OldValue;
            var newValue = e.NewValue;

            if (newValue != oldSource)
            {
                connectorView.SetSource(newValue);
            }
        }

        /// <summary>
        /// Called when the <see cref="Cdp4DiagramConnector.BeginItem"/> is changed
        /// </summary>
        /// <param name="d">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnBeginItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var connectorView = (Cdp4DiagramConnector)d;
            var oldSource = e.OldValue as ThingDiagramContentItem;
            var newSource = e.NewValue as ThingDiagramContentItem;

            if (newSource != null && newSource != oldSource)
            {
                connectorView.Source = ((IDiagramObjectViewModel)newSource.DataContext).Thing;
            }
        }

        /// <summary>
        /// Called when the <see cref="Cdp4DiagramConnector.EndItem"/> is changed
        /// </summary>
        /// <param name="d">The source of the call.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnEndItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var connectorView = (Cdp4DiagramConnector)d;
            var oldSource = e.OldValue as ThingDiagramContentItem;
            var newSource = e.NewValue as ThingDiagramContentItem;

            if (newSource != null && newSource != oldSource)
            {
                connectorView.Target = ((IDiagramObjectViewModel)newSource.DataContext).Thing;
            }
        }

        #endregion


        /// <summary>
        /// The <see cref="Cdp4DiagramOrgChartBehavior"/> that manages the creation of the views
        /// </summary>
        private readonly Cdp4DiagramOrgChartBehavior behaviour;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cdp4DiagramConnector"/> class.
        /// </summary>
        public Cdp4DiagramConnector(IDiagramConnectorViewModel dataContext, Cdp4DiagramOrgChartBehavior behaviour, DiagramConnector baseConnector = null)
        {
            this.DataContext = dataContext;
            this.behaviour = behaviour;
            if (baseConnector != null)
            {
                this.CopyConnectorsettings(baseConnector);
            }
            
            this.Initialize();
        }

        /// <summary>
        /// Gets or sets the <see cref="IDiagramObjectViewModel"/> corresponding to the data-context of the source of the connector
        /// </summary>
        public object Source
        {
            get { return this.GetValue(SourceProperty); }
            set { this.SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="IDiagramObjectViewModel"/> corresponding to the data-context of the target of the connector
        /// </summary>
        public object Target
        {
            get { return this.GetValue(TargetProperty); }
            set { this.SetValue(TargetProperty, value); }
        }
        

        /// <summary>
        /// Set the <see cref="Cdp4DiagramConnector.BeginItem"/> from the view-model
        /// </summary>
        /// <param name="source">The source <see cref="IDiagramObjectViewModel"/></param>
        private void SetSource(object source)
        {
            this.BeginItem = this.GetDiagramContentItemToConnectTo(source);
        }

        /// <summary>
        /// Set the <see cref="Cdp4DiagramConnector.EndItem"/> from the view-model
        /// </summary>
        /// <param name="target">The target <see cref="IDiagramObjectViewModel"/></param>
        private void SetTarget(object target)
        {
            this.EndItem = this.GetDiagramContentItemToConnectTo(target);
        }

        /// <summary>
        /// Get the the target <see cref="DiagramContentItem"/>
        /// to be set either as <see cref="Cdp4DiagramConnector.BeginItem"/> or <see cref="Cdp4DiagramConnector.EndItem"/>
        /// by <see cref="Cdp4DiagramConnector.SetSource"/> or <see cref="Cdp4DiagramConnector.SetTarget"/>
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private DiagramContentItem GetDiagramContentItemToConnectTo(object source)
        {
            var diagramControl = (DiagramControl)this.behaviour.AssociatedObject;
            var diagramObject = diagramControl.Items.OfType<DiagramContentItem>().SingleOrDefault(x => ((NamedThingDiagramContentItem)x.Content).DiagramThing == source);
            if (diagramObject == null)
            {
                throw new InvalidOperationException("DiagramContentItem could not be found.");
            }
            return diagramObject;
        }
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        private void Initialize()
        {
            var sourceBinding = ViewUtils.CreateBinding(this.DataContext, "Source");
            BindingOperations.SetBinding(this, SourceProperty, sourceBinding);

            var targetBinding = ViewUtils.CreateBinding(this.DataContext, "Target");
            BindingOperations.SetBinding(this, TargetProperty, targetBinding);

            var textBinding = ViewUtils.CreateBinding(this.DataContext, "DisplayedText", BindingMode.OneWay);
            BindingOperations.SetBinding(this, ContentProperty, textBinding);

            var connectingPointsBinding = ViewUtils.CreateBinding(this.DataContext, "ConnectingPoints");
            BindingOperations.SetBinding(this, ConnectionPointsProperty, connectingPointsBinding);

            this.CanMove = false;
        }

        /// <summary>
        /// Copies the properties of the drawn connector.
        /// </summary>
        /// <param name="baseConnector">The <see cref="DiagramConnector"/> that this control is based on.</param>
        private void CopyConnectorsettings(DiagramConnector baseConnector)
        {
            this.Anchors = baseConnector.Anchors;
            this.Angle = baseConnector.Angle;
            this.BeginItemPointIndex = baseConnector.BeginItemPointIndex;
            this.BeginPoint = baseConnector.BeginPoint;
            this.ConnectionPoints = baseConnector.ConnectionPoints;
            this.EndItemPointIndex = baseConnector.EndItemPointIndex;
            this.EndPoint = baseConnector.EndPoint;
        }
    }
}