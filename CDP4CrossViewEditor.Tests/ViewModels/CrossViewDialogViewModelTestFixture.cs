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

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.ViewModels;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossViewDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class CrossViewDialogViewModelTestFixture
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

            this.iteration = this.CreateIteration();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModel = new CrossViewDialogViewModel(this.iteration);

            Assert.AreEqual("Select equipments and parameters", viewModel.DialogTitle);
            Assert.IsInstanceOf<ThingSelectorViewModel>(viewModel.ElementSelectorViewModel);
            Assert.IsInstanceOf<ThingSelectorViewModel>(viewModel.ParameterSelectorViewModel);
        }

        [Test]
        public void VerifyThatCommandsWorks()
        {
            var viewModel = new CrossViewDialogViewModel(this.iteration);

            Assert.DoesNotThrow(() => viewModel.OkCommand.Execute(null));
            Assert.DoesNotThrow(() => viewModel.CancelCommand.Execute(null));
        }

        private Iteration CreateIteration()
        {
            return new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
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
    }
}
