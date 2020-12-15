// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingSelectorViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.ViewModels;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ThingSelectorViewModel"/> class
    /// </summary>
    [TestFixture]
    public class ThingSelectorViewModelTestFixture
    {
        /// <summary>
        /// The current session associated <see cref="Assembler"></see>
        /// </summary>
        private Assembler assembler;

        /// <summary>
        /// The current assembler <see cref="Uri"/>
        /// </summary>
        private Uri uri;

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
            this.uri = new Uri("http://www.rheageoup.com");
            this.assembler = new Assembler(this.uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                },
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
                {
                    IterationNumber = 1
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModelElements = new ElementDefinitionSelectorViewModel(this.iteration);
            var viewModelParameters = new ParameterTypeSelectorViewModel(this.iteration);

            Assert.AreEqual(this.iteration, viewModelElements.Iteration);
            Assert.AreEqual(0, viewModelElements.ElementDefinitionSourceList.Count);
            Assert.AreEqual(0, viewModelElements.ElementDefinitionTargetList.Count);

            Assert.AreEqual(this.iteration, viewModelParameters.Iteration);
            Assert.AreEqual(0, viewModelParameters.ParameterTypeSourceList.Count);
            Assert.AreEqual(0, viewModelParameters.ParameterTypeTargetList.Count);
        }

        [Test]
        public void VerifyThatCommandsWorks()
        {
            var viewModel = new ThingSelectorViewModel(this.iteration, ClassKind.ElementBase);

            Assert.DoesNotThrow(() => viewModel.MoveItemsToSource.Execute(null));
            Assert.DoesNotThrow(() => viewModel.MoveItemsToTarget.Execute(null));
            Assert.DoesNotThrow(() => viewModel.MoveItemsUp.Execute(null));
            Assert.DoesNotThrow(() => viewModel.MoveItemsDown.Execute(null));
            Assert.DoesNotThrow(() => viewModel.SortItems.Execute(null));
            Assert.DoesNotThrow(() => viewModel.ClearItems.Execute(null));

            viewModel = new ThingSelectorViewModel(this.iteration, ClassKind.ParameterType);

            Assert.DoesNotThrow(() => viewModel.MoveItemsToSource.Execute(null));
            Assert.DoesNotThrow(() => viewModel.MoveItemsToTarget.Execute(null));
            Assert.DoesNotThrow(() => viewModel.MoveItemsUp.Execute(null));
            Assert.DoesNotThrow(() => viewModel.MoveItemsDown.Execute(null));
            Assert.DoesNotThrow(() => viewModel.SortItems.Execute(null));
            Assert.DoesNotThrow(() => viewModel.ClearItems.Execute(null));
        }

        [Test]
        public void VerifyBindings()
        {
            var viewModelElements = new ElementDefinitionSelectorViewModel(this.iteration);
            viewModelElements.BindData();

            var viewModelParameters = new ParameterTypeSelectorViewModel(this.iteration);
            viewModelParameters.BindData();
        }
    }
}
