// -------------------------------------------------------------------------------------------------
// <copyright file="DefinitionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests
{
    using System;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;    
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Operations;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using ReactiveUI;
    using CDP4CommonView.ViewModels;
    using CDP4Dal.DAL;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DefinitionDialogViewModelTestFixture"/>
    /// </summary>
    [TestFixture]
    public class DefinitionDialogViewModelTestFixture
    {
        private DefinitionDialogViewModel viewmodel;
        private Definition simpleDefinition;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private SiteDirectory siteDirectory;

        private Assembler assembler;
        private Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);

            this.simpleDefinition = new Definition(Guid.NewGuid(), null, null) { LanguageCode = "es-ES", Content = "Definition" };
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);
            
            this.simpleDefinition.Note.Add("Note0");
            this.simpleDefinition.Example.Add("Note0");

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            this.transaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
        }

        /// <summary>
        /// Basic method to test creating an empty <see cref="DefinitionDialogViewModel"/>
        /// </summary>
        [Test]
        public void VerifyCreateNewEmptyDefinitionDialogViewModel()
        {
            this.viewmodel = new DefinitionDialogViewModel();
            Assert.IsNotNull(this.viewmodel);            
        }

        /// <summary>
        /// Basic method to test creating a <see cref="DefinitionDialogViewModel"/>
        /// </summary>
        [Test]
        public void VerifyCreateNewDefinitionDialogViewModel()
        {
            var group = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.assembler.Cache.TryAdd(new CacheKey(group.Iid, null), new Lazy<Thing>(() => group));

            var clone = group.Clone(false);
            clone.Definition.Add(this.simpleDefinition);

            this.transaction.CreateOrUpdate(clone);

            this.viewmodel = new DefinitionDialogViewModel(this.simpleDefinition, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone, null);
            Assert.IsNotNull(this.viewmodel);

            //Test Note features
            this.viewmodel.CreateNoteCommand.Execute(null);
            Assert.AreEqual(this.viewmodel.Note.Count,2);
            this.viewmodel.SelectedNote = this.viewmodel.Note[0];
            Assert.IsTrue(this.viewmodel.SelectedNote.Value.Equals(this.simpleDefinition.Note[0]));
            this.viewmodel.DeleteNoteCommand.Execute(null);
            Assert.AreEqual(this.viewmodel.Note.Count, 1);

            //Test Example features
            this.viewmodel.CreateExampleCommand.Execute(null);
            Assert.AreEqual(this.viewmodel.Example.Count, 2);
            this.viewmodel.SelectedExample = this.viewmodel.Example[0];
            Assert.IsTrue(this.viewmodel.SelectedExample.Value.Equals(this.simpleDefinition.Example[0]));
            this.viewmodel.DeleteExampleCommand.Execute(null);
            Assert.AreEqual(this.viewmodel.Example.Count, 1);

        }

        [Test]
        public void VerifyThatAnUnkownLanguageCodeCanBeLoaded()
        {
            var languageCode = "F6F40215-560D-4104-93E1-6452769FDACC";
            var content = "content in an unknown language";

            var definition = new Definition() { LanguageCode = languageCode, Content = content };

            var requirement = new Requirement();            
            var clone = requirement.Clone(false);
            clone.Definition.Add(definition);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, null);
            transaction.CreateOrUpdate(clone);

            var vm = new DefinitionDialogViewModel(definition, transaction, this.session.Object, true, ThingDialogKind.Create, null, clone, null);

            Assert.AreEqual(vm.SelectedLanguageCode.Name, languageCode);
            Assert.AreEqual(vm.Content, content);
        }

        [Test]
        public void VerifyThatDialogViewModelCanLoadIfLanguageCodeIsNull()
        {
            var definition = new Definition() { LanguageCode = null, Content = null };

            var requirement = new Requirement();
            var clone = requirement.Clone(false);
            clone.Definition.Add(definition);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            var transaction = new ThingTransaction(transactionContext, null);
            transaction.CreateOrUpdate(clone);
            
            Assert.DoesNotThrow(() => new DefinitionDialogViewModel(definition, transaction, this.session.Object, true, ThingDialogKind.Create, null, clone, null));
        }
    }
}
