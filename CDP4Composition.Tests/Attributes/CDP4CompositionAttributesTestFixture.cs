// -------------------------------------------------------------------------------------------------
// <copyright file="CDP4CompositionAttributesTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Attributes
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using NUnit.Framework;

    [TestFixture]
    public class CDP4CompositionAttributesTestFixture
    {
        [Test]
        public void TestDescriptionExport()
        {
            var attributes = new DescribeExportAttribute("name", "description", typeof(IPanelViewModel));
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
            Assert.AreEqual(typeof(IPanelViewModel), attributes.ContractType);

            attributes = new DescribeExportAttribute("name", "description", typeof(IDialogView));
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
            Assert.AreEqual(typeof(IDialogView), attributes.ContractType);
        }

        [Test]
        public void TestDialogViewExport()
        {
            var attributes = new DialogViewExportAttribute("name", "description");
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
        }

        [Test]
        public void TestDialogViewModelExport()
        {
            var attributes = new DialogViewModelExportAttribute("name", "description");
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
        }

        [Test]
        public void TestModuleExportName()
        {
            var attributes = new ModuleExportNameAttribute(typeof (TestClass), "name");
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual(typeof(TestClass), attributes.ModuleType);
        }

        [Test]
        public void TestPanelViewModelExport()
        {
            var attributes = new PanelViewModelExportAttribute("name", "description");
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
            Assert.AreEqual(typeof(IPanelViewModel), attributes.ContractType);
        }
    }

    public class TestClass
    {}
}


