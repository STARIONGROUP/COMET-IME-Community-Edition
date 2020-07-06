using NUnit.Framework;

namespace CDP4Grapher.Tests.ViewModels
{
    using CDP4Grapher.ViewModels;

    [TestFixture]
    public class GrapherBrowserRibbonViewModelTestFixture
    {
        [Test]
        public void Verify()
        {
            var vm = new GrapherBrowserRibbonViewModel();
            Assert.IsNotEmpty(vm.Sessions);
        }
    }
}
