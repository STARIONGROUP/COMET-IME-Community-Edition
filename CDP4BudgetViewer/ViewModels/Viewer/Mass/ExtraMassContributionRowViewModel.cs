// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtraMassContributionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System.Linq;
    using ReactiveUI;
    using Services;

    /// <summary>
    /// View-model for rows representing extra contribution to a mass-budget
    /// </summary>
    public class ExtraMassContributionRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ContributionName"/>
        /// </summary>
        private string contributionName;

        /// <summary>
        /// Backing field for <see cref="ContributionTotal"/>
        /// </summary>
        private float contributionTotal;
        
        /// <summary>
        /// Backing field for <see cref="ContributionMargin"/>
        /// </summary>
        private float contributionMargin;

        /// <summary>
        /// Backing field for <see cref="ContributionMarginRatio"/>
        /// </summary>
        private float contributionMarginRatio;

        /// <summary>
        /// Backing field for <see cref="ContributionTotalWithMargin"/>
        /// </summary>
        private float contributionTotalWithMargin;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraMassContributionRowViewModel"/> class
        /// </summary>
        /// <param name="extraContributionResult">The <see cref="ExtraContribution"/></param>
        public ExtraMassContributionRowViewModel(ExtraContribution extraContributionResult)
        {
            this.ContributionTotal = extraContributionResult.TotalContribution;
            this.ContributionName = string.Join(", ", extraContributionResult.Categories.Select(x => x.Name));

            this.ContributionTotalWithMargin = extraContributionResult.TotalWithMargin;
            this.ContributionMarginRatio = extraContributionResult.MarginRatio;
            this.ContributionMargin = extraContributionResult.Margin;
        }

        /// <summary>
        /// Gets the contributor name
        /// </summary>
        public string ContributionName
        {
            get { return this.contributionName; }
            private set { this.RaiseAndSetIfChanged(ref this.contributionName, value); }
        }

        /// <summary>
        /// Gets the extra-mass contribution total
        /// </summary>
        public float ContributionTotal
        {
            get { return this.contributionTotal; }
            private set { this.RaiseAndSetIfChanged(ref this.contributionTotal, value); }
        }

        /// <summary>
        /// Gets the contributor margin
        /// </summary>
        public float ContributionMargin
        {
            get { return this.contributionMargin; }
            private set { this.RaiseAndSetIfChanged(ref this.contributionMargin, value); }
        }

        /// <summary>
        /// Gets contribution margin ratio
        /// </summary>
        public float ContributionMarginRatio
        {
            get { return this.contributionMarginRatio; }
            private set { this.RaiseAndSetIfChanged(ref this.contributionMarginRatio, value); }
        }

        /// <summary>
        /// Gets the total contribution with margin
        /// </summary>
        public float ContributionTotalWithMargin
        {
            get { return this.contributionTotalWithMargin; }
            private set { this.RaiseAndSetIfChanged(ref this.contributionTotalWithMargin, value); }
        }
    }
}
