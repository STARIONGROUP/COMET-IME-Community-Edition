// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationFilterOperatorHandlerTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Annotations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.ReportingData;

    using CDP4Composition.FilterOperators;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using DevExpress.Data.Filtering;

    using NUnit.Framework;

    [TestFixture]
    public class AnnotationFilterOperatorHandlerTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private Assembler assembler;

        [SetUp]
        public void SetUp()
        {
            this.assembler = new Assembler(this.uri, new CDPMessageBus());

            // touching the type runs its static constructor, which is what registers the operators
            GC.KeepAlive(AnnotationFilterOperatorHandler.HasAnyAnnotationName);
        }

        [Test]
        public void VerifyThatEveryOperatorIsRegistered()
        {
            var names = new[]
            {
                AnnotationFilterOperatorHandler.HasAnyAnnotationName,
                AnnotationFilterOperatorHandler.HasNoAnnotationName,
                AnnotationFilterOperatorHandler.HasOpenAnnotationName,
                AnnotationFilterOperatorHandler.HasClosedAnnotationName,
                AnnotationFilterOperatorHandler.HasAnnotationOfKindName,
                AnnotationFilterOperatorHandler.HasOpenAnnotationOfKindName,
                AnnotationFilterOperatorHandler.HasClosedAnnotationOfKindName
            };

            new AnnotationFilterOperatorHandler(new List<IRowViewModelBase<Thing>>(), "AnnotationList");

            Assert.Multiple(() =>
            {
                foreach (var name in names)
                {
                    Assert.That(CriteriaOperator.GetCustomFunction(name), Is.Not.Null, $"{name} must be registered");
                }
            });
        }

        [Test]
        public void VerifyThatTheOperatorsSelectTheExpectedItems()
        {
            var openRid = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { Status = AnnotationStatusKind.OPEN };
            var closedRid = new ReviewItemDiscrepancy(Guid.NewGuid(), this.assembler.Cache, this.uri) { Status = AnnotationStatusKind.CLOSED };
            var wontFixRequest = new ChangeRequest(Guid.NewGuid(), this.assembler.Cache, this.uri) { Status = AnnotationStatusKind.WONTFIX };
            var note = new EngineeringModelDataNote(Guid.NewGuid(), this.assembler.Cache, this.uri);

            Assert.Multiple(() =>
            {
                // has / has not, regardless of kind or status
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasAnyAnnotationName, [note]), Is.True);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasAnyAnnotationName, []), Is.False);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasNoAnnotationName, []), Is.True);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasNoAnnotationName, [note]), Is.False);

                // open / closed
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasOpenAnnotationName, [openRid, closedRid]), Is.True);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasOpenAnnotationName, [closedRid]), Is.False);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasClosedAnnotationName, [closedRid]), Is.True);

                // WONTFIX is an exit without work, so it counts as closed out rather than still needing action
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasClosedAnnotationName, [wontFixRequest]), Is.True);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasOpenAnnotationName, [wontFixRequest]), Is.False);

                // a note has no status at all, so it is neither open nor closed
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasClosedAnnotationName, [note]), Is.False);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasOpenAnnotationName, [note]), Is.False);

                // change requests and model notes filter separately, which is the point of the kind operators
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasAnnotationOfKindName, [note], "Model Note"), Is.True);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasAnnotationOfKindName, [note], "Change Request"), Is.False);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasAnnotationOfKindName, [wontFixRequest], "Change Request"), Is.True);

                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasOpenAnnotationOfKindName, [openRid], "Review Item Discrepancy"), Is.True);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasOpenAnnotationOfKindName, [closedRid], "Review Item Discrepancy"), Is.False);
                Assert.That(Evaluate(AnnotationFilterOperatorHandler.HasClosedAnnotationOfKindName, [closedRid], "Review Item Discrepancy"), Is.True);
            });
        }

        [Test]
        public void VerifyGetValues()
        {
            var handler = new AnnotationFilterOperatorHandler(new List<IRowViewModelBase<Thing>>(), "AnnotationList");

            var values = handler.GetValues().Cast<string>().ToList();

            Assert.Multiple(() =>
            {
                // every kind is offered even with no rows, so a filter can be built before anything is raised
                Assert.That(values, Does.Contain("Change Request"));
                Assert.That(values, Does.Contain("Model Note"));
                Assert.That(values, Does.Contain("Review Item Discrepancy"));
                Assert.That(values, Is.Ordered);
            });
        }

        private static bool Evaluate(string functionName, IReadOnlyList<EngineeringModelDataAnnotation> annotations, string kindName = null)
        {
            var function = CriteriaOperator.GetCustomFunction(functionName);

            var arguments = kindName == null
                ? new object[] { annotations }
                : new object[] { annotations, kindName };

            return (bool)function.Evaluate(arguments);
        }
    }
}
