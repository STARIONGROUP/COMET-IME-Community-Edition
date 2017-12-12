// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;
    using System.Windows;
    using BasicRdl.ViewModels;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Dal;
    using CDP4Dal.Events;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// suite of tests for the <see cref="CategoryRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class CategoryRowViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        private CategoryRowViewModel rowViewModel;

        private Assembler assembler;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Returns(Task.FromResult("some result"));
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheConstructorSetsTheProperties()
        {
            var name = "name";
            var shortname = "shortname";
            var containerRdl = "containerrdl";
            
            var category = new Category(Guid.NewGuid(), null, null) { ShortName = shortname, Name = name };
            var container = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { ShortName = "containerrdl" };
            container.DefinedCategory.Add(category);

            this.rowViewModel = new CategoryRowViewModel(category, this.session.Object, null);

            Assert.AreEqual(name, this.rowViewModel.Name);
            Assert.AreEqual(shortname, this.rowViewModel.ShortName);
            Assert.AreEqual(containerRdl, this.rowViewModel.ContainerRdl);
            Assert.AreEqual(string.Empty, this.rowViewModel.SuperCategories);
        }

        [Test]
        public void VerifyThatSuperCategoriesAreAsExpected()
        {
            var category = new Category(Guid.NewGuid(), null, null);
            var superCategory_1 = new Category(Guid.NewGuid(), null, null) { ShortName = "supercategory_1" };
            var superCategory_2 = new Category(Guid.NewGuid(), null, null) {  ShortName = "supercategory_2" };

            var container = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { ShortName = "containerrdl"};
            container.DefinedCategory.Add(category);
            container.DefinedCategory.Add(superCategory_1);
            container.DefinedCategory.Add(superCategory_2);

            this.rowViewModel = new CategoryRowViewModel(category, this.session.Object, null);

            Assert.AreEqual(string.Empty, this.rowViewModel.SuperCategories);

            var category_1 = new Category(Guid.NewGuid(), null, null);
            container.DefinedCategory.Add(category_1);
            category_1.SuperCategory.Add(superCategory_1);

            this.rowViewModel = new CategoryRowViewModel(category_1, this.session.Object, null);
            Assert.AreEqual("{supercategory_1}", this.rowViewModel.SuperCategories);

            var category_2 = new Category(Guid.NewGuid(), null, null);
            container.DefinedCategory.Add(category_2);
            category_2.SuperCategory.Add(superCategory_1);
            category_2.SuperCategory.Add(superCategory_2);

            this.rowViewModel = new CategoryRowViewModel(category_2, this.session.Object, null);
            Assert.AreEqual("{supercategory_1, supercategory_2}", this.rowViewModel.SuperCategories);
        }

        [Test]
        public void VerifyThatWhenCategoryIsUpdateViewModelIsUpdate()
        {
            var name = "name";
            var shortname = "shortname";
            var containerRdl = "containerrdl";

            var category = new Category(Guid.NewGuid(), null, null) { ShortName = shortname, Name = name };
            var container = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { ShortName = "containerrdl"}; 
            container.DefinedCategory.Add(category);

            this.rowViewModel = new CategoryRowViewModel(category, this.session.Object, null);

            category.Name = "updated name";
            category.ShortName = "updated shortname";
            container.ShortName = "updated containerrdl";

            // workaround to modify a read-only field
            var type = category.GetType();
            type.GetProperty("RevisionNumber").SetValue(category, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(category, EventKind.Updated);

            Assert.AreEqual("updated shortname", this.rowViewModel.ShortName);
            Assert.AreEqual("updated name", this.rowViewModel.Name);
            Assert.AreEqual("updated containerrdl", this.rowViewModel.ContainerRdl);
        }

        [Test]
        public void VerifyThatWhenContainerRdlIsUpdatedViewModelIsUpdated()
        {
            var name = "name";
            var shortname = "shortname";
            var containerRdl = "containerrdl";

            var category = new Category(Guid.NewGuid(), this.assembler.Cache, null) { ShortName = shortname, Name = name };
            var container = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, null) { ShortName = containerRdl };
            container.DefinedCategory.Add(category);

            this.rowViewModel = new CategoryRowViewModel(category, this.session.Object, null);
            Assert.AreEqual(containerRdl, this.rowViewModel.ContainerRdl);

            container.ShortName = "updated containerrdl";
            CDPMessageBus.Current.SendObjectChangeEvent(container, EventKind.Updated);
            Assert.AreEqual("updated containerrdl", this.rowViewModel.ContainerRdl);
        }

        [Test]
        public void VerifyThatStartDragOnCategoryRowReturnsExpectedPayloadAndEffect()
        {
            var dragInfo = new Mock<IDragInfo>();
            dragInfo.SetupProperty(x => x.Payload);
            dragInfo.SetupProperty(x => x.Effects);

            var name = "name";
            var shortname = "shortname";
            
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, null) { ShortName = shortname, Name = name };
            this.rowViewModel = new CategoryRowViewModel(category, this.session.Object, null);

            this.rowViewModel.StartDrag(dragInfo.Object);
            
            Assert.AreEqual(category, dragInfo.Object.Payload);
            Assert.AreEqual(DragDropEffects.Copy, dragInfo.Object.Effects);
        }
    }
}
