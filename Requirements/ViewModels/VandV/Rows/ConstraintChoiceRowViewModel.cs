// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstraintChoiceRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels.Rows
{
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Extensions;

    using ReactiveUI;

    /// <summary>
    /// One choice in the "From Constraint" picker: either a whole <see cref="ParametricConstraint"/> or a single
    /// <see cref="RelationalExpression"/> within it, so a constraint made of several expressions can be verified by
    /// several V&amp;V items, one per expression.
    /// </summary>
    /// <remarks>
    /// <see cref="ParametricConstraint"/> has no name of its own, which is why the picker binds to this row's
    /// <see cref="Display"/> rather than to the constraint directly (binding to a non-existent <c>Name</c> is what made
    /// the drop-down render blank).
    /// </remarks>
    public class ConstraintChoiceRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConstraintChoiceRowViewModel"/> class.
        /// </summary>
        /// <param name="constraint">The owning <see cref="ParametricConstraint"/>.</param>
        /// <param name="expression">The single <see cref="RelationalExpression"/>, or null for the whole constraint.</param>
        public ConstraintChoiceRowViewModel(ParametricConstraint constraint, RelationalExpression expression)
        {
            this.Constraint = constraint;
            this.Expression = expression;
        }

        /// <summary>
        /// Gets the owning <see cref="ParametricConstraint"/>.
        /// </summary>
        public ParametricConstraint Constraint { get; }

        /// <summary>
        /// Gets the single <see cref="RelationalExpression"/> this choice covers, or null when it is the whole
        /// constraint.
        /// </summary>
        public RelationalExpression Expression { get; }

        /// <summary>
        /// Gets the expression text this choice contributes to the acceptance criteria.
        /// </summary>
        public string ExpressionText =>
            this.Expression != null
                ? $"{this.Expression.ParameterType?.ShortName} {this.Expression.RelationalOperator.ToScientificNotationString()} {string.Join(", ", this.Expression.Value)}"
                : this.Constraint.ToExpressionString();

        /// <summary>
        /// Gets the text shown in the picker.
        /// </summary>
        public string Display =>
            this.Expression != null
                ? $"expression: {this.ExpressionText}"
                : $"whole constraint: {this.ExpressionText}";

        /// <summary>
        /// Gets the <see cref="ParameterOrOverrideBase"/> this choice is bound to, resolved through the
        /// <see cref="BinaryRelationship"/> that links a relational expression to the parameter it constrains. Returns
        /// null when the expression is not bound to a parameter.
        /// </summary>
        public ParameterOrOverrideBase LinkedParameter
        {
            get
            {
                var expressions = this.Expression != null
                    ? new[] { this.Expression }
                    : this.Constraint.Expression.OfType<RelationalExpression>().ToArray();

                var iteration = this.Constraint.GetContainerOfType<Iteration>();

                if (iteration == null)
                {
                    return null;
                }

                // scan the iteration rather than Thing.QueryRelationships: that list is maintained by the SDK's DTO
                // resolver, so it is empty for anything assembled in memory
                var expressionIids = expressions.Select(x => x.Iid).ToList();

                return iteration.Relationship
                    .OfType<BinaryRelationship>()
                    .Where(relationship => relationship.Target != null && expressionIids.Contains(relationship.Target.Iid))
                    .Select(relationship => relationship.Source)
                    .OfType<ParameterOrOverrideBase>()
                    .FirstOrDefault();
            }
        }
    }
}
