// -------------------------------------------------------------------------------------------------
// <copyright file="ExtraContribution.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;

    public struct ExtraContribution
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraContribution"/> structure
        /// </summary>
        /// <param name="categories">The categories</param>
        /// <param name="total">The total</param>
        /// <param name="totalWithMargin">Thetotal with  margin</param>
        /// <param name="quantityKind">The quantityKind</param>
        /// <param name="scale">The measurement-scale for this extra-contribution</param>
        public ExtraContribution(IReadOnlyList<Category> categories, float total, float totalWithMargin, QuantityKind quantityKind, MeasurementScale scale)
        {
            this.Categories = categories;
            this.TotalContribution = total;
            this.TotalWithMargin = totalWithMargin;
            this.QuantityKind = quantityKind;
            this.Scale = scale;
        }

        /// <summary>
        /// Gets the Categories
        /// </summary>
        public IReadOnlyList<Category> Categories { get; }

        /// <summary>
        /// Gets the total contribution
        /// </summary>
        public float TotalContribution { get; }

        /// <summary>
        /// Gets the total value with margin of the extra contribution
        /// </summary>
        public float TotalWithMargin { get; }

        /// <summary>
        /// Gets the margin ratio
        /// </summary>
        public float MarginRatio => this.TotalContribution == 0f ? 0 : (this.TotalWithMargin / this.TotalContribution - 1) * 100f;

        /// <summary>
        /// Gets the margin
        /// </summary>
        public float Margin => this.TotalWithMargin - this.TotalContribution;

        /// <summary>
        /// Gets the <see cref="QuantityKind"/>
        /// </summary>
        public QuantityKind QuantityKind { get; }

        /// <summary>
        /// The scale used
        /// </summary>
        public MeasurementScale Scale { get; }
    }
}
