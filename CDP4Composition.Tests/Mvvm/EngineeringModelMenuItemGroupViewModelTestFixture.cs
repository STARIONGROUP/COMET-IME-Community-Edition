namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using System.Reactive.Concurrency;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class EngineeringModelMenuItemGroupViewModelTestFixture
    {
        private Uri uri;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private Mock<ISession> session;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.be");

            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model" };
            this.model.EngineeringModelSetup = this.modelSetup;

            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri) { IterationNumber =  5 };
            this.iteration.IterationSetup = this.iterationSetup;

            this.model.Iteration.Add(this.iteration);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void Test()
        {
            var item = new EngineeringModelMenuGroupViewModel(this.iteration, this.session.Object);

            Assert.IsTrue(item.Name.Contains(this.modelSetup.Name));

            this.modelSetup.Name = "model1";
            // workaround to modify a read-only field
            var type = this.model.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.model, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(this.model, EventKind.Updated);

            Assert.IsTrue(item.Name.Contains(this.modelSetup.Name));
        }

    }
}