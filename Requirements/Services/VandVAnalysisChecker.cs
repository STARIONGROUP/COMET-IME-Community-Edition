// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVAnalysisChecker.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Evaluates a V&amp;V item's covered parameter against the parametric constraints of the requirement it verifies,
    /// so verification by analysis can be answered from the model itself rather than by hand.
    /// </summary>
    /// <remarks>
    /// This is the one thing a spreadsheet VCD cannot do. It only reports; it never writes a status, because deciding
    /// that a requirement is met stays a human act. The check is skipped unless the item actually has a
    /// <c>coversParameter</c> link, so it never invents a verdict from a guess about which parameter was meant.
    /// </remarks>
    public static class VandVAnalysisChecker
    {
        /// <summary>
        /// Evaluates the V&amp;V item.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/>.</param>
        /// <param name="vandVItem">The V&amp;V item.</param>
        /// <returns>The evaluation, never null.</returns>
        public static VandVAnalysisResult Check(Iteration iteration, Requirement vandVItem)
        {
            if (iteration == null || vandVItem == null)
            {
                return VandVAnalysisResult.NotApplicable("no item");
            }

            var parameter = VandVCoverageWriter
                .QueryCoveredThings<ParameterOrOverrideBase>(iteration, vandVItem, VandVCoverageWriter.CoversParameter)
                .FirstOrDefault();

            if (parameter == null)
            {
                return VandVAnalysisResult.NotApplicable("no covered parameter");
            }

            var requirement = VandVItemCreator.QueryCoveringRelationship(iteration, vandVItem)?.Target as Requirement;

            if (requirement == null)
            {
                return VandVAnalysisResult.NotApplicable("no verified requirement");
            }

            var coveredOptions = VandVCoverageWriter.QueryCoveredThings<Option>(iteration, vandVItem, VandVCoverageWriter.CoversOption).Select(x => x.Iid).ToList();
            var coveredStates = VandVCoverageWriter.QueryCoveredThings<ActualFiniteState>(iteration, vandVItem, VandVCoverageWriter.CoversState).Select(x => x.Iid).ToList();

            return Evaluate(requirement, parameter, coveredOptions, coveredStates);
        }

        /// <summary>
        /// Evaluates a parameter against a requirement's parametric constraints, without needing a saved V&amp;V item.
        /// The dialog uses this to show the verdict while the item is still being filled in.
        /// </summary>
        /// <param name="requirement">The requirement whose constraints apply.</param>
        /// <param name="parameter">The parameter the activity measures.</param>
        /// <param name="coveredOptions">The options the activity covers; empty means every option.</param>
        /// <param name="coveredStates">The actual finite states the activity covers; empty means every state.</param>
        /// <returns>The evaluation, never null.</returns>
        public static VandVAnalysisResult Evaluate(Requirement requirement, ParameterOrOverrideBase parameter, IReadOnlyList<Guid> coveredOptions, IReadOnlyList<Guid> coveredStates)
        {
            if (requirement == null || parameter == null)
            {
                return VandVAnalysisResult.NotApplicable("no covered parameter");
            }

            var expressions = requirement.ParametricConstraint
                .SelectMany(constraint => constraint.Expression.OfType<RelationalExpression>())
                .Where(expression => expression.ParameterType != null
                                     && parameter.ParameterType != null
                                     && expression.ParameterType.Iid == parameter.ParameterType.Iid)
                .ToList();

            if (!expressions.Any())
            {
                return VandVAnalysisResult.NotApplicable("no constraint on this parameter type");
            }

            // comparing a value in kilogram against a limit in gram would silently report a pass, so a differing
            // scale is reported as unchecked rather than guessed at; unit conversion is a separate job
            var mismatched = expressions.FirstOrDefault(expression =>
                expression.Scale != null && parameter.Scale != null && expression.Scale.Iid != parameter.Scale.Iid);

            if (mismatched != null)
            {
                return VandVAnalysisResult.NotApplicable(
                    $"the constraint is in {mismatched.Scale.ShortName} but the parameter is in {parameter.Scale.ShortName}");
            }

            // filter only on the dimensions the parameter actually depends on: a coversOption link left over from a
            // parameter that was later swapped must not filter away the one value set a plain parameter has
            var valueSets = parameter.ValueSets
                .Where(valueSet =>
                    (!parameter.IsOptionDependent || !coveredOptions.Any() || (valueSet.ActualOption != null && coveredOptions.Contains(valueSet.ActualOption.Iid)))
                    && (parameter.StateDependence == null || !coveredStates.Any() || (valueSet.ActualState != null && coveredStates.Contains(valueSet.ActualState.Iid))))
                .ToList();

            if (!valueSets.Any())
            {
                return VandVAnalysisResult.NotApplicable("the covered parameter has no matching value set");
            }

            var failures = new List<string>();
            var checks = 0;

            foreach (var valueSet in valueSets)
            {
                var actual = valueSet.ActualValue.FirstOrDefault();

                if (!TryParse(actual, out var actualValue))
                {
                    continue;
                }

                foreach (var expression in expressions)
                {
                    var limitText = expression.Value.FirstOrDefault();

                    if (!TryParse(limitText, out var limit))
                    {
                        continue;
                    }

                    checks++;

                    if (!Satisfies(actualValue, expression.RelationalOperator, limit))
                    {
                        var slice = DescribeSlice(valueSet);
                        var symbol = expression.RelationalOperator.ToScientificNotationString();

                        failures.Add($"{parameter.ParameterType.ShortName}{slice} is {Format(actualValue)}, required {symbol} {Format(limit)}");
                    }
                }
            }

            if (checks == 0)
            {
                return VandVAnalysisResult.NotApplicable("no numeric value to compare");
            }

            return failures.Any()
                ? VandVAnalysisResult.Violated(string.Join("; ", failures))
                : VandVAnalysisResult.Satisfied($"{parameter.ParameterType.ShortName}, {checks} check(s) passed");
        }

        /// <summary>
        /// Names the option and state a value set pertains to, so a failure says which slice failed.
        /// </summary>
        /// <param name="valueSet">The value set.</param>
        /// <returns>The slice description, empty when the parameter is neither option nor state dependent.</returns>
        private static string DescribeSlice(IValueSet valueSet)
        {
            var parts = new List<string>();

            if (valueSet.ActualOption != null)
            {
                parts.Add(valueSet.ActualOption.ShortName);
            }

            if (valueSet.ActualState != null)
            {
                parts.Add(valueSet.ActualState.ShortName);
            }

            return parts.Any() ? $" [{string.Join(", ", parts)}]" : string.Empty;
        }

        /// <summary>
        /// Applies a relational operator.
        /// </summary>
        /// <param name="actual">The actual value.</param>
        /// <param name="relationalOperator">The operator.</param>
        /// <param name="limit">The required value.</param>
        /// <returns>true when the constraint holds.</returns>
        private static bool Satisfies(double actual, RelationalOperatorKind relationalOperator, double limit)
        {
            switch (relationalOperator)
            {
                // exact equality on purpose: both sides are parsed from the stored text, so identical text yields
                // identical doubles. This is not an approximate comparison and must not be read as one.
                case RelationalOperatorKind.EQ:
                    return actual == limit;
                case RelationalOperatorKind.NE:
                    return actual != limit;
                case RelationalOperatorKind.LT:
                    return actual < limit;
                case RelationalOperatorKind.GT:
                    return actual > limit;
                case RelationalOperatorKind.LE:
                    return actual <= limit;
                case RelationalOperatorKind.GE:
                    return actual >= limit;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Parses a stored value. Parameter values are always written with the invariant culture.
        /// </summary>
        /// <param name="text">The stored text.</param>
        /// <param name="value">The parsed value.</param>
        /// <returns>true when the text is a number.</returns>
        private static bool TryParse(string text, out double value)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Formats a value for display.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The formatted value.</returns>
        private static string Format(double value)
        {
            return value.ToString("G6", CultureInfo.InvariantCulture);
        }
    }
}
