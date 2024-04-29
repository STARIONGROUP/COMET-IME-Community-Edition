// -----------------------------------------------------------------------
// <copyright file="IterationTrackParameter.cs" company="Starion Group S.A.">
// Copyright (c) 2020 Starion Group S.A. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Widget
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The class that contains information about the tracked <see cref="ParameterOrOverrideBase"/>
    /// </summary>
    public class IterationTrackParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IterationTrackParameter"/> class.
        /// </summary>
        /// <param name="parameterOrOverride">The <see cref="ParameterOrOverrideBase"/> that is used to construct this <see cref="IterationTrackParameter"/></param>
        public IterationTrackParameter(ParameterOrOverrideBase parameterOrOverride)
        {
            this.ParameterOrOverride = parameterOrOverride;
            this.ModelCode = parameterOrOverride?.ModelCode() ?? "";
            this.UnitSymbol = parameterOrOverride?.Scale?.UserFriendlyShortName ?? "-";
        }

        /// <summary>
        /// The <see cref="ParameterOrOverrideBase"/> that was used to construct this <see cref="IterationTrackParameter"/> 
        /// </summary>
        public ParameterOrOverrideBase ParameterOrOverride { get; }

        /// <summary>
        /// The ModelCode that is used to show in a Widget
        /// </summary>
        public string ModelCode { get; }

        /// <summary>
        /// The title of the control 
        /// </summary>
        public string ControlTitle { get; set; }

        /// <summary>
        /// The symbol of the Parameter's unit (kg, cm, etc.)
        /// </summary>
        public string UnitSymbol { get; }
    }
}
