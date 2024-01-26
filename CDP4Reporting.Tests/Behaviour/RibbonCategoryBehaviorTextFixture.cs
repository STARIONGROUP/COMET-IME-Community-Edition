// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonCategoryBehaviorTextFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Reporting.Tests.Behaviour
{
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Composition.Mvvm.Behaviours;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RibbonCategoryBehaviorTextFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private RibbonCategoryBehavior ribbonCategoryBehavior;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.ribbonCategoryBehavior = new RibbonCategoryBehavior();
        }

        [Test]
        public void VerifyInitializedMembers()
        {
            Assert.IsNull(this.ribbonCategoryBehavior.CategoryName);
        }

        [Test]
        public void VerifyTheDefaultValueOfTheDependencyPropertyContainer()
        {
            Assert.AreEqual("CategoryName", RibbonCategoryBehavior.CategoryNameProperty.Name);
        }

        [Test]
        public void VerifyIfTheCategoryNameIsChanged()
        {
            Assert.IsNull(this.ribbonCategoryBehavior.CategoryName);
            RibbonCategoryBehavior.SetCategoryName(this.ribbonCategoryBehavior, "abc");
            Assert.AreEqual("abc", this.ribbonCategoryBehavior.CategoryName);
        }
    }
}
