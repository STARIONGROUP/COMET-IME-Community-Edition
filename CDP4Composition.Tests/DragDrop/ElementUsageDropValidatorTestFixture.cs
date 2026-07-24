// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageDropValidatorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Tests.DragDrop
{
    using System;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;

    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ElementUsageDropValidator"/> class.
    /// </summary>
    [TestFixture]
    public class ElementUsageDropValidatorTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Iteration iteration;
        private ElementDefinition sourceDefinition;
        private ElementDefinition targetDefinition;
        private ElementDefinition referencedDefinition;
        private ElementUsage elementUsage;

        [SetUp]
        public void SetUp()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            var engineeringModel = new EngineeringModel(Guid.NewGuid(), null, null);
            this.iteration = new Iteration(Guid.NewGuid(), null, null);
            engineeringModel.Iteration.Add(this.iteration);

            this.sourceDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            this.targetDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            this.referencedDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(this.sourceDefinition);
            this.iteration.Element.Add(this.targetDefinition);
            this.iteration.Element.Add(this.referencedDefinition);

            this.elementUsage = new ElementUsage(Guid.NewGuid(), null, null) { ElementDefinition = this.referencedDefinition };
            this.sourceDefinition.ContainedElement.Add(this.elementUsage);
        }

        [Test]
        public void VerifyThatDroppingOnAnotherDefinitionReturnsMove()
        {
            var effect = ElementUsageDropValidator.GetDropEffect(this.elementUsage, this.targetDefinition, this.permissionService.Object);

            Assert.That(effect, Is.EqualTo(DragDropEffects.Move));
        }

        [Test]
        public void VerifyThatMovingOntoTheCurrentContainerReturnsNone()
        {
            var effect = ElementUsageDropValidator.GetDropEffect(this.elementUsage, this.sourceDefinition, this.permissionService.Object);

            Assert.That(effect, Is.EqualTo(DragDropEffects.None));
        }

        [Test]
        public void VerifyThatDroppingOntoTheReferencedDefinitionReturnsNone()
        {
            // the usage references referencedDefinition; dropping it there would create a self-containment loop
            var moveEffect = ElementUsageDropValidator.GetDropEffect(this.elementUsage, this.referencedDefinition, this.permissionService.Object);

            Assert.That(moveEffect, Is.EqualTo(DragDropEffects.None));
        }

        [Test]
        public void VerifyThatContainmentLoopReturnsNone()
        {
            // referencedDefinition (the usage's type) already uses targetDefinition -> re-parenting there would create a loop
            var loopUsage = new ElementUsage(Guid.NewGuid(), null, null) { ElementDefinition = this.targetDefinition };
            this.referencedDefinition.ContainedElement.Add(loopUsage);

            var effect = ElementUsageDropValidator.GetDropEffect(this.elementUsage, this.targetDefinition, this.permissionService.Object);

            Assert.That(effect, Is.EqualTo(DragDropEffects.None));
        }

        [Test]
        public void VerifyThatMissingPermissionReturnsNone()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(false);

            var effect = ElementUsageDropValidator.GetDropEffect(this.elementUsage, this.targetDefinition, this.permissionService.Object);

            Assert.That(effect, Is.EqualTo(DragDropEffects.None));
        }

        [Test]
        public void VerifyThatDifferentModelReturnsNone()
        {
            var otherModel = new EngineeringModel(Guid.NewGuid(), null, null);
            var otherIteration = new Iteration(Guid.NewGuid(), null, null);
            otherModel.Iteration.Add(otherIteration);
            var foreignDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            otherIteration.Element.Add(foreignDefinition);

            var effect = ElementUsageDropValidator.GetDropEffect(this.elementUsage, foreignDefinition, this.permissionService.Object);

            Assert.That(effect, Is.EqualTo(DragDropEffects.None));
        }

        [Test]
        public void VerifyThatNullArgumentsReturnNone()
        {
            Assert.That(ElementUsageDropValidator.GetDropEffect(null, this.targetDefinition, this.permissionService.Object), Is.EqualTo(DragDropEffects.None));
            Assert.That(ElementUsageDropValidator.GetDropEffect(this.elementUsage, null, this.permissionService.Object), Is.EqualTo(DragDropEffects.None));
        }
    }
}
