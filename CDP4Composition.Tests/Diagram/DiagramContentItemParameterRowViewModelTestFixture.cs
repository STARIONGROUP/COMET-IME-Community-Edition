// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramContentItemParameterRowViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Diagram
{
    using System.Threading;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Diagram;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DiagramContentItemParameterRowViewModel"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class DiagramContentItemParameterRowViewModelTestFixture
    {
        private Parameter parameter1;
        private Parameter parameter2;
        private Mock<ISession> session;

        [SetUp]
        public void SetUp()
        {
            this.parameter1 = new Parameter
            {
                ParameterType = new SimpleQuantityKind(),
                StateDependence = new ActualFiniteStateList(),
                IsOptionDependent = true
            };

            this.parameter2 = new Parameter
            {
                ParameterType = new CompoundParameterType()
            };

            var possibleFiniteStateList = new PossibleFiniteStateList();
            possibleFiniteStateList.PossibleState.Add(new PossibleFiniteState { Name = "state1" });
            possibleFiniteStateList.PossibleState.Add(new PossibleFiniteState { Name = "state2" });

            var actualFiniteStateList = new ActualFiniteStateList();
            actualFiniteStateList.PossibleFiniteStateList.Add(possibleFiniteStateList);

            var actualState1 = new ActualFiniteState();
            actualState1.PossibleState.Add(possibleFiniteStateList.PossibleState[0]);

            var actualState2 = new ActualFiniteState();
            actualState2.PossibleState.Add(possibleFiniteStateList.PossibleState[1]);

            actualFiniteStateList.ActualState.Add(actualState1);
            actualFiniteStateList.ActualState.Add(actualState2);

            this.parameter1.ValueSet.Add(
                new ParameterValueSet
                {
                    Published = new ValueArray<string>(new[]
                    {
                        "value1"
                    }),
                    ActualState = actualState1,
                    ActualOption = new Option { Name = "option1" }
                });

            this.parameter1.ValueSet.Add(
                new ParameterValueSet
                {
                    Published = new ValueArray<string>(new[]
                    {
                        "value2"
                    }),
                    ActualState = actualState2,
                    ActualOption = new Option { Name = "option2" }
                });

            (this.parameter2.ParameterType as CompoundParameterType)?
                .Component.Add(
                    new ParameterTypeComponent
                    {
                        ShortName = "c1",
                        ParameterType = new SimpleQuantityKind()
                    });

            (this.parameter2.ParameterType as CompoundParameterType)?
                .Component.Add(
                    new ParameterTypeComponent
                    {
                        ShortName = "c2",
                        ParameterType = new SimpleQuantityKind()
                    });

            this.parameter2.ValueSet.Add(
                new ParameterValueSet
                {
                    Published = new ValueArray<string>(new[]
                    {
                        "value3",
                        "value4"
                    })
                });

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
        }

        [Test]
        public void VerifyThatDiagramContentItemChildStringContainsExpectedParts1()
        {
            var rowViewModel = new DiagramContentItemParameterRowViewModel(this.parameter1, this.session.Object, null);

            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("value1"));
            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("value2"));
            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("option1"));
            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("option2"));
            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("state1"));
            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("state2"));
        }

        [Test]
        public void VerifyThatDiagramContentItemChildStringContainsExpectedParts2()
        {
            var rowViewModel = new DiagramContentItemParameterRowViewModel(this.parameter2, this.session.Object, null);

            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("c1"));
            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("c2"));
            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("value3"));
            Assert.IsTrue(rowViewModel.DiagramContentItemChildString.Contains("value4"));
        }
    }
}
