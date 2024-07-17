// -------------------------------------------------------------------------------------------------
// <copyright file="ParametricConstraintRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;

    /// <summary>
    /// Partial class representing <see cref="ParametricConstraintRowViewModel"/>
    /// </summary>
    public partial class ParametricConstraintRowViewModel
    {
        /// <summary>
        /// Updates the column values in <see cref="ObjectBrowser"/>
        /// </summary>
        protected override void UpdateColumnValues()
        {
            this.Name = string.Format("{{ {0} }}", string.Join(", ", this.Thing.Expression.Select(e => e.StringValue)));
            this.ShortName = (this.Thing.TopExpression == null) ? string.Empty : this.Thing.TopExpression.StringValue;
        }
    }
}
