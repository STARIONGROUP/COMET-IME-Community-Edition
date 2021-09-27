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
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.Services;
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
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingCreator> thingCreator;
        private Uri uri;
        private Iteration iteration;
        private ModelReferenceDataLibrary modelReferenceDataLibrary;
        private Mock<IObfuscationService> obfuscationService;
        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);
            this.obfuscationService = new Mock<IObfuscationService>();

            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.thingCreator = new Mock<IThingCreator>();

            var siteDirectory = new SiteDirectory();
            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var iterationsetup = new IterationSetup();
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) {EngineeringModelSetup = engineeringModelSetup};            
            engineeringModelSetup.IterationSetup.Add(iterationsetup);

            var siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            siteDirectory.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary);
            this.modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) {RequiredRdl = siteReferenceDataLibrary};
            engineeringModelSetup.RequiredRdl.Add(this.modelReferenceDataLibrary);
            
            var person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri) { GivenName = "test", Surname = "test" };
            var participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = person };
            engineeringModelSetup.Participant.Add(participant);
            
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri) {IterationSetup = iterationsetup};
            engineeringModel.Iteration.Add(this.iteration);
            siteDirectory.Model.Add(engineeringModelSetup);

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
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null, this.obfuscationService.Object);
            row.ThingCreator = this.thingCreator.Object;
            
            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var ratioScale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri);
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
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null, this.obfuscationService.Object);
            row.ThingCreator = new TestThingCreatorThatThrowsExceptions();
            
            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var ratioScale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.uri);
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
            var iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            iteration.Element.Add(elementDefinition);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null, this.obfuscationService.Object);

            var payload = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
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

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = domainOfExpertise };
            this.iteration.Element.Add(elementDefinition);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null, this.obfuscationService.Object);
            row.ThingCreator = this.thingCreator.Object;

            var payload = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
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

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = domainOfExpertise };
            this.iteration.Element.Add(elementDefinition);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null, this.obfuscationService.Object);
            row.ThingCreator = new TestThingCreatorThatThrowsExceptions();

            var payload = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
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

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) {ShortName = "PROD", Name = "Products"};
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.modelReferenceDataLibrary.DefinedCategory.Add(category);

            this.iteration.Element.Add(elementDefinition);

            var browser = new ElementDefinitionsBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, null, this.dialogNavigationService.Object, null, null, null);
            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, browser, this.obfuscationService.Object);

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(category);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.That(dropInfo.Object.Effects, Is.EqualTo(DragDropEffects.Copy));
            
            row.Drop(dropInfo.Object);
            Assert.IsFalse(row.HasError);
        }

        [Test]
        public void VerifyThatDragCategoryWithoutElementDefinitionClassKindSetsNoneEffect()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            category.PermissibleClass.Add(ClassKind.Parameter);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null, this.obfuscationService.Object);

            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(category);
            dropInfo.SetupProperty(x => x.Effects);

            row.DragOver(dropInfo.Object);

            Assert.AreEqual(dropInfo.Object.Effects, DragDropEffects.None);
        }

        [Test]
        public void VeriftyThatDragAlreadyCategorizedCategorySetsNoneEffect()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            elementDefinition.Category.Add(category);

            this.iteration.Element.Add(elementDefinition);

            this.modelReferenceDataLibrary.DefinedCategory.Add(category);

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null, this.obfuscationService.Object);

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

            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.Owner = domainOfExpertise;
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            elementDefinition.Category.Add(category);
            var group = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            elementDefinition.ParameterGroup.Add(group);
            this.iteration.Element.Add(elementDefinition);
            this.assembler.Cache.TryAdd(new CacheKey(group.Iid, this.iteration.Iid), new Lazy<Thing>(() => group));

            var row = new ElementDefinitionRowViewModel(elementDefinition, domainOfExpertise, this.session.Object, null, this.obfuscationService.Object);

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
        /// Apply <see cref="Category"/> to <see cref="ICategorizableThing"/>
        /// </summary>
        /// <param name="category">
        /// The <see cref="Category"/> to apply
        /// </param>
        /// <param name="categorizableThing">
        /// The <see cref="ICategorizableThing"/> that the category is applied to.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the new <see cref="Category"/> application is to be added to
        /// </param>
        public Task ApplyCategory<T>(Category category, T categorizableThing, ISession session) where T : Thing, ICategorizableThing
        {
            throw new Exception("The category could not be applied");
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

        /// <summary>
        /// Create a new <see cref="ElementUsage"/> with InterfaceEndKind
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
        /// <param name="interfaceEndKind">The <see cref="InterfaceEndKind"/> to create with</param>
        public Task CreateElementUsage(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session, InterfaceEndKind interfaceEndKind)
        {
            throw new Exception();
        }

        /// <summary>
        /// Method for creating a <see cref="BinaryRelationship"/> between a <see cref="ParameterOrOverrideBase"/> and a <see cref="RelationalExpression"/>.
        /// </summary>
        /// <param name="session">The <see cref="Session"/> for which the <see cref="BinaryRelationship"/> will be created</param>
        /// <param name="iteration">The <see cref="Iteration"/> for which the  <see cref="BinaryRelationship"/> will be created</param>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/> that acts as the source of the <see cref="BinaryRelationship"/></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/> that acts as the target of the <see cref="BinaryRelationship"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public Task CreateBinaryRelationshipForRequirementVerification(ISession session, Iteration iteration, ParameterOrOverrideBase parameter, RelationalExpression relationalExpression)
        {
            throw new Exception("The Binary Relationship could not be created");
        }

        /// <summary>
        /// Checks if creating a <see cref="BinaryRelationship"/> is allowed for these two objects
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="relationalExpression">The <see cref="RelationalExpression"/></param>
        /// <returns>True if creation is allowed</returns>
        public bool IsCreateBinaryRelationshipForRequirementVerificationAllowed(ParameterOrOverrideBase parameter, RelationalExpression relationalExpression)
        {
            return false;
        }

        public Task<ElementUsage> CreateAndGetElementUsage(ElementDefinition container, ElementDefinition referencedDefinition, DomainOfExpertise owner, ISession session)
        {
            return Task.FromResult(default(ElementUsage));
        }

        /// <summary>
        /// Create and return a new Interface between two <see cref="ElementUsage"/>
        /// </summary>
        /// <param name="output">
        /// The source <see cref="ElementUsage"/> of the interface
        /// </param>
        /// <param name="input">
        /// The target <see cref="ElementUsage"/> of the interface
        /// </param>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of the <see cref="ElementUsage"/> that is to be created.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Parameter"/> is to be added
        /// </param>
        public Task<BinaryRelationship> CreateAndGetInterface(ElementUsage output, ElementUsage input, Iteration iteration, DomainOfExpertise owner, ISession session)
        {
            throw new Exception();
        }
    }
}
