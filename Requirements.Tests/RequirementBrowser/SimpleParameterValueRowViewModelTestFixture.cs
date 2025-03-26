// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleParameterValueRowViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate
//
//    This file is part of COMET-IME Community Edition.
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

namespace CDP4Requirements.Tests.RequirementBrowser
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class SimpleParameterValueRowViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private Requirement requirement;
        private SimpleParameterValue simpleParameterValue;
        private TextParameterType textParameterType;
        private EnumerationParameterType enumParameterType;
        private CompoundParameterType compoundParameterType;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();

            this.requirement = new Requirement(Guid.NewGuid(), null, null);

            this.simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), null, null)
            {
                Scale = new CyclicRatioScale
                {
                    Name = "a", ShortName = "e"
                },
            };

            this.compoundParameterType = new CompoundParameterType(Guid.NewGuid(), null, null)
            {
                Name = "compoundPT", ShortName = "cpt"
            };

            this.compoundParameterType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), null, null)
            {
                ParameterType = this.textParameterType
            });

            this.textParameterType = new TextParameterType(Guid.NewGuid(), null, null)
            {
                Name = "textPT", ShortName = "tpt"
            };

            this.enumParameterType = new EnumerationParameterType(Guid.NewGuid(), null, null)
            {
                Name = "enumPT", ShortName = "ept",
                AllowMultiSelect = false
            };

            this.enumParameterType.ValueDefinition.Add(new EnumerationValueDefinition(Guid.NewGuid(), null, null) 
            {
                Name = "enumVal",
                ShortName = "eval"
            });

            this.simpleParameterValue.ParameterType = this.textParameterType;
            var values = new List<string> { "1" };
            this.simpleParameterValue.Value = new ValueArray<string>(values);

            this.requirement.ParameterValue.Add(this.simpleParameterValue);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.Term, It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new SimpleParameterValueRowViewModel(this.simpleParameterValue, this.session.Object, null);

            Assert.That(vm.Name, Is.EqualTo(this.textParameterType.Name));
            Assert.That(vm.ShortName, Is.EqualTo($"{this.textParameterType.ShortName} [{vm.Scale.ShortName}]"));
            vm.Scale = null;

            Assert.That(vm.ShortName, Is.EqualTo(this.textParameterType.ShortName));
            Assert.That(vm.Definition, Is.Null);
            Assert.That(vm.Value, Is.EqualTo("1"));
            Assert.That(vm.ParameterTypeClassKind, Is.EqualTo(ClassKind.TextParameterType));
            Assert.That(vm.IsValueSetEditorActive, Is.True);
            Assert.That(vm.IsMultiSelect, Is.False);
        }

        [Test]
        public void VerifyThatCompoundParameterTypePropertiesAreSet()
        {
            //Not supported, but should not crash
            this.simpleParameterValue.ParameterType = this.compoundParameterType;

            var vm = new SimpleParameterValueRowViewModel(this.simpleParameterValue, this.session.Object, null);

            Assert.That(vm.IsValueSetEditorActive, Is.False);
            Assert.That(vm.IsMultiSelect, Is.False);

            Assert.That(vm.Name, Is.EqualTo(this.compoundParameterType.Name));
            Assert.That(vm.ShortName, Is.EqualTo($"{this.compoundParameterType.ShortName} [{vm.Scale.ShortName}]"));
            vm.Scale = null;

            Assert.That(vm.ShortName, Is.EqualTo(this.compoundParameterType.ShortName));
            Assert.That(vm.Definition, Is.Null);
            Assert.That(vm.ParameterTypeClassKind, Is.EqualTo(ClassKind.CompoundParameterType));
        }

        [Test]
        public void VerifyThatSingleSelectEnumerationParameterTypePropertiesAreSet()
        {
            //Not supported, but should not crash
            this.simpleParameterValue.ParameterType = this.enumParameterType;

            var vm = new SimpleParameterValueRowViewModel(this.simpleParameterValue, this.session.Object, null);

            Assert.That(vm.IsValueSetEditorActive, Is.True);
            Assert.That(vm.IsMultiSelect, Is.False);

            Assert.That(vm.Name, Is.EqualTo(this.enumParameterType.Name));
            Assert.That(vm.ShortName, Is.EqualTo($"{this.enumParameterType.ShortName} [{vm.Scale.ShortName}]"));
            vm.Scale = null;

            Assert.That(vm.ShortName, Is.EqualTo(this.enumParameterType.ShortName));
            Assert.That(vm.Definition, Is.Null);
            Assert.That(vm.ParameterTypeClassKind, Is.EqualTo(ClassKind.EnumerationParameterType));
        }

        [Test]
        public void VerifyThatMultiSelectEnumerationParameterTypePropertiesAreSet()
        {
            //Not supported, but should not crash
            this.simpleParameterValue.ParameterType = this.enumParameterType;
            this.enumParameterType.AllowMultiSelect = true;

            var vm = new SimpleParameterValueRowViewModel(this.simpleParameterValue, this.session.Object, null);

            Assert.That(vm.IsValueSetEditorActive, Is.True);
            Assert.That(vm.IsMultiSelect, Is.True);

            Assert.That(vm.Name, Is.EqualTo(this.enumParameterType.Name));
            Assert.That(vm.ShortName, Is.EqualTo($"{this.enumParameterType.ShortName} [{vm.Scale.ShortName}]"));
            vm.Scale = null;

            Assert.That(vm.ShortName, Is.EqualTo(this.enumParameterType.ShortName));
            Assert.That(vm.Definition, Is.Null);
            Assert.That(vm.ParameterTypeClassKind, Is.EqualTo(ClassKind.EnumerationParameterType));
        }
    }
}
