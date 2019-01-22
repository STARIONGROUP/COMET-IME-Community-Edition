// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.ElementDefinitionTreeRows
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.Utilities;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    using Rule = System.Data.Rule;

    /// <summary>
    /// Suite of tests for the <see cref="ElementDefinitionRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class ElementDefinitionRowViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;
        
        /// <summary>
        /// A mock of the <see cref="IPermissionService"/>
        /// </summary>
        private Mock<IPermissionService> permissionService;

        /// <summary>
        /// A mock of <see cref="IThingDialogNavigationService"/>
        /// </summary>
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Mock<IDialogNavigationService> dialogNavigationService;

        /// <summary>
        /// A mock of <see cref="IThingCreator"/>
        /// </summary>
        private Mock<IThingCreator> thingCreator;
        
        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        /// <summary>
        /// The iteration.
        /// </summary>
        private Iteration iteration;

        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");

            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.thingCreator = new Mock<IThingCreator>();

            var sitedir = new SiteDirectory();
            var iterationsetup = new IterationSetup();
            var engModel = new EngineeringModel(Guid.NewGuid(), null, null);
            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            modelSetup.IterationSetup.Add(iterationsetup);

            var person = new Person(Guid.NewGuid(), null, null) { GivenName = "test", Surname = "test" };
            var participant = new Participant(Guid.NewGuid(), null, null) { Person = person };
            modelSetup.Participant.Add(participant);
            engModel.EngineeringModelSetup = modelSetup;
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri) {IterationSetup = iterationsetup};
            engModel.Iteration.Add(this.iteration);
            sitedir.Model.Add(modelSetup);

            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { {this.iteration, new Tuple<DomainOfExpertise, Participant>(null, participant)}});
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatParameterGetsCreatedWhenParameterTypeIsDropped()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null);
            row.ThingCreator = this.thingCreator.Object;
            
            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;
            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.Drop(dropInfo.Object);
            
            this.thingCreator.Verify(x => x.CreateParameter(elementDefinition, null, simpleQuantityKind, ratioScale, domainOfExpertise, this.session.Object));
        }

        [Test]
        public void VerifyThatExceptionIsCaughtWhenParameterCouldNotBeCreatedOnDrop()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null);
            row.ThingCreator = new TestThingCreatorThatThrowsExceptions();
            
            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);
            simpleQuantityKind.DefaultScale = ratioScale;
            var payload = new Tuple<ParameterType, MeasurementScale>(simpleQuantityKind, ratioScale);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.Drop(dropInfo.Object);

            Assert.AreEqual("The parameter could not be created", row.ErrorMsg);          
        }
        
        [Test]
        public void VerifyThatDragElementDefinitionSetsCopyEffect()
        {
            var iteration = new Iteration(Guid.NewGuid(), null, null);
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            iteration.Element.Add(elementDefinition);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null);

            var payload = new ElementDefinition(Guid.NewGuid(), null, null);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(dropInfo.Object.Effects, DragDropEffects.None);
        }

        [Test]
        public void VerifyThatElementUsageGetsCreatedWhenElementDefinitionIsDropped()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise();
            var elementDefinition = new ElementDefinition { Owner = domainOfExpertise };
            this.iteration.Element.Add(elementDefinition);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null);
            row.ThingCreator = this.thingCreator.Object;

            var payload = new ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(payload);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.Drop(dropInfo.Object);

            this.thingCreator.Verify(x => x.CreateElementUsage(elementDefinition, payload, domainOfExpertise, this.session.Object));
        }

        [Test]
        public void VerifyThatExceptionIsCaughtWhenElementUsageCouldNotBeCreatedOnDrop()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise();
            var elementDefinition = new ElementDefinition { Owner = domainOfExpertise };
            this.iteration.Element.Add(elementDefinition);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null);
            row.ThingCreator = new TestThingCreatorThatThrowsExceptions();

            var payload = new ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(payload);
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(payload);
            dropInfo.SetupProperty(x => x.Effects);

            row.Drop(dropInfo.Object);

            Assert.AreEqual("The Element Usage could not be created", row.ErrorMsg);
        }

        [Test]
        public void VerifyThatDragCategorySetsCopyEffectAndCanBeDropped()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            var category = new Category(Guid.NewGuid(), null, null);
            category.PermissibleClass.Add(ClassKind.ElementDefinition);

            this.iteration.Element.Add(elementDefinition);

            var browser = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, null, this.dialogNavigationService.Object);
            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, browser);

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(category);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(dropInfo.Object.Effects, DragDropEffects.Copy);

            row.Drop(dropInfo.Object);
            Assert.IsFalse(row.HasError);
        }

        [Test]
        public void VerifyThatDragCategoryWithoutElementDefinitionClassKindSetsNoneEffect()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            var category = new Category(Guid.NewGuid(), null, null);
            category.PermissibleClass.Add(ClassKind.Parameter);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null);

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(category);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(dropInfo.Object.Effects, DragDropEffects.None);
        }

        [Test]
        public void VeriftyThatDragAlreadyCategorizedCategorySetsNoneEffect()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            var category = new Category(Guid.NewGuid(), null, null);
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            elementDefinition.Category.Add(category);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null);

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(category);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(dropInfo.Object.Effects, DragDropEffects.None);
        }

        [Test]
        public void VerifyThatGroupMayBeDropped()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            elementDefinition.Owner = domainOfExpertise;
            var category = new Category(Guid.NewGuid(), null, null);
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            elementDefinition.Category.Add(category);
            var group = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(group);
            this.iteration.Element.Add(elementDefinition);
            this.assembler.Cache.TryAdd(new CacheKey(group.Iid, this.iteration.Iid), new Lazy<Thing>(() => group));

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null);

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(group);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);
            Assert.AreEqual(dropInfo.Object.Effects, DragDropEffects.Move);

            row.Drop(dropInfo.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
    }

    /// <summary>
    /// Implementation of <see cref="IThingCreator"/> that throws an exception on each method.
    /// </summary>
    /// <remarks>
    /// This class is used due to it not being clear how to use moq library to throw an exception 
    /// from an async method
    /// </remarks>
    internal class TestThingCreatorThatThrowsExceptions : IThingCreator
    {
        /// <summary>
        /// Create a new <see cref="Parameter"/>
        /// </summary>
        /// <param name="elementDefinition">
        /// The container <see cref="ElementDefinition"/> of the <see cref="Parameter"/> that is to be created.
        /// </param>
        /// <param name="group">
        /// The <see cref="ParameterGroup"/> that the <see cref="Parameter"/> is to be grouped in.
        /// </param>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/> that the new <see cref="Parameter"/> references
        /// </param>
        /// <param name="measurementScale">
        /// The <see cref="MeasurementScale"/> that the <see cref="Parameter"/> references in case the <see cref="ParameterType"/> is a <see cref="QuantityKind"/>
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of the <see cref="Parameter"/> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Parameter"/> is to be added
        /// </param>
        public Task CreateParameter(
            ElementDefinition elementDefinition,
            ParameterGroup @group,
            ParameterType parameterType,
            MeasurementScale measurementScale,
            DomainOfExpertise owner,
            ISession session)
        {
            throw new Exception("The parameter could not be created");
        }

        /// <summary>
        /// Create a new <see cref="UserRuleVerification"/>
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The container <see cref="RuleVerificationList"/> of the <see cref="UserRuleVerification"/> that is to be created.
        /// </param>
        /// <param name="rule">
        /// The <see cref="Rule"/> that the new <see cref="UserRuleVerification"/> references.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the new <see cref="UserRuleVerification"/> is to be added
        /// </param>
        public Task CreateUserRuleVerification(RuleVerificationList ruleVerificationList, CDP4Common.SiteDirectoryData.Rule rule, ISession session)
        {
            throw new Exception("The User Rule could not be created");
        }

        /// <summary>
        /// Create a new <see cref="BuiltInRuleVerification"/>
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The container <see cref="RuleVerificationList"/> of the <see cref="BuiltInRuleVerification"/> that is to be created.
        /// </param>
        /// <param name="name">
        /// The name for the <see cref="BuiltInRuleVerification"/>
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the new <see cref="UserRuleVerification"/> is to be added
        /// </param>
        public Task CreateBuiltInRuleVerification(RuleVerificationList ruleVerificationList, string name, ISession session)
        {
            throw new Exception("The Builtin Rule could not be created");
        }

        /// <summary>
        /// Create a new <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="container">
        /// The container <see cref="ElementDefinition"/> of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="referencedDefinition">
        /// The referenced <see cref="ElementDefinition"/> of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Parameter"/> is to be added
        /// </param>
        public Task CreateElementUsage(
            ElementDefinition container,
            ElementDefinition referencedDefinition,
            DomainOfExpertise owner,
            ISession session)
        {
            throw new Exception("The Element Usage could not be created");
        }
    }
}
