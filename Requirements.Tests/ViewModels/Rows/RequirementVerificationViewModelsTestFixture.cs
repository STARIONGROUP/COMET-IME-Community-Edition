// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementVerificationViewModelsTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.ViewModels.Rows
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4Requirements.Extensions;
    using CDP4Requirements.ViewModels;
    using CDP4Requirements.ViewModels.RequirementBrowser;

    using CDP4RequirementsVerification;
    using CDP4RequirementsVerification.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Tests several different ViewModels to check if <see cref="RequirementStateOfCompliance"/> is set correctly
    /// </summary>
    [TestFixture]
    internal class RequirementVerificationViewModels
    {
        private Mock<ISession> session;
        private readonly PropertyInfo revision = typeof(Thing).GetProperty("RevisionNumber");
        private readonly Uri uri = new Uri("https://www.stariongroup.eu");
        private Requirement requirement;
        private ParametricConstraint parametricConstraint;
        private ParametricConstraintRowViewModel parametricConstraintRowViewModel;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private DomainOfExpertise domain;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "test" };

            this.requirement = new Requirement
            {
                Owner = this.domain
            };

            this.parametricConstraint = new ParametricConstraint { Container = this.requirement };
            this.parametricConstraintRowViewModel = new ParametricConstraintRowViewModel(this.parametricConstraint, this.session.Object, null);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [TestCase(typeof(OrExpression))]
        [TestCase(typeof(RelationalExpression))]
        [TestCase(typeof(NotExpression))]
        [TestCase(typeof(ExclusiveOrExpression))]
        [TestCase(typeof(AndExpression))]
        public void VerifyThatRequirementVerificationStateOfComplianceIsSetForBooleanExpressionRowViewModels(Type thingType)
        {
            var thing = Activator.CreateInstance(thingType);
            IHaveWritableRequirementStateOfCompliance row = null;

            if (thing is BooleanExpression booleanExpressionRow)
            {
                row = booleanExpressionRow.GetBooleanExpressionViewModel(this.parametricConstraintRowViewModel) as IHaveWritableRequirementStateOfCompliance;

                if (row is IRowViewModelBase<Thing> rowViewModelBase)
                {
                    this.parametricConstraintRowViewModel.ContainedRows.Add(rowViewModelBase);
                }
            }

            Assert.IsNotNull(row);

            this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Calculating), thing);
            Assert.AreEqual(RequirementStateOfCompliance.Calculating, row.RequirementStateOfCompliance);

            this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Pass), thing);
            Assert.AreEqual(RequirementStateOfCompliance.Pass, row.RequirementStateOfCompliance);

            if (thingType == typeof(RelationalExpression) && row is IHaveContainerViewModel)
            {
                this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Unknown), typeof(ParameterOrOverrideBase));
                Assert.AreEqual(RequirementStateOfCompliance.Unknown, row.RequirementStateOfCompliance);
                Assert.AreEqual(RequirementStateOfCompliance.Unknown, this.parametricConstraintRowViewModel.RequirementStateOfCompliance);
            }
        }

        [Test]
        public void VerifyThatRequirementVerificationStateOfComplianceIsSetForParametricConstraintRowViewModel()
        {
            this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Calculating), this.parametricConstraint);
            Assert.AreEqual(RequirementStateOfCompliance.Calculating, this.parametricConstraintRowViewModel.RequirementStateOfCompliance);

            this.messageBus.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Pass), this.parametricConstraint);
            Assert.AreEqual(RequirementStateOfCompliance.Pass, this.parametricConstraintRowViewModel.RequirementStateOfCompliance);
        }
    }
}
