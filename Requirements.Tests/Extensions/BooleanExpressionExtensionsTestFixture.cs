// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BooleanExpressionExtensionsTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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

namespace CDP4Requirements.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4Requirements.Extensions;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Tests for the <see cref="CDP4Common.Extensions.BooleanExpressionExtensions"/> class
    /// </summary>
    [TestFixture]
    internal class BooleanExpressionExtensionsTestFixture
    {
        private Mock<ISession> session;
        private FakeViewModelBase viewModelBase;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            this.viewModelBase = new FakeViewModelBase(this.session.Object);
        }

        [Test]
        [TestCaseSource(nameof(GetClassesForAllBooleanExpressionTypes))]
        public void VerifyThatAllAvailableBooleanExpressionsReturnAViewModel(BooleanExpression booleanExpression)
        {
            Assert.IsNotNull(booleanExpression.GetBooleanExpressionViewModel(this.viewModelBase));
        }

        /// <summary>
        /// Fake ViewModelBase, because we can't set a ISession
        /// </summary>
        private class FakeViewModelBase : IViewModelBase<Thing>, IISession
        {
            public FakeViewModelBase(ISession session)
            {
                this.Session = session;
            }

            /// <summary>
            /// Gets the <see cref="Thing"/> that is represented by the current <see cref="IViewModelBase{T}"/>
            /// </summary>
            public Thing Thing { get; }

            /// <summary>
            /// Gets the <see cref="ICDPMessageBus"/>
            /// </summary>
            public ICDPMessageBus CDPMessageBus { get; }

            /// <summary>
            /// Dispose of the <see cref="IDisposable"/> objects
            /// </summary>
            public void Dispose()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets the <see cref="T:CDP4Dal.ISession" />
            /// </summary>
            public ISession Session { get; }
        }

        /// <summary>
        /// Gets all classes that are inherited from <see cref="BooleanExpression"/>
        /// </summary>
        /// <returns>The <see cref="IEnumerable{BooleanExpression}"/></returns>
        public static IEnumerable<BooleanExpression> GetClassesForAllBooleanExpressionTypes()
        {
            foreach (var type in
                     Assembly.GetAssembly(typeof(BooleanExpression)).GetTypes()
                         .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BooleanExpression))))
            {
                yield return (BooleanExpression)Activator.CreateInstance(type);
            }
        }
    }
}
