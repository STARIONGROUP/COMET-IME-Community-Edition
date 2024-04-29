﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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

namespace CDP4CrossViewEditor.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.ViewModels;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NetOffice.ExcelApi;

    using NUnit.Framework;

    using Parameter = CDP4Common.EngineeringModelData.Parameter;

    /// <summary>
    /// Suite of tests for the <see cref="CrossViewDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class CrossViewDialogViewModelTestFixture
    {
        /// <summary>
        /// The current excel file path used by the test
        /// </summary>
        private string excelFilePath;

        /// <summary>
        /// The current set of credentials that will be used
        /// </summary>
        private readonly Credentials credentials = new Credentials(
            "John",
            "Doe",
            new Uri("https://www.stariongroup.eu/"));

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

        /// <summary>
        /// Preserved element definitions iids
        /// </summary>
        private List<Guid> preservedElementsIids = new List<Guid>();

        /// <summary>
        /// Preserved parameter types iids
        /// </summary>
        private List<Guid> preservedParametersIids = new List<Guid>();

        private Parameter parameterRedundancy;
        private Parameter parameterPowerOn;
        private Parameter parameterPowerStandBy;
        private Parameter parameterPowerPeak;
        private Parameter parameterPowerPowerDutyCycle;
        private Parameter parameterPowerMean;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.credentials.Uri, this.messageBus);
            this.session = new Mock<ISession>();
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

            var parameterTypeRedundancy = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "redundancy",
                ShortName = "redundancy",
                Component =
                {
                    new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        ShortName = "scheme",
                        ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                        {
                            ShortName = "scheme_pt"
                        }
                    },
                    new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        ShortName = "type",
                        ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                        {
                            ShortName = "type_pt"
                        }
                    },
                    new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        ShortName = "k",
                        ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                        {
                            ShortName = "k_pt"
                        }
                    },
                    new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        ShortName = "n",
                        ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                        {
                            ShortName = "n_pt"
                        }
                    }
                }
            };

            var parameterTypePowerOn = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ShortName = "P_on"
            };

            var parameterTypePowerStandby = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ShortName = "P_stby"
            };

            var parameterTypePowerPeak = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ShortName = "P_peak"
            };

            var parameterTypePowerDutyCycle = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ShortName = "P_duty_cyc"
            };

            var parameterTypePowerMean = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "P_mean",
                ShortName = "P_mean"
            };

            var parameter = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterType,
                Scale = parameterType.DefaultScale,
                Owner = domain
            };

            this.parameterRedundancy = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterTypeRedundancy,
                Scale = parameterTypePowerOn.DefaultScale,
                Owner = domain
            };

            this.parameterPowerOn = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterTypePowerOn,
                Scale = parameterTypePowerOn.DefaultScale,
                Owner = domain
            };

            this.parameterPowerStandBy = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterTypePowerStandby,
                Scale = parameterTypePowerOn.DefaultScale,
                Owner = domain
            };

            this.parameterPowerPeak = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterTypePowerPeak,
                Scale = parameterTypePowerOn.DefaultScale,
                Owner = domain
            };

            this.parameterPowerPowerDutyCycle = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterTypePowerDutyCycle,
                Scale = parameterTypePowerOn.DefaultScale,
                Owner = domain
            };

            this.parameterPowerMean = new Parameter
            {
                Iid = Guid.NewGuid(),
                ParameterType = parameterTypePowerMean,
                Scale = parameterTypePowerOn.DefaultScale,
                Owner = domain
            };

            this.iteration.Element.Add(elementDefinition);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(parameter);

            var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\TestData\test.xlsx");
            var fileinfo = new FileInfo(sourcePath);

            var targetPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\TestData\temporarytestfile.xlsx");
            var tempfile = fileinfo.CopyTo(targetPath, true);
            this.excelFilePath = tempfile.FullName;
        }

        [TearDown]
        public void TearDown()
        {
            if (System.IO.File.Exists(this.excelFilePath))
            {
                System.IO.File.Delete(this.excelFilePath);
            }
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModel = new CrossViewDialogViewModel(null, this.iteration, this.session.Object, null);

            Assert.AreEqual("Select ElementDefinitions and ParameterTypes", viewModel.DialogTitle);
            Assert.IsInstanceOf<ElementDefinitionSelectorViewModel>(viewModel.ElementSelectorViewModel);
            Assert.IsInstanceOf<ParameterTypeSelectorViewModel>(viewModel.ParameterSelectorViewModel);

            Assert.AreEqual(true, viewModel.PersistValues);

            Assert.DoesNotThrow(() => viewModel.ElementSelectorViewModel.BindData());
            Assert.DoesNotThrow(() => viewModel.ParameterSelectorViewModel.BindData());
        }

        [Test]
        public void VerifyThatOkCommandWorksWithNullWorkbook()
        {
            var viewModel = new CrossViewDialogViewModel(null, this.iteration, this.session.Object, null);

            Assert.IsInstanceOf<ElementDefinitionSelectorViewModel>(viewModel.ElementSelectorViewModel);
            Assert.IsInstanceOf<ParameterTypeSelectorViewModel>(viewModel.ParameterSelectorViewModel);

            Assert.DoesNotThrow(() => viewModel.ElementSelectorViewModel.BindData());
            Assert.DoesNotThrow(() => viewModel.ParameterSelectorViewModel.BindData());

            Assert.DoesNotThrowAsync(async () => await viewModel.OkCommand.Execute());

            Assert.IsTrue(viewModel.DialogResult.Result);
        }

        [Test]
        [Category("OfficeDependent")]
        public void VerifyThatOkCommandWorks()
        {
            var application = new Application();
            var workbook = application.Workbooks.Open(this.excelFilePath, false, false);

            Assert.NotNull(workbook);

            var viewModel = new CrossViewDialogViewModel(application, this.iteration, this.session.Object, workbook);

            Assert.IsInstanceOf<ElementDefinitionSelectorViewModel>(viewModel.ElementSelectorViewModel);
            Assert.IsInstanceOf<ParameterTypeSelectorViewModel>(viewModel.ParameterSelectorViewModel);

            Assert.DoesNotThrow(() => viewModel.ElementSelectorViewModel.BindData());
            Assert.DoesNotThrow(() => viewModel.ParameterSelectorViewModel.BindData());

            Assert.DoesNotThrowAsync(async () => await viewModel.OkCommand.Execute());

            Assert.IsTrue(viewModel.DialogResult.Result);

            workbook.Close();
            workbook.Dispose();

            application.Quit();
            application.Dispose();
        }

        [Test]
        public void VerifyThatCancelCommandWorks()
        {
            var viewModel = new CrossViewDialogViewModel(null, this.iteration, this.session.Object, null);

            Assert.DoesNotThrowAsync(async () => await viewModel.CancelCommand.Execute());

            Assert.IsFalse(viewModel.DialogResult.Result);
        }

        [Test]
        public void VerifyThatPowerCommandWorks()
        {
            var parameterTypeSelectorViewModel = new ParameterTypeSelectorViewModel(
                this.iteration,
                this.session.Object,
                new List<Guid>());

            Assert.Zero(parameterTypeSelectorViewModel.ParameterTypeSourceList.Count);

            parameterTypeSelectorViewModel.BindData();

            Assert.AreEqual(0, parameterTypeSelectorViewModel.SelectedSourceList.Count);
            Assert.AreEqual(0, parameterTypeSelectorViewModel.SelectedTargetList.Count);

            Assert.AreEqual(1, parameterTypeSelectorViewModel.ParameterTypeSourceList.Count);
            Assert.AreEqual(0, parameterTypeSelectorViewModel.ParameterTypeTargetList.Count);

            parameterTypeSelectorViewModel.SelectedSourceList.Add(parameterTypeSelectorViewModel.ParameterTypeSourceList.FirstOrDefault());
            parameterTypeSelectorViewModel.ExecuteMoveToTarget();

            Assert.AreEqual(0, parameterTypeSelectorViewModel.ParameterTypeSourceList.Count);
            Assert.AreEqual(1, parameterTypeSelectorViewModel.ParameterTypeTargetList.Count);

            parameterTypeSelectorViewModel.SelectedTargetList.Add(parameterTypeSelectorViewModel.ParameterTypeTargetList.FirstOrDefault());
            parameterTypeSelectorViewModel.ExecuteMoveToSource();

            Assert.AreEqual(1, parameterTypeSelectorViewModel.ParameterTypeSourceList.Count);
            Assert.AreEqual(0, parameterTypeSelectorViewModel.ParameterTypeTargetList.Count);

            Assert.AreEqual(0, parameterTypeSelectorViewModel.SelectedSourceList.Count);
            Assert.AreEqual(0, parameterTypeSelectorViewModel.SelectedTargetList.Count);
        }

        [Test]
        public void VerifyThatTogglePowerCommandWorks()
        {
            // Add 6 power related parameters
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(this.parameterRedundancy);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(this.parameterPowerOn);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(this.parameterPowerStandBy);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(this.parameterPowerPeak);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(this.parameterPowerPowerDutyCycle);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(this.parameterPowerMean);

            var parameterTypeSelectorViewModel = new ParameterTypeSelectorViewModel(
                this.iteration,
                this.session.Object,
                this.preservedParametersIids);

            Assert.Zero(parameterTypeSelectorViewModel.ParameterTypeSourceList.Count);

            parameterTypeSelectorViewModel.BindData();

            parameterTypeSelectorViewModel.PowerParametersEnabled = true;
            Assert.DoesNotThrowAsync(async () => await parameterTypeSelectorViewModel.PowerParametersCommand.Execute());
            Assert.IsTrue(parameterTypeSelectorViewModel.PowerParametersEnabled);

            parameterTypeSelectorViewModel.PowerParametersEnabled = false;
            Assert.DoesNotThrowAsync(async () => await parameterTypeSelectorViewModel.PowerParametersCommand.Execute());
            Assert.IsFalse(parameterTypeSelectorViewModel.PowerParametersEnabled);
        }
    }
}
