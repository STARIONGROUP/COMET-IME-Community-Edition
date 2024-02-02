// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDiagramContentItemTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Diagram
{
    using System.Threading;

    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Diagram;
    using CDP4Composition.Services;

    using CDP4Dal;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ElementDefinitionDiagramContentItemViewModel"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ElementDefinitionDiagramContentItemTestFixture
    {
        private ArchitectureElement diagramObject;
        private ElementDefinition elementDefinition;
        private Parameter parameter1;
        private Parameter parameter2;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingCreator> thingCreator;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.thingCreator = new Mock<IThingCreator>();
            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>()).Returns(this.thingCreator.Object);

            this.elementDefinition = new ElementDefinition();
            this.parameter1 = new Parameter { ParameterType = new SimpleQuantityKind() };
            this.parameter2 = new Parameter { ParameterType = new SimpleQuantityKind() };
            this.elementDefinition.Parameter.Add(this.parameter1);
            this.elementDefinition.Parameter.Add(this.parameter2);

            this.diagramObject = new ArchitectureElement()
            {
                DepictedThing = this.elementDefinition
            };

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
        }

        [Test]
        public void VerifyThatElementDefinitionDiagramContentItemCanBeConstructed()
        {
            var elementDefinitionDiagramContentItem = new ElementDefinitionDiagramContentItemViewModel(this.diagramObject, this.session.Object, null);

            Assert.AreEqual(this.elementDefinition, elementDefinitionDiagramContentItem.Thing);
            Assert.AreEqual(2, elementDefinitionDiagramContentItem.DiagramContentItemChildren.Count);
        }
    }
}
