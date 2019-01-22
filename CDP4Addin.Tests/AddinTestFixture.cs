// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddinTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4AddinCE.Tests
{
    using System.Threading;
    using CDP4Composition;
    using Moq;
    using NetOffice.OfficeApi;
    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="AddinRibbonPart"/> class
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class AddinTestFixture
    {
        private Mock<IRibbonControl> ribbonControl;
        
        private string ribbonId;

        private string ribbbonTag;
        
        private Mock<IFluentRibbonManager> fluentRibbonManager;

        private Addin addin;

        [SetUp]
        public void SetUp()
        {
           
            
            this.ribbonId = "ribbonId";
            this.ribbbonTag = "ribbonTag";

            this.ribbonControl = new Mock<IRibbonControl>();
            this.ribbonControl.Setup(x => x.Id).Returns(this.ribbonId);
            this.ribbonControl.Setup(x => x.Tag).Returns(this.ribbbonTag);

            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();

            this.addin = new Addin();
            this.addin.FluentRibbonManager = this.fluentRibbonManager.Object;

            //TODO: this.addin.GetCustomUI()
        }

        [Test]
        public void VerifyAddinOnActionCallback()
        {
            this.addin.OnAction(this.ribbonControl.Object);
            this.fluentRibbonManager.Verify(x => x.OnAction(this.ribbonId, this.ribbbonTag));
        }

        [Test]
        public void VerifyAddinGetContentCallback()
        {
            this.addin.GetContent(this.ribbonControl.Object);
            this.fluentRibbonManager.Verify(x => x.GetContent(this.ribbonId, this.ribbbonTag));
        }

        [Test]
        public void VerifyAddinGetEnabledCallback()
        {
            this.addin.GetEnabled(this.ribbonControl.Object);
            this.fluentRibbonManager.Verify(x => x.GetEnabled(this.ribbonId, this.ribbbonTag));
        }

        [Test]
        public void VerifyAddinGetImageCallback()
        {
            this.addin.GetImage(this.ribbonControl.Object);
            this.fluentRibbonManager.Verify(x => x.GetImage(this.ribbonId, this.ribbbonTag));
        }

        [Test]
        public void VerifyAddinGetLabelCallback()
        {
            this.addin.GetLabel(this.ribbonControl.Object);
            this.fluentRibbonManager.Verify(x => x.GetLabel(this.ribbonId, this.ribbbonTag));
        }

        [Test]
        public void VerifyAddinGetPressedCallback()
        {
            this.addin.GetPressed(this.ribbonControl.Object);
            this.fluentRibbonManager.Verify(x => x.GetPressed(this.ribbonId, this.ribbbonTag));
        }

        [Test]
        public void VerifyAddinGetVisibleCallback()
        {
            this.addin.GetVisible(this.ribbonControl.Object);
            this.fluentRibbonManager.Verify(x => x.GetVisible(this.ribbonId, this.ribbbonTag));
        }


    }
}
