// ------------------------------------------------------------------------------------------------
// <copyright file="BooleanExpressionMethods.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Requirements.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Requirements.ViewModels;

    /// <summary>
    /// This class contains methods for specific <see cref="BooleanExpression"/> related functionality 
    /// </summary>
    public static class BooleanExpressionExtensions
    {
        /// <summary>
        /// Gets the <see cref="BooleanExpression"/>'s viewmodel
        /// </summary>
        /// <typeparam name="T">Needed to correctly specify the rules the <see cref="viewModel"/> parameters needs to satisfy.</typeparam>
        /// <param name="booleanExpression">The <see cref="BooleanExpression"/> that needs to be checked, so the right viewmodel can be created and returned.</param>
        /// <param name="viewModel">A viewmodel that implements the <see cref="IViewModelBase{Thing}"/> and the <see cref="IISession"/> interfaces</param>
        /// <returns>Implementation of <see cref="IRowViewModelBase{BooleanExpression}"/></returns>
        public static IRowViewModelBase<BooleanExpression> GetBooleanExpressionViewModel<T>(this BooleanExpression booleanExpression, T viewModel) where T : IViewModelBase<Thing>, IISession
        {
            if (booleanExpression is NotExpression notExpression)
            {
                return new NotExpressionRowViewModel(notExpression, viewModel.Session, viewModel);
            }

            if (booleanExpression is AndExpression andExpression)
            {
                return new AndExpressionRowViewModel(andExpression, viewModel.Session, viewModel);
            }

            if (booleanExpression is OrExpression orExpression)
            {
                return new OrExpressionRowViewModel(orExpression, viewModel.Session, viewModel);
            }

            if (booleanExpression is ExclusiveOrExpression exclusiveOrExpression)
            {
                return new ExclusiveOrExpressionRowViewModel(exclusiveOrExpression, viewModel.Session, viewModel);
            }

            if (booleanExpression is RelationalExpression relationalExpression)
            {
                return new RelationalExpressionRowViewModel(relationalExpression, viewModel.Session, viewModel);
            }

            return null;
        }
    }
}
