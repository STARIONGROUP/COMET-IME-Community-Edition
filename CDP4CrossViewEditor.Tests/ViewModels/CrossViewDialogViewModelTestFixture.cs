// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CrossViewEditor.Tests.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.ViewModels;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossViewDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class CrossViewDialogViewModelTestFixture
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
        /// Current iteration used for test
        /// </summary>
        private Iteration iteration;

        [SetUp]
        public void SetUp()
        {
            this.assembler = new Assembler(this.credentials.Uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

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

            var elementDefinition = new ElementDefinition
            {
                Iid = Guid.NewGuid(),
                Name = "ElementDefinition_1",
                ShortName = "ED_1",
                Container = this.iteration,
                Owner = domain
            };

            var parameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ShortName = "P_SimpleQuantityKind"
            };

            var parameterTypePowerOn = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ShortName = "P_on"
            };

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = parameterType.DefaultScale,
                Owner = domain
            };

            var parameterPowerOn = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterTypePowerOn,
                Scale = parameterTypePowerOn.DefaultScale,
                Owner = domain
            };

            this.iteration.Element.Add(elementDefinition);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(parameter);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(parameterPowerOn);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModel = new CrossViewDialogViewModel(null, this.iteration, this.session.Object);

            Assert.AreEqual("Select ElementDefinitions and ParameterTypes", viewModel.DialogTitle);
            Assert.IsInstanceOf<ElementDefinitionSelectorViewModel>(viewModel.ElementSelectorViewModel);
            Assert.IsInstanceOf<ParameterTypeSelectorViewModel>(viewModel.ParameterSelectorViewModel);
        }

        [Test]
        public void VerifyThatOkCommandsWorks()
        {
            var viewModel = new CrossViewDialogViewModel(null, this.iteration, this.session.Object);
            var elementDefinitionSelectorViewModel = new ElementDefinitionSelectorViewModel(this.iteration, this.session.Object);
            var parameterTypeSelectorViewModel = new ParameterTypeSelectorViewModel(this.iteration, this.session.Object);

            Assert.DoesNotThrow(() => elementDefinitionSelectorViewModel.BindData());
            Assert.DoesNotThrow(() => parameterTypeSelectorViewModel.BindData());

            parameterTypeSelectorViewModel.SelectedSourceList = parameterTypeSelectorViewModel.ParameterTypeSourceList;
            elementDefinitionSelectorViewModel.SelectedSourceList = elementDefinitionSelectorViewModel.ElementDefinitionSourceList;

            Assert.DoesNotThrow(() => viewModel.OkCommand.Execute(null));
            Assert.IsTrue(viewModel.DialogResult.Result);
        }

        [Test]
        public void VerifyThatCancelCommandsWorks()
        {
            var viewModel = new CrossViewDialogViewModel(null, this.iteration, this.session.Object);
            Assert.DoesNotThrow(() => viewModel.CancelCommand.Execute(null));
            Assert.IsFalse(viewModel.DialogResult.Result);
        }

        [Test]
        public void VerifyThatPowerCommandsWorks()
        {
            var parameterTypeSelectorViewModel = new ParameterTypeSelectorViewModel(this.iteration, this.session.Object);

            Assert.Zero(parameterTypeSelectorViewModel.ParameterTypeSourceList.Count);

            parameterTypeSelectorViewModel.BindData();

            Assert.AreEqual(2, parameterTypeSelectorViewModel.ParameterTypeSourceList.Count);
            Assert.AreEqual(0, parameterTypeSelectorViewModel.ParameterTypeTargetList.Count);

            parameterTypeSelectorViewModel.PowerParametersEnabled = true;

            Assert.AreEqual(1, parameterTypeSelectorViewModel.ParameterTypeTargetList.Count);
            Assert.AreEqual(1, parameterTypeSelectorViewModel.ParameterTypeTargetList.Count);

            parameterTypeSelectorViewModel.PowerParametersEnabled = false;

            Assert.AreEqual(2, parameterTypeSelectorViewModel.ParameterTypeSourceList.Count);
            Assert.AreEqual(0, parameterTypeSelectorViewModel.ParameterTypeTargetList.Count);
        }
    }
}
