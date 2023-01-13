// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessedValueSetGeneratorTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Tests.DataCollection
{
    using System;

    using CDP4Common.EngineeringModelData;

    using CDP4Reporting.DataCollection;
    using CDP4Reporting.SubmittableParameterValues;
    using CDP4Reporting.Utilities;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ProcessedValueSetGeneratorTestFixture
    {
        private Mock<IOptionDependentDataCollector> optionDependentDataCollector;

        private Iteration iteration;

        private Option option1;

        private Option option2;
        private ProcessedValueSetGenerator processedValueSetGenerator;

        [SetUp]
        public void SetUp()
        {
            this.optionDependentDataCollector = new Mock<IOptionDependentDataCollector>();
            this.iteration = new Iteration(Guid.NewGuid(), null, null);

            // Options
            this.option1 = new Option(Guid.NewGuid(), null, null)
            {
                ShortName = "option1",
                Name = "option1"
            };

            this.option2 = new Option(Guid.NewGuid(), null, null)
            {
                ShortName = "option2",
                Name = "option2"
            };

            this.iteration.Option.Add(this.option1);
            this.iteration.Option.Add(this.option2);
            this.processedValueSetGenerator = new ProcessedValueSetGenerator(this.optionDependentDataCollector.Object);
        }

        [Test]
        public void VerifyWriteAllowedForCurrentSelectedOptionAndFullGenericPath()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST\\option1", false);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option1), Is.True);
        }

        [Test]
        public void VerifyWriteAllowedForOtherOptionAndFullGenericPath()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST\\option1", false);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option2), Is.True);
        }

        [Test]
        public void VerifyWriteAllowedForCurrentSelectedOptionAndFullExactPath()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST\\option1", true);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option1), Is.True);
        }

        [Test]
        public void VerifyWriteAllowedForOtherOptionAndFullExactPath()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST\\option1", true);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option2), Is.False);
        }

        [Test]
        public void VerifyWriteAllowedForCurrentSelectedOptionAndIncompleteGenericPathHavingNo4thElement()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST", false);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option1), Is.True);
        }

        [Test]
        public void VerifyWriteAllowedForOtherOptionAndIncompleteGenericPathHavingNo4thElement()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST", false);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option2), Is.True);
        }

        [Test]
        public void VerifyWriteAllowedForCurrentSelectedOptionAndIncompleteExactPathHavingNo4thElement()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST", true);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option1), Is.False);
        }

        [Test]
        public void VerifyWriteAllowedForOtherOptionAndIncompleteExactPathHavingNo4thElement()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST", true);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option2), Is.False);
        }

        [Test]
        public void VerifyWriteAllowedForCurrentSelectedOptionAndIncompleteGenericPathHavingEmpty4thElement()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST\\", false);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option1), Is.True);
        }

        [Test]
        public void VerifyWriteAllowedForOtherOptionAndIncompleteGenericPathHavingEmpty4thElement()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST\\", false);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option2), Is.True);
        }

        [Test]
        public void VerifyWriteAllowedForCurrentSelectedOptionAndIncompleteExactPathHavingEmpty4thElement()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST\\", true);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option1), Is.False);
        }

        [Test]
        public void VerifyWriteAllowedForOtherOptionAndIncompleteExactPathHavingEmpty4thElement()
        {
            this.optionDependentDataCollector.Setup(x => x.SelectedOption).Returns(this.option1);
            var submittableParameterValue = new SubmittableParameterValue("ED\\P\\ST\\", true);
            Assert.That(this.processedValueSetGenerator.ValueSetWriteAllowed(submittableParameterValue, option2), Is.False);
        }
    }
}
