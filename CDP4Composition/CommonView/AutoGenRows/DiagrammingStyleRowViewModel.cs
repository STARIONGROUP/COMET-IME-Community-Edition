// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagrammingStyleRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    This is an auto-generated class. Any manual changes to this file will be overwritten!
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
        /// Backing field for <see cref="FillColor"/> property
        /// </summary>
        private Color fillColor;

        /// <summary>
        /// Backing field for <see cref="FillColorName"/> property
        /// </summary>
        private string fillColorName;

        /// <summary>
        /// Backing field for <see cref="FillOpacity"/> property
        /// </summary>
        private float fillOpacity;

        /// <summary>
        /// Backing field for <see cref="FontBold"/> property
        /// </summary>
        private bool fontBold;

        /// <summary>
        /// Backing field for <see cref="FontColor"/> property
        /// </summary>
        private Color fontColor;

        /// <summary>
        /// Backing field for <see cref="FontColorName"/> property
        /// </summary>
        private string fontColorName;

        /// <summary>
        /// Backing field for <see cref="FontItalic"/> property
        /// </summary>
        private bool fontItalic;

        /// <summary>
        /// Backing field for <see cref="FontName"/> property
        /// </summary>
        private string fontName;

        /// <summary>
        /// Backing field for <see cref="FontSize"/> property
        /// </summary>
        private float fontSize;

        /// <summary>
        /// Backing field for <see cref="FontStrokeThrough"/> property
        /// </summary>
        private bool fontStrokeThrough;

        /// <summary>
        /// Backing field for <see cref="FontUnderline"/> property
        /// </summary>
        private bool fontUnderline;

        /// <summary>
        /// Backing field for <see cref="StrokeColor"/> property
        /// </summary>
        private Color strokeColor;

        /// <summary>
        /// Backing field for <see cref="StrokeColorName"/> property
        /// </summary>
        private string strokeColorName;

        /// <summary>
        /// Backing field for <see cref="StrokeOpacity"/> property
        /// </summary>
        private float strokeOpacity;

        /// <summary>
        /// Backing field for <see cref="StrokeWidth"/> property
        /// </summary>
        private float strokeWidth;

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
        /// Gets or sets the FillColor
        /// </summary>
        public Color FillColor
        {
            get { return this.fillColor; }
            set { this.RaiseAndSetIfChanged(ref this.fillColor, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="FillColor"/>
        /// </summary>
        public string FillColorName
        {
            get { return this.fillColorName; }
            set { this.RaiseAndSetIfChanged(ref this.fillColorName, value); }
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
        /// Gets or sets the FontBold
        /// </summary>
        public bool FontBold
        {
            get { return this.fontBold; }
            set { this.RaiseAndSetIfChanged(ref this.fontBold, value); }
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
        /// Gets or set the Name of <see cref="FontColor"/>
        /// </summary>
        public string FontColorName
        {
            get { return this.fontColorName; }
            set { this.RaiseAndSetIfChanged(ref this.fontColorName, value); }
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
        /// Gets or sets the FontName
        /// </summary>
        public string FontName
        {
            get { return this.fontName; }
            set { this.RaiseAndSetIfChanged(ref this.fontName, value); }
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
        /// Gets or sets the FontStrokeThrough
        /// </summary>
        public bool FontStrokeThrough
        {
            get { return this.fontStrokeThrough; }
            set { this.RaiseAndSetIfChanged(ref this.fontStrokeThrough, value); }
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
        /// Gets or sets the StrokeColor
        /// </summary>
        public Color StrokeColor
        {
            get { return this.strokeColor; }
            set { this.RaiseAndSetIfChanged(ref this.strokeColor, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="StrokeColor"/>
        /// </summary>
        public string StrokeColorName
        {
            get { return this.strokeColorName; }
            set { this.RaiseAndSetIfChanged(ref this.strokeColorName, value); }
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
        /// Gets or sets the StrokeWidth
        /// </summary>
        public float StrokeWidth
        {
            get { return this.strokeWidth; }
            set { this.RaiseAndSetIfChanged(ref this.strokeWidth, value); }
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
            this.FillColor = this.Thing.FillColor;
            if (this.Thing.FillColor != null)
            {
                this.FillColorName = this.Thing.FillColor.Name;
            }
            else
            {
                this.FillColorName = string.Empty;
            }
            if (this.Thing.FillOpacity.HasValue)
            {
                this.FillOpacity = this.Thing.FillOpacity.Value;
            }
            if (this.Thing.FontBold.HasValue)
            {
                this.FontBold = this.Thing.FontBold.Value;
            }
            this.FontColor = this.Thing.FontColor;
            if (this.Thing.FontColor != null)
            {
                this.FontColorName = this.Thing.FontColor.Name;
            }
            else
            {
                this.FontColorName = string.Empty;
            }
            if (this.Thing.FontItalic.HasValue)
            {
                this.FontItalic = this.Thing.FontItalic.Value;
            }
            this.FontName = this.Thing.FontName;
            if (this.Thing.FontSize.HasValue)
            {
                this.FontSize = this.Thing.FontSize.Value;
            }
            if (this.Thing.FontStrokeThrough.HasValue)
            {
                this.FontStrokeThrough = this.Thing.FontStrokeThrough.Value;
            }
            if (this.Thing.FontUnderline.HasValue)
            {
                this.FontUnderline = this.Thing.FontUnderline.Value;
            }
            this.StrokeColor = this.Thing.StrokeColor;
            if (this.Thing.StrokeColor != null)
            {
                this.StrokeColorName = this.Thing.StrokeColor.Name;
            }
            else
            {
                this.StrokeColorName = string.Empty;
            }
            if (this.Thing.StrokeOpacity.HasValue)
            {
                this.StrokeOpacity = this.Thing.StrokeOpacity.Value;
            }
            if (this.Thing.StrokeWidth.HasValue)
            {
                this.StrokeWidth = this.Thing.StrokeWidth.Value;
            }
        }
    }
}
