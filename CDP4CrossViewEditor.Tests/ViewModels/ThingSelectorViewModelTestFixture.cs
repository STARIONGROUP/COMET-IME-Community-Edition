// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingSelectorViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.ViewModels;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ThingSelectorViewModel"/> class
    /// </summary>
    [TestFixture]
    public class ThingSelectorViewModelTestFixture
    {
        /// <summary>
        /// The current set of credentials that will be used
        /// </summary>
        private readonly Credentials credentials = new Credentials(
            "John",
            "Doe",
            new Uri("http://www.rheagroup.com/"));

        /// <summary>
        /// The current session associated <see cref="Assembler"></see>
        /// </summary>
        private Assembler assembler;

        /// <summary>
        /// Mock <see cref="ISession"/>
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// Mock <see cref="IDal"/>
        /// </summary>
        private Mock<IDal> dal;

        /// <summary>
        /// Current iteration used for test <see cref="Iteration"/>
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// Preserved element definitions iids
        /// </summary>
        private List<Guid> preservedElementsIids = new List<Guid>();

        /// <summary>
        /// Preserved parameter types iids
        /// </summary>
        private readonly List<Guid> preservedParametersIids = new List<Guid>();

        /// <summary name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.credentials.Uri, this.messageBus);

            this.dal = new Mock<IDal>();
            this.dal.SetupProperty(d => d.Session);
            this.assembler = new Assembler(this.credentials.Uri, this.messageBus);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Dal).Returns(this.dal.Object);
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Container = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                },
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    IterationNumber = 1
                }
            };

            var domain = new DomainOfExpertise(Guid.NewGuid(), this.session.Object.Assembler.Cache, this.session.Object.Credentials.Uri)
            {
                Name = "Domain"
            };

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.session.Object.Assembler.Cache, this.session.Object.Credentials.Uri)
            {
                Name = "ElementDefinition",
                ShortName = "ED",
                Container = this.iteration,
                Owner = domain
            };

            var elementUsage = new ElementUsage(Guid.NewGuid(), this.session.Object.Assembler.Cache, this.session.Object.Credentials.Uri)
            {
                Name = "ElementUsage",
                ShortName = "EU",
                Owner = domain
            };

            elementDefinition.ContainedElement.Add(elementUsage);
            elementUsage.ElementDefinition = elementDefinition;

            var parameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_SimpleQuantityKind"
            };

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = parameterType.DefaultScale,
                Owner = domain
            };

            this.iteration.Element.Add(elementDefinition);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(parameter);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModelElements = new ElementDefinitionSelectorViewModel(
                this.iteration,
                this.session.Object,
                null);

            var viewModelParameters = new ParameterTypeSelectorViewModel(
                this.iteration,
                this.session.Object,
                null);

            Assert.AreEqual(this.iteration, viewModelElements.Iteration);
            Assert.AreEqual(0, viewModelElements.ElementDefinitionSourceList.Count);
            Assert.AreEqual(0, viewModelElements.ElementDefinitionTargetList.Count);
            Assert.AreEqual(ClassKind.ElementDefinition, viewModelElements.ThingClassKind);
            Assert.IsNull(viewModelElements.PreservedIids);

            Assert.AreEqual(this.iteration, viewModelParameters.Iteration);
            Assert.AreEqual(0, viewModelParameters.ParameterTypeSourceList.Count);
            Assert.AreEqual(0, viewModelParameters.ParameterTypeTargetList.Count);
            Assert.AreEqual(ClassKind.ParameterType, viewModelParameters.ThingClassKind);
            Assert.IsNull(viewModelParameters.PreservedIids);
        }

        [Test]
        public void VerifyThatCommandsWorksOnElements()
        {
            var viewModel = new ElementDefinitionSelectorViewModel(
                this.iteration,
                this.session.Object,
                this.preservedElementsIids);

            Assert.DoesNotThrow(() => viewModel.BindData());

            viewModel.SelectedSourceList = viewModel.ElementDefinitionSourceList;
            var elementDefinition = viewModel.SelectedSourceList.FirstOrDefault();

            Assert.NotNull(elementDefinition);
            Assert.AreEqual(0, elementDefinition.Categories.Count);
            Assert.AreEqual("ElementDefinition(Domain)", elementDefinition.ToString());

            Assert.DoesNotThrowAsync(async () => await viewModel.MoveItemsToTarget.Execute());

            viewModel.SelectedTargetList = viewModel.ElementDefinitionTargetList;
            Assert.DoesNotThrowAsync(async () => await viewModel.MoveItemsToSource.Execute());

            viewModel.SelectedSourceList = viewModel.ElementDefinitionSourceList;
            Assert.DoesNotThrowAsync(async () => await viewModel.MoveItemsToTarget.Execute());
            Assert.DoesNotThrowAsync(async () => await viewModel.ClearItems.Execute());
        }

        [Test]
        public void VerifyThatCommandsWorksOnParameters()
        {
            var viewModel = new ParameterTypeSelectorViewModel(
                this.iteration,
                this.session.Object,
                this.preservedParametersIids);

            Assert.DoesNotThrow(() => viewModel.BindData());

            viewModel.SelectedSourceList = viewModel.ParameterTypeSourceList;
            var parameterType = viewModel.SelectedSourceList.FirstOrDefault();

            Assert.NotNull(parameterType);
            Assert.AreEqual("SimpleQuantityKind", parameterType.Type);
            Assert.AreEqual("P_SimpleQuantityKind(SimpleQuantityKind)", parameterType.ToString());

            Assert.DoesNotThrowAsync(async () => await viewModel.MoveItemsToTarget.Execute());

            viewModel.SelectedTargetList = viewModel.ParameterTypeTargetList;
            Assert.DoesNotThrowAsync(async () => await viewModel.MoveItemsToSource.Execute());

            viewModel.SelectedSourceList = viewModel.ParameterTypeSourceList;
            Assert.DoesNotThrowAsync(async () => await viewModel.ClearItems.Execute());
        }
    }
}
