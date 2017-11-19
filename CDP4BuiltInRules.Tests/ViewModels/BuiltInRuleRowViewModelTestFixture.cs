// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    using CDP4BuiltInRules.ViewModels;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="BuiltInRuleRowViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class BuiltInRuleRowViewModelTestFixture
    {
        private Mock<IDragInfo> draginfo;
        private BuiltInRule builtInRule;
        private Mock<IBuiltInRuleMetaData> iBuiltInRuleMetaData;
            
        [SetUp]
        public void SetUp()
        {
            this.draginfo = new Mock<IDragInfo>();

            this.iBuiltInRuleMetaData = new Mock<IBuiltInRuleMetaData>();
            this.iBuiltInRuleMetaData.Setup(x => x.Author).Returns("RHEA");
            this.iBuiltInRuleMetaData.Setup(x => x.Name).Returns("shortnamerule");
            this.iBuiltInRuleMetaData.Setup(x => x.Description).Returns("verifies that the shortnames are correct");

            this.builtInRule = new TestBuiltInRule();
        }

        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var viewmodel = new BuiltInRuleRowViewModel(this.builtInRule, this.iBuiltInRuleMetaData.Object);

            Assert.AreEqual("RHEA", viewmodel.Author);
            Assert.AreEqual("shortnamerule", viewmodel.Name);
            Assert.AreEqual("verifies that the shortnames are correct", viewmodel.Description);

            Assert.AreEqual(this.builtInRule, viewmodel.Rule);
        }

        [Test]
        public void VerifyThatPayloadAndDraggEffectAreSetOnStartDrag()
        {
            var viewmodel = new BuiltInRuleRowViewModel(this.builtInRule, this.iBuiltInRuleMetaData.Object);

            this.draginfo.SetupProperty(x => x.Effects);
            this.draginfo.SetupProperty(x => x.Payload);

            viewmodel.StartDrag(this.draginfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, this.draginfo.Object.Effects);
            Assert.AreEqual(this.iBuiltInRuleMetaData.Object, this.draginfo.Object.Payload);
        }

        /// <summary>
        /// test class
        /// </summary>
        private class TestBuiltInRule : BuiltInRule
        {
            public override IEnumerable<RuleViolation> Verify(Iteration iteration)
            {
                throw new NotSupportedException();
            }
        }
    }
}
