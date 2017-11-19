// -----------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlExportRequirementsSpecificationSelectionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.HtmlReport
{
    using System.Collections.Generic;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Requirements.ViewModels;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="HtmlExportRequirementsSpecificationSelectionDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class HtmlExportRequirementsSpecificationSelectionDialogViewModelTestFixture
    {
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private List<Iteration> possibleIterations;
        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private RequirementsSpecification requirementsSpecification;

        [SetUp]
        public void SetUp()
        {
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.possibleIterations = new List<Iteration>();
            
            var engineeringModelSetup = new EngineeringModelSetup() { ShortName = "TESTEM" };
            var iterationSetup = new IterationSetup() { IterationNumber = 1 };

            this.engineeringModel = new EngineeringModel { EngineeringModelSetup = engineeringModelSetup };
            this.iteration = new Iteration { IterationSetup = iterationSetup };

            this.engineeringModel.Iteration.Add(this.iteration);
            
            this.requirementsSpecification = new RequirementsSpecification {ShortName = "REQSPEC" };
            this.iteration.RequirementsSpecification.Add(this.requirementsSpecification);

            this.possibleIterations.Add(this.iteration);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatDialogCanBeConstructedAndPropertiesAreSet()
        {
            var vm = new HtmlExportRequirementsSpecificationSelectionDialogViewModel(this.possibleIterations, this.fileDialogService.Object);

            Assert.AreEqual(this.engineeringModel, vm.SelectedEngineeringModel);
            Assert.AreEqual(this.iteration, vm.SelectedIteration);
            Assert.AreEqual(this.requirementsSpecification, vm.SelectedRequirementsSpecification);
        }
    }
}
