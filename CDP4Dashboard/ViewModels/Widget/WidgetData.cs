// -----------------------------------------------------------------------
// <copyright file="WidgetData.cs" company="RHEA">
// Copyright (c) 2020 RHEA Group. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Widget
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    /// <summary>
    /// The data that belongs to a Widget and that can be used in tiles and charts
    /// </summary>
    public class WidgetData
    {
        /// <summary>
        /// Sets or gets the <see cref="ParameterOrOverrideBase"/> 
        /// </summary>
        public ParameterOrOverrideBase ParameterOrOverride { get; set; }

        /// <summary>
        /// Gets the Parameter Name
        /// </summary>
        public string ParameterName => this.ParameterOrOverride?.ModelCode() ?? "";

        /// <summary>
        /// Gets or sets the <see cref="ActualFiniteState"/>
        /// </summary>
        public ActualFiniteState ActualFiniteState { get; set; }

        /// <summary>
        /// Gets or sets the State Name
        /// </summary>
        public string StateName => this.ActualFiniteState?.Name ?? "";

        /// <summary>
        /// Gets or sets the <see cref="Option"/>
        /// </summary>
        public Option Option { get; set; }

        /// <summary>
        /// Gets or sets the Option Name
        /// </summary>
        public string OptionName => this.Option?.Name ?? "";

        /// <summary>
        /// Gets or sets the <see cref="Value"/>
        /// </summary>
        public ValueArray<string> Value { get; set; }

        /// <summary>
        /// Gets or sets the Revision Number
        /// </summary>
        public int RevisionNumber { get; set; }

        /// <summary>
        /// Gets or sets the actor <see cref="Person"/> 
        /// </summary>
        public Person Actor { get; set; }

        /// <summary>
        /// Gets or sets the Instant <see cref="DateTime"/>
        /// </summary>
        public DateTime Instant { get; set; }
    }
}
