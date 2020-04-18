// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramContentItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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
            // TODO Set Style
        }
    }
}