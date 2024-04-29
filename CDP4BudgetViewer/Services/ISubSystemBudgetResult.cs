// -------------------------------------------------------------------------------------------------
// <copyright file="ISubSystemBudgetResult.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Services
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using Config;
    using ViewModels;

    public interface ISubSystemBudgetResult
    {
        /// <summary>
        /// Gets the current <see cref="SubSystem"/>
        /// </summary>
        SubSystem SubSystem { get; }

        /// <summary>
        /// Gets the current <see cref="Config.BudgetConfig"/>
        /// </summary>
        BudgetConfig BudgetConfig { get; }

        /// <summary>
        /// Gets the current <see cref="Option"/>
        /// </summary>
        Option Option { get; }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise"/>
        /// </summary>
        DomainOfExpertise CurrentDomain { get; }

        /// <summary>
        /// Gets the <see cref="SystemLevelKind"/> to use by default
        /// </summary>
        SystemLevelKind SystemLevelUsed { get; }

        /// <summary>
        /// Gets the <see cref="MeasurementScale"/> used
        /// </summary>
        MeasurementScale Scale { get; }
    }
}
