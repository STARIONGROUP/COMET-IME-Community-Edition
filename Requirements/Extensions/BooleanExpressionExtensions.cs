// ------------------------------------------------------------------------------------------------
// <copyright file="BooleanExpressionMethods.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
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

        /// <summary>
        /// Creates a string that represents a tree of (nested) <see cref="BooleanExpression"/>s.
        /// </summary>
        /// <param name="rows">The rows which are used to create <see cref="BooleanExpression"/>'s string representation</param>
        /// <param name="thing">The <see cref="Thing"/> for which the tree will be built</param>
        /// <returns>A <see cref="string"/> that represents the <see cref="BooleanExpression"/> tree</returns>
        public static string ToExpressionString(this IEnumerable<IRowViewModelBase<BooleanExpression>> rows, Thing thing)
        {
            var stringBuilder = new StringBuilder();

            if (rows != null)
            {
                foreach (var expressionRow in rows.ToList())
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(thing is BooleanExpression booleanExpression ? $" {booleanExpression.StringValue} " : " AND ");
                    }

                    GetStringExpression(expressionRow, stringBuilder);
                }
            }

            if (thing.ClassKind == ClassKind.NotExpression)
            {
                stringBuilder.Insert(0, $"{((NotExpression)thing).StringValue} ");
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Builds a string that represents the whole tree for the <see cref="BooleanExpression"/> of the given row.
        /// </summary>
        /// <param name="expressionRow"></param>
        /// <param name="stringBuilder"></param>
        private static void GetStringExpression(IRowViewModelBase<BooleanExpression> expressionRow, StringBuilder stringBuilder)
        {
            if (expressionRow.Thing.ClassKind == ClassKind.RelationalExpression)
            {
                stringBuilder.Append("(");
                stringBuilder.Append(expressionRow.Thing.StringValue.Trim());
                stringBuilder.Append(")");
            }
            else
            {
                if (expressionRow.ContainerViewModel is IRowViewModelBase<BooleanExpression>)
                {
                    stringBuilder.Append("(");
                }

                foreach (var containedExpressionRow in expressionRow.ContainedRows)
                {
                    if (expressionRow.Thing.ClassKind == ClassKind.NotExpression)
                    {
                        stringBuilder.Append($" {expressionRow.Thing.StringValue} ");

                        GetStringExpression(containedExpressionRow as IRowViewModelBase<BooleanExpression>, stringBuilder);
                    }
                    else
                    {
                        GetStringExpression(containedExpressionRow as IRowViewModelBase<BooleanExpression>, stringBuilder);

                        if (containedExpressionRow != expressionRow.ContainedRows.Last())
                        {
                            stringBuilder.Append($" {expressionRow.Thing.StringValue} ");
                        }
                    }
                }

                if (expressionRow.ContainerViewModel is IRowViewModelBase<BooleanExpression>)
                {
                    stringBuilder.Append(")");
                }
            }
        }
    }
}
