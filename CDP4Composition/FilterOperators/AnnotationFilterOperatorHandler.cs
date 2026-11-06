// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationFilterOperatorHandler.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.FilterOperators
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.ReportingData;

    using CDP4Composition.Annotations;
    using CDP4Composition.Mvvm;

    using DevExpress.Data.Filtering;
    using DevExpress.Xpf.Core.FilteringUI;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Handles the setting of operators and the collection of data for an annotation column in a
    /// <see cref="DataViewBase"/>, so a browser can be narrowed to the items that carry an annotation, that carry a
    /// particular kind of annotation, or that still have one open.
    /// </summary>
    public class AnnotationFilterOperatorHandler : CustomFilterOperatorHandler
    {
        /// <summary>
        /// The name of the HasAnyAnnotation filter operator
        /// </summary>
        public const string HasAnyAnnotationName = "HasAnyAnnotation";

        /// <summary>
        /// The name of the HasNoAnnotation filter operator
        /// </summary>
        public const string HasNoAnnotationName = "HasNoAnnotation";

        /// <summary>
        /// The name of the HasOpenAnnotation filter operator
        /// </summary>
        public const string HasOpenAnnotationName = "HasOpenAnnotation";

        /// <summary>
        /// The name of the HasClosedAnnotation filter operator
        /// </summary>
        public const string HasClosedAnnotationName = "HasClosedAnnotation";

        /// <summary>
        /// The name of the HasAnnotationOfKind filter operator
        /// </summary>
        public const string HasAnnotationOfKindName = "HasAnnotationOfKind";

        /// <summary>
        /// The name of the HasOpenAnnotationOfKind filter operator
        /// </summary>
        public const string HasOpenAnnotationOfKindName = "HasOpenAnnotationOfKind";

        /// <summary>
        /// The name of the HasClosedAnnotationOfKind filter operator
        /// </summary>
        public const string HasClosedAnnotationOfKindName = "HasClosedAnnotationOfKind";

        /// <summary>
        /// Constructor which starts the static registration of the custom filter operators
        /// </summary>
        static AnnotationFilterOperatorHandler()
        {
            CreateAndRegisterCustomFunctionOperators();
        }

        /// <summary>
        /// Instanciates a new <see cref="AnnotationFilterOperatorHandler"/>
        /// </summary>
        /// <param name="rowViewModels">
        /// The <see cref="IEnumerable{T}"/> of type <see cref="IRowViewModelBase{Thing}"/>
        /// that contains rows where to collect data for.
        /// </param>
        /// <param name="fieldName">
        /// The name of the property (fieldname from a column's perspective) we want to collect the data from.
        /// </param>
        public AnnotationFilterOperatorHandler(IEnumerable<IRowViewModelBase<Thing>> rowViewModels, string fieldName)
            : base(rowViewModels, fieldName)
        {
        }

        /// <summary>
        /// Set the <see cref="FilterEditorQueryOperatorsEventArgs.Operators"/> property.
        /// </summary>
        /// <param name="filterEditorQueryOperatorsEventArgs">
        /// The <see cref="FilterEditorQueryOperatorsEventArgs"/>.
        /// </param>
        public override void SetOperators(FilterEditorQueryOperatorsEventArgs filterEditorQueryOperatorsEventArgs)
        {
            filterEditorQueryOperatorsEventArgs.Operators.Clear();

            // the first four take no value and answer whether the item carries one at all; the last three take a
            // kind, so change requests and model notes can be filtered separately
            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(HasAnyAnnotationName) { Caption = "Has Any Annotation" });

            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(HasNoAnnotationName) { Caption = "Has No Annotation" });

            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(HasOpenAnnotationName) { Caption = "Has Open Annotation" });

            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(HasClosedAnnotationName) { Caption = "Has Closed Annotation" });

            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(HasAnnotationOfKindName) { Caption = "Has Annotation of Kind" });

            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(HasOpenAnnotationOfKindName) { Caption = "Has Open Annotation of Kind" });

            filterEditorQueryOperatorsEventArgs.Operators.Add(
                new FilterEditorOperatorItem(HasClosedAnnotationOfKindName) { Caption = "Has Closed Annotation of Kind" });
        }

        /// <summary>
        /// Get the values to be used in the <see cref="FilterEditorControl"/>s combobox
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{Object}"/> containing every annotation kind
        /// </returns>
        /// <remarks>
        /// Offers every kind the model allows rather than only the kinds already present, so a filter can be built
        /// before anything has been raised and does not change shape as annotations come and go.
        /// </remarks>
        public override IEnumerable<object> GetValues()
        {
            return AnnotationKind.All
                .Select(kind => kind.Name)
                .OrderBy(x => x)
                .ToList();
        }

        /// <summary>
        /// Creates and registers the custom functions used to filter an annotation column, which is of type
        /// <see cref="IEnumerable{T}"/> of <see cref="EngineeringModelDataAnnotation"/>.
        /// </summary>
        private static void CreateAndRegisterCustomFunctionOperators()
        {
            CriteriaOperator.RegisterCustomFunction(CustomFunctionFactory.Create(
                HasAnyAnnotationName,
                (IEnumerable<EngineeringModelDataAnnotation> annotations) => annotations?.Any() ?? false));

            CriteriaOperator.RegisterCustomFunction(CustomFunctionFactory.Create(
                HasNoAnnotationName,
                (IEnumerable<EngineeringModelDataAnnotation> annotations) => !(annotations?.Any() ?? false)));

            CriteriaOperator.RegisterCustomFunction(CustomFunctionFactory.Create(
                HasOpenAnnotationName,
                (IEnumerable<EngineeringModelDataAnnotation> annotations) => annotations?.Any(AnnotationQuery.IsOpen) ?? false));

            // deliberately not the negation of the one above: an item with nothing raised against it is not closed
            // out, it simply has no annotations, and lumping those together makes the filter useless for a review
            CriteriaOperator.RegisterCustomFunction(CustomFunctionFactory.Create(
                HasClosedAnnotationName,
                (IEnumerable<EngineeringModelDataAnnotation> annotations) => annotations?.Any(IsClosed) ?? false));

            CriteriaOperator.RegisterCustomFunction(CustomFunctionFactory.Create(
                HasAnnotationOfKindName,
                (IEnumerable<EngineeringModelDataAnnotation> annotations, string kindName) =>
                    annotations?.Any(x => IsKind(x, kindName)) ?? false));

            CriteriaOperator.RegisterCustomFunction(CustomFunctionFactory.Create(
                HasOpenAnnotationOfKindName,
                (IEnumerable<EngineeringModelDataAnnotation> annotations, string kindName) =>
                    annotations?.Any(x => IsKind(x, kindName) && AnnotationQuery.IsOpen(x)) ?? false));

            CriteriaOperator.RegisterCustomFunction(CustomFunctionFactory.Create(
                HasClosedAnnotationOfKindName,
                (IEnumerable<EngineeringModelDataAnnotation> annotations, string kindName) =>
                    annotations?.Any(x => IsKind(x, kindName) && IsClosed(x)) ?? false));
        }

        /// <summary>
        /// Asserts whether an annotation is of the named kind.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <param name="kindName">The kind name as offered in the filter editor.</param>
        /// <returns>true when the kinds match.</returns>
        private static bool IsKind(EngineeringModelDataAnnotation annotation, string kindName)
        {
            return AnnotationKind.Describe(annotation).Equals(kindName);
        }

        /// <summary>
        /// Asserts whether an annotation has been closed out, that is, it carries a status and that status is not
        /// one that still needs action. A model note carries no status and is therefore never closed.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>true when it is closed out.</returns>
        private static bool IsClosed(EngineeringModelDataAnnotation annotation)
        {
            return annotation is ModellingAnnotationItem && !AnnotationQuery.IsOpen(annotation);
        }
    }
}
