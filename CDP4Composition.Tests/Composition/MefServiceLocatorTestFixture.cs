// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MefServiceLocatorTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.Composition
{
    using System;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using CDP4Composition.Composition;

    using CommonServiceLocator;

    using NUnit.Framework;

    [TestFixture]
    public class MefServiceLocatorTestFixture
    {
        [Test]
        public void VerifyGetAllInstancesByTypeReturnsCorrectInstances()
        {
            string firstStringInstance = nameof(firstStringInstance);
            string secondStringInstance = nameof(secondStringInstance);

            var container = new CompositionContainer();
            container.ComposeExportedValue(firstStringInstance);
            container.ComposeExportedValue(secondStringInstance);
            container.ComposeExportedValue(10);
            container.ComposeExportedValue(1m);

            var serviceLocator = new MefServiceLocator(container);

            var instances = serviceLocator.GetAllInstances(typeof(string));

            Assert.That(instances, Has.Count.EqualTo(2));
            Assert.That(instances, Has.Exactly(1).EqualTo(firstStringInstance));
            Assert.That(instances, Has.Exactly(1).EqualTo(secondStringInstance));
        }

        [Test]
        public void VerifyGetAllInstancesReturnsCorrectInstances()
        {
            string firstStringInstance = nameof(firstStringInstance);
            string secondStringInstance = nameof(secondStringInstance);

            var container = new CompositionContainer();
            container.ComposeExportedValue(firstStringInstance);
            container.ComposeExportedValue(secondStringInstance);
            container.ComposeExportedValue(typeof(string));
            container.ComposeExportedValue(Math.PI);

            var serviceLocator = new MefServiceLocator(container);

            var instances = serviceLocator.GetAllInstances<string>();

            Assert.That(instances.ToList(), Has.Count.EqualTo(2));
            Assert.That(instances, Has.Exactly(1).EqualTo(firstStringInstance));
            Assert.That(instances, Has.Exactly(1).EqualTo(secondStringInstance));
        }

        [Test]
        public void VerifyGetInstanceThrowsWithMultipleInstances()
        {
            string firstStringInstance = nameof(firstStringInstance);
            string secondStringInstance = nameof(secondStringInstance);

            var container = new CompositionContainer();
            container.ComposeExportedValue(firstStringInstance);
            container.ComposeExportedValue(secondStringInstance);

            var serviceLocator = new MefServiceLocator(container);

            Assert.Throws<ActivationException>(() => serviceLocator.GetInstance<string>());
        }

        [Test]
        public void VerifyGetInstanceThrowsWithZeroInstances()
        {
            string firstStringInstance = nameof(firstStringInstance);
            string secondStringInstance = nameof(secondStringInstance);

            var container = new CompositionContainer();
            container.ComposeExportedValue(firstStringInstance);
            container.ComposeExportedValue(secondStringInstance);

            var serviceLocator = new MefServiceLocator(container);

            Assert.Throws<ActivationException>(() => serviceLocator.GetInstance<int>());
        }

        [Test]
        public void VerifyGetInstanceReturnsCorrectInstance()
        {
            string stringInstance = nameof(stringInstance);

            var container = new CompositionContainer();
            container.ComposeExportedValue(stringInstance);

            var serviceLocator = new MefServiceLocator(container);

            var instance = serviceLocator.GetInstance<string>();

            Assert.That(instance, Is.EqualTo(stringInstance));
        }

        [Test]
        public void VerifyGetInstanceByTypeReturnsCorrectInstance()
        {
            string stringInstance = nameof(stringInstance);

            var container = new CompositionContainer();
            container.ComposeExportedValue(stringInstance);

            var serviceLocator = new MefServiceLocator(container);

            var instance = serviceLocator.GetInstance(typeof(string));

            Assert.That(instance, Is.EqualTo(stringInstance));
        }

        [Test]
        public void VerifyGetInstanceByKeyReturnsCorrectInstance()
        {
            string stringInstance = nameof(stringInstance);

            var container = new CompositionContainer();
            container.ComposeExportedValue("key", stringInstance);
            container.ComposeExportedValue("wrong key", "a string instance");
            container.ComposeExportedValue("another wrong key", "another string instance");

            var serviceLocator = new MefServiceLocator(container);

            var instance = serviceLocator.GetInstance<string>("key");

            Assert.That(instance, Is.EqualTo(stringInstance));
        }

        [Test]
        public void VerifyGetInstanceByKeyAndTypeReturnsCorrectInstance()
        {
            string stringInstance = nameof(stringInstance);

            var container = new CompositionContainer();
            container.ComposeExportedValue("key", stringInstance);
            container.ComposeExportedValue("wrong key", "a string instance");
            container.ComposeExportedValue("another wrong key", "another string instance");

            var serviceLocator = new MefServiceLocator(container);

            var instance = serviceLocator.GetInstance(typeof(string), "key");

            Assert.That(instance, Is.EqualTo(stringInstance));
        }
    }
}
