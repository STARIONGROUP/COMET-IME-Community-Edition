// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetComputationException.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Exceptions
{
    using System;

    /// <summary>
    /// An exception class used when a budget-computation issue occured
    /// </summary>
    public class BudgetComputationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetComputationException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        public BudgetComputationException(string message) : base(message)
        {
        }
    }
}
