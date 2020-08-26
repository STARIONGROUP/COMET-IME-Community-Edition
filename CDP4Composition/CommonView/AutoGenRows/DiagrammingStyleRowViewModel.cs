// -------------------------------------------------------------------------------------------------
// <copyright file="DiagrammingStyleRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="DiagrammingStyle"/>
    /// </summary>
    public abstract partial class DiagrammingStyleRowViewModel<T> : DiagramThingBaseRowViewModel<T> where T : DiagrammingStyle
    {

        /// <summary>
        /// Backing field for <see cref="FillOpacity"/>
        /// </summary>
        private float fillOpacity;

        /// <summary>
        /// Backing field for <see cref="StrokeWidth"/>
        /// </summary>
        private float strokeWidth;

        /// <summary>
        /// Backing field for <see cref="StrokeOpacity"/>
        /// </summary>
        private float strokeOpacity;

        /// <summary>
        /// Backing field for <see cref="FontSize"/>
        /// </summary>
        private float fontSize;

        /// <summary>
        /// Backing field for <see cref="FontName"/>
        /// </summary>
        private string fontName;

        /// <summary>
        /// Backing field for <see cref="FontItalic"/>
        /// </summary>
        private bool fontItalic;

        /// <summary>
        /// Backing field for <see cref="FontBold"/>
        /// </summary>
        private bool fontBold;

        /// <summary>
        /// Backing field for <see cref="FontUnderline"/>
        /// </summary>
        private bool fontUnderline;

        /// <summary>
        /// Backing field for <see cref="FontStrokeThrough"/>
        /// </summary>
        private bool fontStrokeThrough;

        /// <summary>
        /// Backing field for <see cref="FillColor"/>
        /// </summary>
        private Color fillColor;

        /// <summary>
        /// Backing field for <see cref="StrokeColor"/>
        /// </summary>
        private Color strokeColor;

        /// <summary>
        /// Backing field for <see cref="FontColor"/>
        /// </summary>
        private Color fontColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagrammingStyleRowViewModel{T}"/> class
        /// </summary>
        /// <param name="diagrammingStyle">The <see cref="DiagrammingStyle"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected DiagrammingStyleRowViewModel(T diagrammingStyle, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagrammingStyle, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the FillOpacity
        /// </summary>
        public float FillOpacity
        {
            get { return this.fillOpacity; }
            set { this.RaiseAndSetIfChanged(ref this.fillOpacity, value); }
        }

        /// <summary>
        /// Gets or sets the StrokeWidth
        /// </summary>
        public float StrokeWidth
        {
            get { return this.strokeWidth; }
            set { this.RaiseAndSetIfChanged(ref this.strokeWidth, value); }
        }

        /// <summary>
        /// Gets or sets the StrokeOpacity
        /// </summary>
        public float StrokeOpacity
        {
            get { return this.strokeOpacity; }
            set { this.RaiseAndSetIfChanged(ref this.strokeOpacity, value); }
        }

        /// <summary>
        /// Gets or sets the FontSize
        /// </summary>
        public float FontSize
        {
            get { return this.fontSize; }
            set { this.RaiseAndSetIfChanged(ref this.fontSize, value); }
        }

        /// <summary>
        /// Gets or sets the FontName
        /// </summary>
        public string FontName
        {
            get { return this.fontName; }
            set { this.RaiseAndSetIfChanged(ref this.fontName, value); }
        }

        /// <summary>
        /// Gets or sets the FontItalic
        /// </summary>
        public bool FontItalic
        {
            get { return this.fontItalic; }
            set { this.RaiseAndSetIfChanged(ref this.fontItalic, value); }
        }

        /// <summary>
        /// Gets or sets the FontBold
        /// </summary>
        public bool FontBold
        {
            get { return this.fontBold; }
            set { this.RaiseAndSetIfChanged(ref this.fontBold, value); }
        }

        /// <summary>
        /// Gets or sets the FontUnderline
        /// </summary>
        public bool FontUnderline
        {
            get { return this.fontUnderline; }
            set { this.RaiseAndSetIfChanged(ref this.fontUnderline, value); }
        }

        /// <summary>
        /// Gets or sets the FontStrokeThrough
        /// </summary>
        public bool FontStrokeThrough
        {
            get { return this.fontStrokeThrough; }
            set { this.RaiseAndSetIfChanged(ref this.fontStrokeThrough, value); }
        }

        /// <summary>
        /// Gets or sets the FillColor
        /// </summary>
        public Color FillColor
        {
            get { return this.fillColor; }
            set { this.RaiseAndSetIfChanged(ref this.fillColor, value); }
        }

        /// <summary>
        /// Gets or sets the StrokeColor
        /// </summary>
        public Color StrokeColor
        {
            get { return this.strokeColor; }
            set { this.RaiseAndSetIfChanged(ref this.strokeColor, value); }
        }

        /// <summary>
        /// Gets or sets the FontColor
        /// </summary>
        public Color FontColor
        {
            get { return this.fontColor; }
            set { this.RaiseAndSetIfChanged(ref this.fontColor, value); }
        }

	
        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
            if(this.Thing.FillOpacity.HasValue)
            {
                this.FillOpacity = this.Thing.FillOpacity.Value;
            }
            if(this.Thing.StrokeWidth.HasValue)
            {
                this.StrokeWidth = this.Thing.StrokeWidth.Value;
            }
            if(this.Thing.StrokeOpacity.HasValue)
            {
                this.StrokeOpacity = this.Thing.StrokeOpacity.Value;
            }
            if(this.Thing.FontSize.HasValue)
            {
                this.FontSize = this.Thing.FontSize.Value;
            }
            this.FontName = this.Thing.FontName;
            if(this.Thing.FontItalic.HasValue)
            {
                this.FontItalic = this.Thing.FontItalic.Value;
            }
            if(this.Thing.FontBold.HasValue)
            {
                this.FontBold = this.Thing.FontBold.Value;
            }
            if(this.Thing.FontUnderline.HasValue)
            {
                this.FontUnderline = this.Thing.FontUnderline.Value;
            }
            if(this.Thing.FontStrokeThrough.HasValue)
            {
                this.FontStrokeThrough = this.Thing.FontStrokeThrough.Value;
            }
            this.FillColor = this.Thing.FillColor;
            this.StrokeColor = this.Thing.StrokeColor;
            this.FontColor = this.Thing.FontColor;
        }
    }
}
