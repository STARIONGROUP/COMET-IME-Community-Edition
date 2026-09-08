// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVGateStateToBrushConverter.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;
    using System.Windows.Media;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;

    using CDP4Composition.Utilities;

    /// <summary>
    /// Colours a cell of the VCRM by the stage gate state it reports, so the three answers a gate review has to act on,
    /// undefined, failed and closed out, are visible without reading any of the text.
    /// </summary>
    /// <remarks>
    /// The matrix columns are a project's own stage gates, so the grid generates them from a
    /// <see cref="System.Data.DataTable"/> and the cells carry text rather than a typed state. Rather than duplicate the
    /// labels here, the converter asks <see cref="VandVStageGateQuery.Describe"/> for each state's label and matches on
    /// it: a relabelled state stays coloured, and one that is not handled falls through to transparent.
    /// </remarks>
    public class VandVGateStateToBrushConverter : IValueConverter
    {
        /// <summary>
        /// The brush a cell that needs no attention gets, frozen so WPF does not clone it per cell.
        /// </summary>
        private static readonly SolidColorBrush Plain = CreateFrozen(Brushes.Transparent.Color);

        /// <summary>
        /// The state a cell reports, keyed by the label it leads with. Resolved once, because <see cref="Convert"/>
        /// runs for every visible cell on every rebuild and every scroll tick and may not rebuild the labels per call.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, VandVGateState> StateByLabel = Enum.GetValues(typeof(VandVGateState))
            .Cast<VandVGateState>()
            .Select(state => new KeyValuePair<string, VandVGateState>(VandVStageGateQuery.Describe(state), state))
            .Where(pair => !string.IsNullOrEmpty(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        /// <summary>
        /// The background brush each state gets, the three a gate review must act on coloured and the rest plain.
        /// Frozen so WPF shares one instance across every cell instead of cloning per binding.
        /// </summary>
        private static readonly IReadOnlyDictionary<VandVGateState, SolidColorBrush> BrushByState = new Dictionary<VandVGateState, SolidColorBrush>
        {
            { VandVGateState.Undefined, CreateFrozen(((SolidColorBrush)CDP4Color.Inconclusive.GetBrush()).Color) },
            { VandVGateState.Failed, CreateFrozen(((SolidColorBrush)CDP4Color.Failed.GetBrush()).Color) },
            { VandVGateState.ClosedOut, CreateFrozen(((SolidColorBrush)CDP4Color.Succeeded.GetBrush()).Color) }
        };

        /// <summary>
        /// Converts a matrix cell's text to the background brush for that cell.
        /// </summary>
        /// <param name="value">The cell text.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>A <see cref="SolidColorBrush"/>, transparent for a state that needs no attention.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;

            if (string.IsNullOrWhiteSpace(text))
            {
                return Plain;
            }

            if (VandVCompliance.Shortfalls.Any(shortfall => VandVCoverageQuery.AreSameEnumValue(text, shortfall)))
            {
                return BrushByState[VandVGateState.Failed];
            }

            return BrushByState.TryGetValue(QueryState(text), out var brush) ? brush : Plain;
        }

        /// <summary>
        /// Not supported: the matrix is read-only.
        /// </summary>
        /// <param name="value">The parameter is not used.</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>A <see cref="NotSupportedException"/> is thrown.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(VandVGateStateToBrushConverter)} can only be used in a one-way binding.");
        }

        /// <summary>
        /// Recovers the state a cell reports from the label it leads with.
        /// </summary>
        /// <param name="text">The cell text.</param>
        /// <returns>The state, or <see cref="VandVGateState.Complete"/> when the text carries no state label.</returns>
        /// <remarks>
        /// The style that uses this converter is applied to every cell of the grid, requirement short-names and names
        /// included, because the stage gate columns are generated and cannot be styled individually. Matching the
        /// whole label loosely (a bare <c>StartsWith</c>) coloured a requirement named "Closed out actions from PDR"
        /// green in the very artifact a gate review decides from. Every cell that really does report a state is either
        /// the label alone (<see cref="VandVGateCell.QueryText"/> with no detail, and the State at Gate column) or the
        /// label followed by ": " and the detail (the same cell with detail, and the verdict column), so the leading
        /// token up to the first colon is the label, and only an exact dictionary hit on it counts.
        /// </remarks>
        private static VandVGateState QueryState(string text)
        {
            var colon = text.IndexOf(':');
            var label = colon < 0 ? text : text.Substring(0, colon);

            return StateByLabel.TryGetValue(label, out var state) ? state : VandVGateState.Complete;
        }

        /// <summary>
        /// Builds a frozen brush, which WPF can share across every cell instead of cloning per binding.
        /// </summary>
        /// <param name="color">The colour.</param>
        /// <returns>The frozen brush.</returns>
        private static SolidColorBrush CreateFrozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            return brush;
        }
    }
}
