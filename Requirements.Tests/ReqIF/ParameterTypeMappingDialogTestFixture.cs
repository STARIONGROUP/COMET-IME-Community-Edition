// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeMappingDialogTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
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

namespace CDP4Requirements.Tests.ReqIF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReqIFSharp;

    [TestFixture]
    internal class ParameterTypeMappingDialogTestFixture
    {
        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private Mock<IDialogNavigationService> dialogNavigationService;

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        /// <summary>
        /// The <see cref="ISession"/> in which are information shall be written
        /// </summary>
        private Mock<ISession> session;

        private Mock<IPermissionService> permissionService;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;
        private DomainOfExpertise domain;
        private EngineeringModel model;
        private Iteration iteration;
        private Person person;
        private Participant participant;

        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        private Assembler assembler;
        private Uri uri = new Uri("http://test.com");

        private ReqIF reqIf;
        private DatatypeDefinitionString stringDatadef;
        private DatatypeDefinitionBoolean boolDatadef;
        private DatatypeDefinitionDate dateDatadef;
        private DatatypeDefinitionEnumeration enumDatadef;
        private DatatypeDefinitionInteger intDatadef;
        private DatatypeDefinitionReal realDatadef;

        private ParameterType pt;

        private ParameterTypeMappingDialogViewModel dialog;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.modelsetup.RequiredRdl.Add(this.mrdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };

            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationSetup);
            this.sitedir.Domain.Add(this.domain);
            this.model.Iteration.Add(this.iteration);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.sitedir.Person.Add(this.person);
            this.modelsetup.Participant.Add(this.participant);

            this.pt = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl.ParameterType.Add(this.pt);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.reqIf = new ReqIF();
            this.reqIf.Lang = "en";
            var corecontent = new ReqIFContent();
            this.reqIf.CoreContent = corecontent;
            this.stringDatadef = new DatatypeDefinitionString();
            this.boolDatadef = new DatatypeDefinitionBoolean();
            this.intDatadef = new DatatypeDefinitionInteger();
            this.realDatadef = new DatatypeDefinitionReal();
            this.enumDatadef = new DatatypeDefinitionEnumeration();
            this.enumDatadef.SpecifiedValues.Add(new EnumValue { Properties = new EmbeddedValue { Key = 1, OtherContent = "enum1" } });

            this.dateDatadef = new DatatypeDefinitionDate();

            corecontent.DataTypes.Add(this.stringDatadef);
            corecontent.DataTypes.Add(this.boolDatadef);
            corecontent.DataTypes.Add(this.dateDatadef);
            corecontent.DataTypes.Add(this.enumDatadef);
            corecontent.DataTypes.Add(this.intDatadef);
            corecontent.DataTypes.Add(this.realDatadef);

            this.dialog = new ParameterTypeMappingDialogViewModel(this.reqIf.Lang, corecontent.DataTypes, null, this.iteration, this.session.Object, this.thingDialogNavigationService.Object);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [Test]
        public void VerifyThatMenuIsPopulated()
        {
            this.dialog.SelectedRow = this.dialog.MappingRows.First(x => x.Identifiable == this.stringDatadef);
            Assert.That(this.dialog.CreateParameterTypeCommands, Has.Count.EqualTo(4));

            this.dialog.SelectedRow = this.dialog.MappingRows.First(x => x.Identifiable == this.boolDatadef);
            Assert.That(this.dialog.CreateParameterTypeCommands, Has.Count.EqualTo(1));

            this.dialog.SelectedRow = this.dialog.MappingRows.First(x => x.Identifiable == this.enumDatadef);
            Assert.That(this.dialog.CreateParameterTypeCommands, Has.Count.EqualTo(1));

            this.dialog.SelectedRow = this.dialog.MappingRows.First(x => x.Identifiable == this.dateDatadef);
            Assert.That(this.dialog.CreateParameterTypeCommands, Has.Count.EqualTo(3));
        }

        [Test]
        public void VerifyThatEnumValueRowsHaveNoCreateCommands()
        {
            var enumRow = this.dialog.MappingRows.First(x => x.Identifiable == this.enumDatadef);

            // the enumeration datatype row itself can create an Enumeration Parameter Type
            this.dialog.SelectedRow = enumRow;
            Assert.That(this.dialog.CreateParameterTypeCommands, Has.Count.EqualTo(1));

            // but its enum-value child rows (the literals) cannot create a parameter type
            this.dialog.SelectedRow = enumRow.EnumValue.First();
            Assert.That(this.dialog.CreateParameterTypeCommands, Has.Count.EqualTo(0));
        }

        [Test]
        public void VerifyThatCreateGenericParameterTypeWorks()
        {
            this.thingDialogNavigationService.Setup(
                    x => x.Navigate(It.IsAny<ParameterType>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, null, null))
                .Returns(false);

            this.dialog.SelectedRow = this.dialog.MappingRows.First(x => x.Identifiable == this.stringDatadef);
            this.dialog.CreateParameterTypeCommands.First().MenuCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, null, null));
        }

        [Test]
        public void VerifyThatCreateEnumWorks()
        {
            this.thingDialogNavigationService.Setup(
                    x => x.Navigate(It.IsAny<ParameterType>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, null, null))
                .Returns(false);

            this.dialog.SelectedRow = this.dialog.MappingRows.First(x => x.Identifiable == this.enumDatadef);
            this.dialog.CreateParameterTypeCommands.First().MenuCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, null, null));
        }

        [Test]
        public void VerifyThatCreatedEnumValueDefinitionShortNamesAreValidAndUnique()
        {
            var enumDatatypeDefinition = new DatatypeDefinitionEnumeration { LongName = "T_LegalObligation" };
            enumDatatypeDefinition.SpecifiedValues.Add(new EnumValue { LongName = "mandatory (>= 1)", Properties = new EmbeddedValue { Key = 1, OtherContent = "Other content for: mandatory (>= 1)" } });
            enumDatatypeDefinition.SpecifiedValues.Add(new EnumValue { LongName = "mandatory (== 1)", Properties = new EmbeddedValue { Key = 2, OtherContent = "Other content for: mandatory (== 1)" } });
            enumDatatypeDefinition.SpecifiedValues.Add(new EnumValue { LongName = "not applicable", Properties = new EmbeddedValue { Key = 3, OtherContent = "Other content for: not applicable" } });
            enumDatatypeDefinition.SpecifiedValues.Add(new EnumValue { LongName = "ordinary", Properties = new EmbeddedValue { Key = 4, OtherContent = "Other content for: ordinary" } });

            EnumerationParameterType capturedParameterType = null;

            this.thingDialogNavigationService.Setup(
                    x => x.Navigate(It.IsAny<ParameterType>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, null, null))
                .Callback<Thing, IThingTransaction, ISession, bool, ThingDialogKind, IThingDialogNavigationService, Thing, IEnumerable<Thing>>(
                    (thing, transaction, session, isRoot, dialogKind, service, container, chain) => capturedParameterType = thing as EnumerationParameterType)
                .Returns(false);

            var dialog = new ParameterTypeMappingDialogViewModel(this.reqIf.Lang, new DatatypeDefinition[] { enumDatatypeDefinition }, null, this.iteration, this.session.Object, this.thingDialogNavigationService.Object);

            dialog.SelectedRow = dialog.MappingRows.First(x => x.Identifiable == enumDatatypeDefinition);
            dialog.CreateParameterTypeCommands.First().MenuCommand.Execute(null);

            Assert.That(capturedParameterType, Is.Not.Null);

            var shortNamePattern = new Regex("^[a-zA-Z0-9_]+$");
            var shortNames = capturedParameterType.ValueDefinition.Select(x => x.ShortName).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(shortNames, Has.All.Matches<string>(x => shortNamePattern.IsMatch(x)), "all short-names must be valid");
                Assert.That(shortNames.Distinct().Count(), Is.EqualTo(shortNames.Count), "short-names must be unique");
                Assert.That(shortNames, Has.None.Contains("Other content"), "the ReqIF OTHER-CONTENT placeholder must not be used as short-name");

                // the human-readable name is preserved
                Assert.That(capturedParameterType.ValueDefinition.Select(x => x.Name), Does.Contain("mandatory (>= 1)"));
            });
        }

        [Test]
        public void VerifyThatCreateScaleWorks()
        {
            this.thingDialogNavigationService.Setup(
                    x => x.Navigate(It.IsAny<MeasurementScale>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, null, null))
                .Returns(true);

            this.dialog.SelectedRow = this.dialog.MappingRows.First(x => x.Identifiable == this.realDatadef);
            this.dialog.CreateMeasurementScaleCommands.First().MenuCommand.Execute(null);

            this.thingDialogNavigationService.Verify(
                x => x.Navigate(It.IsAny<MeasurementScale>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, null, null));
        }

        [Test]
        public async Task VerifyThatCancelCommandWorks()
        {
            await this.dialog.CancelCommand.Execute();
            Assert.That(this.dialog.DialogResult.Result.Value, Is.False);
        }

        [Test]
        public async Task VerifyThatOkCommandWorks()
        {
            this.dialog = new ParameterTypeMappingDialogViewModel(this.reqIf.Lang, new DatatypeDefinition[] { this.stringDatadef }, null, this.iteration, this.session.Object, this.thingDialogNavigationService.Object);

            foreach (var datatypeDefinitionMappingRowViewModel in this.dialog.MappingRows)
            {
                datatypeDefinitionMappingRowViewModel.MappedThing = this.pt;
            }

            await this.dialog.NextCommand.Execute();
            Assert.That(this.dialog.DialogResult.Result.Value, Is.True);
        }
    }
}
