// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluentRibbonManagerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Xml;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    
    /// <summary>
    /// Suite of tests for the <see cref="FluentRibbonManager"/> class
    /// </summary>
    public class FluentRibbonManagerTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private FluentRibbonManager fluentRibbonManager;
        private RibbonPartTest ribbonPart;
        private string existingribbonpartid;
        private string nonexsitingribbonpartid;
        private string ribbonXml;        
        private const string content = "some content";

        [SetUp]
        public void SetUp()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.fluentRibbonManager = new FluentRibbonManager();

            this.existingribbonpartid = "test";
            this.ribbonXml = string.Format("<test id=\"{0}\">this is a piece of test ribbon xml</test>", this.existingribbonpartid);

            this.nonexsitingribbonpartid = "a non exsting control";

            this.ribbonPart = new RibbonPartTest(1, this.panelNavigationService.Object, null, null);
            this.fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);            
        }

        [Test]
        public void VerifyThatValidRibbonXmlIsReturned()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var stream = executingAssembly.GetManifestResourceStream("CDP4Composition.Tests.Resources.RibbonXml.customui14.xsd");

            var fluentXml = this.fluentRibbonManager.GetFluentXml();

            using (var stringReader = new StringReader(fluentXml))
            {
                using (var reader = XmlReader.Create(stringReader))
                {
                    while (reader.Read())
                    {
                    }
                }
            }

            Assert.IsNotNullOrEmpty(fluentXml);
        }

        [Test]
        public void VerifyThatRibbonXmlIsReturned()
        {
            Assert.AreEqual(this.ribbonXml, this.ribbonPart.RibbonXml);
        }

        [Test]
        public void VerifyThatOnActionIsInvoked()
        {
            this.fluentRibbonManager.OnAction(this.nonexsitingribbonpartid);
            Assert.IsFalse(this.ribbonPart.OnActionCalled);

            this.fluentRibbonManager.OnAction(this.existingribbonpartid);
            Assert.IsTrue(this.ribbonPart.OnActionCalled);
        }

        [Test]
        public void VerifyThatGetDescriptionReturnsExpected()
        {
            Assert.AreEqual(string.Empty, this.fluentRibbonManager.GetDescription(this.nonexsitingribbonpartid));

            var description = this.GenerateRandomString(4096);
            this.ribbonPart.Description = description;
            Assert.AreEqual(description, this.fluentRibbonManager.GetDescription(this.existingribbonpartid));

            this.ribbonPart.Description = description + " - append some charachters to make it longer than allowed";
            Assert.AreEqual(description, this.fluentRibbonManager.GetDescription(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetKeyTipReturnsExpected()
        {
            Assert.AreEqual(string.Empty, this.fluentRibbonManager.GetKeytip(this.nonexsitingribbonpartid));

            var keytip = this.GenerateRandomString(3);
            this.ribbonPart.Keytip = keytip;
            Assert.AreEqual(keytip, this.fluentRibbonManager.GetKeytip(this.existingribbonpartid));

            this.ribbonPart.Keytip = keytip + " - append some charachters to make it longer than allowed";
            Assert.AreEqual(keytip, this.fluentRibbonManager.GetKeytip(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetScreenTipReturnsExpected()
        {
            Assert.AreEqual(string.Empty, this.fluentRibbonManager.GetScreentip(this.nonexsitingribbonpartid));
            
            var screentip = this.GenerateRandomString(1024);
            this.ribbonPart.Screentip = screentip;
            Assert.AreEqual(screentip, this.fluentRibbonManager.GetScreentip(this.existingribbonpartid));

            this.ribbonPart.Screentip = screentip + " - append some charachters to make it longer than allowed";
            Assert.AreEqual(screentip, this.fluentRibbonManager.GetScreentip(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetContentReturnsExpected()
        {
            Assert.AreEqual(string.Empty, this.fluentRibbonManager.GetContent(this.nonexsitingribbonpartid));

            this.ribbonPart.Content = "some invalid xml content";
            Assert.AreEqual(string.Empty, this.fluentRibbonManager.GetContent(this.existingribbonpartid));

            this.ribbonPart.Content = "<validcontent>some valid content</validcontent>";
            Assert.AreEqual("<validcontent>some valid content</validcontent>", this.fluentRibbonManager.GetContent(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetLabelReturnsExpected()
        {
            Assert.AreEqual(string.Empty, this.fluentRibbonManager.GetLabel(this.nonexsitingribbonpartid));

            var label = this.GenerateRandomString(1024);
            this.ribbonPart.Label = label;
            Assert.AreEqual(label, this.fluentRibbonManager.GetLabel(this.existingribbonpartid));

            this.ribbonPart.Label = label + " - append some charachters to make it longer than allowed";
            Assert.AreEqual(label, this.fluentRibbonManager.GetLabel(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetSizeReturnsExpected()
        {
            Assert.AreEqual("normal", this.fluentRibbonManager.GetSize(this.nonexsitingribbonpartid));

            this.ribbonPart.Size = "normal";
            Assert.AreEqual("normal", this.fluentRibbonManager.GetSize(this.existingribbonpartid));

            this.ribbonPart.Size = "large";
            Assert.AreEqual("large", this.fluentRibbonManager.GetSize(this.existingribbonpartid));

            this.ribbonPart.Size = "someotherword";
            Assert.AreEqual("normal", this.fluentRibbonManager.GetSize(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetSupertipReturnsExpected()
        {
            Assert.AreEqual(string.Empty, this.fluentRibbonManager.GetSupertip(this.nonexsitingribbonpartid));

            var supertip = this.GenerateRandomString(1024);
            this.ribbonPart.Supertip = supertip;
            Assert.AreEqual(supertip, this.fluentRibbonManager.GetSupertip(this.existingribbonpartid));

            this.ribbonPart.Supertip = supertip + " - append some charachters to make it longer than allowed";
            Assert.AreEqual(supertip, this.fluentRibbonManager.GetSupertip(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetEnabledReturnsExpected()
        {
            Assert.IsTrue(this.fluentRibbonManager.GetEnabled(this.nonexsitingribbonpartid));

            this.ribbonPart.Enabled = false;

            Assert.IsFalse(this.fluentRibbonManager.GetEnabled(this.existingribbonpartid));

            this.ribbonPart.Enabled = true;

            Assert.IsTrue(this.fluentRibbonManager.GetEnabled(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetPressedReturnsExpected()
        {
            Assert.IsFalse(this.fluentRibbonManager.GetPressed(this.nonexsitingribbonpartid));

            this.ribbonPart.Pressed = false;

            Assert.IsFalse(this.fluentRibbonManager.GetPressed(this.existingribbonpartid));

            this.ribbonPart.Pressed = true;

            Assert.IsTrue(this.fluentRibbonManager.GetPressed(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetVisibleReturnsExpected()
        {
            Assert.IsTrue(this.fluentRibbonManager.GetVisible(this.nonexsitingribbonpartid));

            this.ribbonPart.Visible = false;

            Assert.IsFalse(this.fluentRibbonManager.GetVisible(this.existingribbonpartid));

            this.ribbonPart.Visible = true;

            Assert.IsTrue(this.fluentRibbonManager.GetVisible(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetShowImageReturnsExpected()
        {
            Assert.IsTrue(this.fluentRibbonManager.GetShowImage(this.nonexsitingribbonpartid));

            this.ribbonPart.ShowImage = false;

            Assert.IsFalse(this.fluentRibbonManager.GetShowImage(this.existingribbonpartid));

            this.ribbonPart.ShowImage = true;

            Assert.IsTrue(this.fluentRibbonManager.GetShowImage(this.existingribbonpartid));
        }

        [Test]
        public void VerifyThatGetShowLabelReturnsExpected()
        {
            Assert.IsTrue(this.fluentRibbonManager.GetShowLabel(this.nonexsitingribbonpartid));

            this.ribbonPart.ShowLabel = false;

            Assert.IsFalse(this.fluentRibbonManager.GetShowLabel(this.existingribbonpartid));

            this.ribbonPart.ShowLabel = true;

            Assert.IsTrue(this.fluentRibbonManager.GetShowLabel(this.existingribbonpartid));
        }

        /// <summary>
        /// Generates a random string with a specific length
        /// </summary>
        /// <param name="length">
        /// the length of the string
        /// </param>
        /// <returns>
        /// a string with the specified length
        /// </returns>
        public string GenerateRandomString(int length)
        {
            var random = new Random();
            string randomString = string.Empty;
            int randNumber;

            // Loop ‘length’ times to generate a random number or character
            for (int i = 0; i < length; i++)
            {
                if (random.Next(1, 3) == 1)
                {
                    randNumber = random.Next(97, 123); //char {a-z}
                }
                else
                {
                    randNumber = random.Next(48, 58); //int {0-9}
                }
                
                // append random char or digit to random string
                randomString = randomString + (char)randNumber;
            }

            // return the random string
            return randomString;
        }
    }

    /// <summary>
    /// The purpose of the <see cref="RibbonPartTest"/> class is to assist in unit testing the <see cref="FluentRibbonManager"/>
    /// Every Callback method as a property as a counterpart that is set by the test fixture test methods to verify that the
    /// <see cref="FluentRibbonManager"/> invokes the correct methods on the registered <see cref="RibbonPart"/>s and returns
    /// the proper default value if the requested <see cref="RibbonPart"/> does not exist.
    /// </summary>
    internal class RibbonPartTest : RibbonPart
    {
        internal RibbonPartTest(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService)
        {
        }

        public override async Task OnAction(string ribbonControlTag, string ribbonControlId)
        {
            this.OnActionCalled = true;
        }

        public bool OnActionCalled { get; private set; }

        public string Content { get; set; }

        public override string GetContent(string ribbonControlTag, string ribbonControlId)
        {
            return this.Content;
        }

        public string Description { get; set; }

        public override string GetDescription(string ribbonControlTag, string ribbonControlId)
        {
            return this.Description;
        }

        public string Keytip { get; set; }

        public override string GetKeytip(string ribbonControlTag, string ribbonControlId)
        {
            return this.Keytip;
        }

        public string Screentip { get; set; }

        public override string GetScreentip(string ribbonControlTag, string ribbonControlId)
        {
            return this.Screentip;
        }

        public string Label { get; set; }

        public override string GetLabel(string ribbonControlTag, string ribbonControlId)
        {
            return this.Label;
        }

        public string Size { get; set; }

        public override string GetSize(string ribbonControlTag, string ribbonControlId)
        {
            return this.Size;
        }

        public string Supertip { get; set; }

        public override string GetSupertip(string ribbonControlTag, string ribbonControlId)
        {
            return this.Supertip;
        }

        public Image Image { get; set; }

        public override Image GetImage(string ribbonControlTag, string ribbonControlId)
        {
            return this.Image;
        }

        public bool Enabled { get; set; }

        public override bool GetEnabled(string ribbonControlTag, string ribbonControlId)
        {
            return this.Enabled;
        }

        public bool Pressed { get; set; }

        public override bool GetPressed(string ribbonControlTag, string ribbonControlId)
        {
            return this.Pressed;
        }

        public bool Visible { get; set; }

        public override bool GetVisible(string ribbonControlTag, string ribbonControlId)
        {
            return this.Visible;
        }

        public bool ShowImage { get; set; }

        public override bool GetShowImage(string ribbonControlTag, string ribbonControlId)
        {
            return this.ShowImage;
        }

        public bool ShowLabel { get; set; }

        public override bool GetShowLabel(string ribbonControlTag, string ribbonControlId)
        {
            return this.ShowLabel;
        }

        protected override string GetRibbonXmlResourceName()
        {
            return "testribbon";
        }

        protected override Assembly GetCurrentAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}
