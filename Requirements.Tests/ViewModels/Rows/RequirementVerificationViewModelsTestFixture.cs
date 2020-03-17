// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementVerificationViewModels.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ViewModels.Rows
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using System.Threading.Tasks;

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
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Requirement requirement;
        private ParametricConstraint parametricConstraint;
        private ParametricConstraintRowViewModel parametricConstraintRowViewModel;
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private DomainOfExpertise domain;

        [SetUp]
        public void Setup()
        {
            this.assembler = new Assembler(this.uri);

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);

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
            CDPMessageBus.Current.ClearSubscriptions();
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

            CDPMessageBus.Current.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Calculating), thing);
            Assert.AreEqual(RequirementStateOfCompliance.Calculating, row.RequirementStateOfCompliance);

            CDPMessageBus.Current.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Pass), thing);
            Assert.AreEqual(RequirementStateOfCompliance.Pass, row.RequirementStateOfCompliance);

            if (thingType == typeof(RelationalExpression) && row is IHaveContainerViewModel)
            {
                CDPMessageBus.Current.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Unknown), typeof(ParameterOrOverrideBase));
                Assert.AreEqual(RequirementStateOfCompliance.Unknown, row.RequirementStateOfCompliance);
                Assert.AreEqual(RequirementStateOfCompliance.Unknown, this.parametricConstraintRowViewModel.RequirementStateOfCompliance);
            }
        }

        [Test]
        public void VerifyThatRequirementVerificationStateOfComplianceIsSetForParametricConstraintRowViewModel()
        {
            CDPMessageBus.Current.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Calculating), this.parametricConstraint);
            Assert.AreEqual(RequirementStateOfCompliance.Calculating, this.parametricConstraintRowViewModel.RequirementStateOfCompliance);

            CDPMessageBus.Current.SendMessage(new RequirementStateOfComplianceChangedEvent(RequirementStateOfCompliance.Pass), this.parametricConstraint);
            Assert.AreEqual(RequirementStateOfCompliance.Pass, this.parametricConstraintRowViewModel.RequirementStateOfCompliance);
        }
    }
}
