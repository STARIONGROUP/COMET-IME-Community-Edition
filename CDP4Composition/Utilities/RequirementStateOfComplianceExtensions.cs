// -----------------------------------------------------------------------------------------------
// <copyright file="RequirementStateOfComplianceExtensions.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    using System.Windows.Media;

    using CDP4RequirementsVerification;

    /// <summary>
    /// Extension methods for the <see cref="RequirementStateOfCompliance"/> enum
    /// </summary>
    public static class RequirementStateOfComplianceExtensions
    {
        /// <summary>
        /// Get the hexadecimal string value representation for a RequirementStateOfCompliance's color.
        /// </summary>
        /// <param name="stateOfCompliance">The <see cref="RequirementStateOfCompliance"/></param>
        /// <returns>Hexadecimal string value representation of the color</returns>
        public static string GetHexColorValue(this RequirementStateOfCompliance stateOfCompliance)
        {
            switch (stateOfCompliance)
            {
                case RequirementStateOfCompliance.Pass:
                    return CDP4Color.Succeeded.GetHexValue();

                case RequirementStateOfCompliance.Failed:
                    return CDP4Color.Failed.GetHexValue();

                case RequirementStateOfCompliance.Inconclusive:
                    return CDP4Color.Inconclusive.GetHexValue();

                default:
                    return "#CCCCCC";
            }
        }

        /// <summary>
        /// Get the <see cref="SolidColorBrush"/> representation for a <see cref="RequirementStateOfCompliance"/>.
        /// </summary>
        /// <param name="stateOfCompliance">The <see cref="RequirementStateOfCompliance"/></param>
        /// <returns><see cref="SolidColorBrush"/></returns>
        public static Brush GetBrush(this RequirementStateOfCompliance stateOfCompliance)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(stateOfCompliance.GetHexColorValue());
        }
    }
}
