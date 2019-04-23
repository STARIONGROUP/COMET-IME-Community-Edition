// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnDefinition.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Composition.Services;
    using ReactiveUI;
    using Settings;

    /// <summary>
    /// A view-model for dynamic column definition
    /// </summary>
    public class ColumnDefinition : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Header"/>
        /// </summary>
        private string header;

        /// <summary>
        /// Backing field for <see cref="ToolTip"/>
        /// </summary>
        private string toolTip;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDefinition"/> class
        /// </summary>
        /// <param name="thing">The represented <see cref="Thing"/></param>
        /// <param name="displayKind">The <see cref="DisplayKind"/> of the column.</param>
        public ColumnDefinition(DefinedThing thing, DisplayKind displayKind)
        {
            this.RelationshipCount = 0;
            this.Header = displayKind == DisplayKind.Name ? thing.Name : thing.ShortName;
            this.ToolTip = thing.Tooltip();
            this.FieldName = thing.ShortName;
            this.ThingId = thing.Iid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnDefinition"/> class
        /// </summary>
        /// <param name="header">The header</param>
        /// <param name="fieldname">The field-name</param>
        /// <param name="offsetCount">By offsetting count we keep this column from being filtered out even though it does not hold relationships.</param>
        public ColumnDefinition(string header, string fieldname, bool offsetCount = false)
        {
            this.RelationshipCount = offsetCount? -1 : 0;
            this.Header = header;
            this.FieldName = fieldname;
        }

        /// <summary>
        /// Gets the identifier of the <see cref="Thing"/> represented by this column
        /// </summary>
        public Guid ThingId { get; }

        /// <summary>
        /// Gets the header to display
        /// </summary>
        public string Header
        {
            get { return this.header; }
            private set { this.RaiseAndSetIfChanged(ref this.header, value); }
        }

        /// <summary>
        /// Gets the tooltip to display
        /// </summary>
        public string ToolTip
        {
            get { return this.toolTip; }
            private set { this.RaiseAndSetIfChanged(ref this.toolTip, value); }
        }

        /// <summary>
        /// Gets the name of the fieldname for this column
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Gets or sets the total count of relationships this column has.
        /// </summary>
        public int RelationshipCount { get; set; }
    }
}
