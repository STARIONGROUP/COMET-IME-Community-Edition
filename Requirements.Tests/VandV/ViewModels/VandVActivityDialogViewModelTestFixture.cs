// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Windows.Input;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.ViewModels;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="VandVActivityDialogViewModel"/>, defaults, validation and edit mode.
    /// </summary>
    [TestFixture]
    public class VandVActivityDialogViewModelTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private CDPMessageBus messageBus;
        private Assembler assembler;
        private Mock<ISession> session;
        private Iteration iteration;
        private RequirementsSpecification specification;
        private DomainOfExpertise domain;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session = new Mock<ISession>();

            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SITERDL" };
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "MRDL", RequiredRdl = srdl };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "SYS", Name = "System" };

            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            modelSetup.RequiredRdl.Add(mrdl);
            modelSetup.ActiveDomain.Add(this.domain);

            var model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = modelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            model.Iteration.Add(this.iteration);

            this.specification = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "VNV", Owner = this.domain };
            this.iteration.RequirementsSpecification.Add(this.specification);

            this.session
                .Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) } });
        }

        [Test]
        public void VerifyThatANewDialogSuggestsAFreeActivityNumber()
        {
            var vm = new VandVActivityDialogViewModel(this.iteration, this.session.Object);

            Assert.That(vm.IsEditMode, Is.False);
            Assert.That(vm.Title, Is.EqualTo("Create V&V Activity"));
            Assert.That(vm.ShortName, Is.EqualTo("ACT_1"));
            Assert.That(vm.Owner, Is.EqualTo(this.domain));
            Assert.That(vm.PossibleMethods, Is.Not.Empty, "the manifest defaults must back an unseeded library");

            this.specification.Requirement.Add(new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ACT_1" });

            Assert.That(new VandVActivityDialogViewModel(this.iteration, this.session.Object).ShortName, Is.EqualTo("ACT_2"));
        }

        [Test]
        public void VerifyThatValidationRejectsADuplicateOrInvalidNumber()
        {
            this.specification.Requirement.Add(new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "ACT_42" });

            var vm = new VandVActivityDialogViewModel(this.iteration, this.session.Object);

            vm.ShortName = "ACT_42";
            Assert.That(vm[nameof(vm.ShortName)], Does.Contain("already uses"));

            vm.ShortName = "ACT 43";
            Assert.That(vm[nameof(vm.ShortName)], Does.Contain("letters, digits and underscores"));

            vm.ShortName = "ACT_43";
            Assert.That(vm[nameof(vm.ShortName)], Is.Empty);
        }

        [Test]
        public void VerifyThatMethodAndStageAreMandatoryOnAnActivity()
        {
            var vm = new VandVActivityDialogViewModel(this.iteration, this.session.Object) { Name = "Produce mass budget" };

            Assert.That(vm[nameof(vm.Method)], Does.Contain("mandatory"));
            Assert.That(vm[nameof(vm.Stage)], Does.Contain("mandatory"));
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.False);

            vm.Method = "Analysis";
            vm.Stage = "CDR";

            Assert.That(vm[nameof(vm.Method)], Is.Empty);
            Assert.That(vm[nameof(vm.Stage)], Is.Empty);
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);
        }

        [Test]
        public void VerifyThatEditModeLoadsTheActivityAndItsReport()
        {
            var report = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = "FAT_Report", Name = "FAT Report", Owner = this.domain };
            report.Category.Add(new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVCategory.VnVReport });
            this.iteration.RequirementsSpecification.Add(report);

            var activity = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ShortName = "ACT_1",
                Name = "Produce mass budget",
                Owner = this.domain
            };

            activity.ParameterValue.Add(new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { ShortName = VandVParameter.Method },
                Value = new ValueArray<string>(new[] { "Analysis" })
            });

            report.Requirement.Add(activity);

            var vm = new VandVActivityDialogViewModel(this.iteration, this.session.Object, activity);

            Assert.That(vm.IsEditMode, Is.True);
            Assert.That(vm.ShortName, Is.EqualTo("ACT_1"));
            Assert.That(vm.Report, Is.EqualTo(report), "the report is the activity's containing specification");
            Assert.That(vm.PossibleReports, Does.Contain(report), "only existing reports are offered");
            Assert.That(vm.Method, Is.EqualTo("Analysis"));
            Assert.That(vm.BuildAttributes()[VandVParameter.Method], Is.EqualTo("Analysis"));
        }
    }
}
