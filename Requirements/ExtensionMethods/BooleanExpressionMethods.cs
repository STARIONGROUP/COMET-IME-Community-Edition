// ------------------------------------------------------------------------------------------------
// <copyright file="BooleanExpressionMethods.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace CDP4Requirements.ExtensionMethods
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Requirements.ViewModels;

    /// <summary>
    /// This class contains methods for specific BooleanExpression related functionality 
    /// </summary>
    public static class BooleanExpressionMethods
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
                stringBuilder.Append(expressionRow.Thing.StringValue);
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

        /// <summary>
        /// Gets the expressions that are toplevel for this list of <see cref="BooleanExpression"/>
        /// </summary>
        /// <returns><see cref="IReadOnlyList{BooleanExpression}"/> containing top level <see cref="BooleanExpression"/>s</returns>
        public static IReadOnlyList<BooleanExpression> GetTopLevelExpressions(this IList<BooleanExpression> expressionList)
        {
            var notInTerms = new List<BooleanExpression>();

            foreach (var thingExpression in expressionList)
            {
                switch (thingExpression.ClassKind)
                {
                    case ClassKind.NotExpression:
                        if (thingExpression is NotExpression notExpression && !notInTerms.Contains(notExpression.Term))
                        {
                            notInTerms.Add(notExpression.Term);
                        }

                        break;

                    case ClassKind.AndExpression:
                        notInTerms.AddRange(((AndExpression)thingExpression).Term.Where(x => !notInTerms.Contains(x)));

                        break;

                    case ClassKind.OrExpression:
                        notInTerms.AddRange(((OrExpression)thingExpression).Term.Where(x => !notInTerms.Contains(x)));

                        break;

                    case ClassKind.ExclusiveOrExpression:
                        notInTerms.AddRange(((ExclusiveOrExpression)thingExpression).Term.Where(x => !notInTerms.Contains(x)));

                        break;
                }
            }

            return expressionList.Where(x => !notInTerms.Contains(x)).ToList();
        }

        /// <summary>
        /// Gets the expressions that are children of <see cref="myself" /> or are "free" at the toplevel of the <see cref="BooleanExpression"/> tree.
        /// "Free" means not set as a child of another <see cref="BooleanExpression"/>.
        /// </summary>
        /// <param name="expressionList">List that contains all known <see cref="BooleanExpression"/>s</param>
        /// <param name="myself">The <see cref="BooleanExpression"/> for which its direct children must be returned</param>
        /// <returns><see cref="IReadOnlyList{BooleanExpression}"/> containing <see cref="BooleanExpression"/>s that are direct children of the class in the <see cref="myself"/> parameter or that are not set as a child for another <see cref="BooleanExpression"/></returns>
        public static IReadOnlyList<BooleanExpression> GetMyAndFreeExpressions(this IList<BooleanExpression> expressionList, BooleanExpression myself)
        {
            var myExpressions = new List<BooleanExpression>();

            myExpressions.AddRange(GetMyExpressions(myself));
            myExpressions.AddRange(expressionList.GetTopLevelExpressions().OfType<RelationalExpression>().Where(x => !myExpressions.Contains(x)));

            return myExpressions.ToList();
        }

        /// <summary>
        /// Gets the expressions that are direct children of <see cref="myself"/>
        /// </summary>
        /// <param name="myself"></param>
        /// <returns><see cref="IReadOnlyList{BooleanExpression}"/> containing <see cref="BooleanExpression"/>s that are direct children of the class in the <see cref="myself"/> parameter</returns>
        private static IReadOnlyList<BooleanExpression> GetMyExpressions(BooleanExpression myself)
        {
            var myExpressions = new List<BooleanExpression>();

            switch (myself.ClassKind)
            {
                case ClassKind.NotExpression:
                    if (myself is NotExpression notExpression)
                    {
                        myExpressions.Add(notExpression.Term);
                    }

                    break;

                case ClassKind.AndExpression:
                    myExpressions.AddRange(((AndExpression)myself).Term);

                    break;

                case ClassKind.OrExpression:
                    myExpressions.AddRange(((OrExpression)myself).Term);

                    break;

                case ClassKind.ExclusiveOrExpression:
                    myExpressions.AddRange(((ExclusiveOrExpression)myself).Term);

                    break;
            }

            return myExpressions.ToList();
        }
    }
}
