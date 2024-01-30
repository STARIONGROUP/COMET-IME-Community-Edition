// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewWorkbookDataTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4OfficeInfrastructure.Tests.OfficeDal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CDP4OfficeInfrastructure.OfficeDal;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossviewWorkbookData"/> class.
    /// </summary>
    [TestFixture]
    public class CrossviewWorkbookDataTestFixture
    {
        private Iteration iteration;
        private Mock<IServiceLocator> serviceLocator;

        private readonly Credentials credentials = new Credentials(
            "John",
            "Doe",
            new Uri("http://www.rheagroup.com/"));

        private Assembler assembler;
        private Mock<ISession> session;

        private SiteDirectory siteDir;
        private Person person;
        private SiteReferenceDataLibrary genericRdl;
        private SiteReferenceDataLibrary siteRdl;

        private ElementDefinition elementDefinition;
        private SimpleQuantityKind parameterType;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMetaDataProvider>()).Returns(new MetaDataProvider());

            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.credentials.Uri, this.messageBus);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri);
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri) { Container = this.siteDir };
            this.siteDir.Person.Add(this.person);

            this.genericRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri) { Name = "Generic RDL", ShortName = "GenRDL" };
            this.siteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri) { RequiredRdl = this.genericRdl };
            this.siteDir.SiteReferenceDataLibrary.Add(this.siteRdl);

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

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.session.Object.Assembler.Cache, this.session.Object.Credentials.Uri)
            {
                Name = "ElementDefinition",
                ShortName = "ED",
                Container = this.iteration,
                Owner = domain
            };

            this.parameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "SimpleQuantityKind",
                ShortName = "SQK",
            };

            this.siteRdl.ParameterType.Add(this.parameterType);

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = this.parameterType,
                Scale = this.parameterType.DefaultScale,
                Owner = domain
            };

            this.iteration.Element.Add(this.elementDefinition);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(parameter);
        }

        [Test]
        public void VefrifyThatWorkbookDataSerializes()
        {
            string xmlstringpass1;
            string xmlstringpass2;

            var elements = new List<ElementDefinition>
            {
                this.elementDefinition
            };

            var parameterTypes = new List<ParameterType>
            {
                this.parameterType
            };

            var manualyValues = new Dictionary<string, string> { { parameterTypes[0].Iid.ToString(), "1" } };

            var workbookDataPass1 = new CrossviewWorkbookData(
                elements.Select(ed => ed.Iid),
                parameterTypes.Select(pt => pt.Iid),
                manualyValues);

            CrossviewWorkbookData workbookDataPass2;

            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(CrossviewWorkbookData));
                serializer.Serialize(writer, workbookDataPass1);
                xmlstringpass1 = writer.ToString();
            }

            using (var reader = new StringReader(xmlstringpass1))
            {
                var serializer = new XmlSerializer(typeof(CrossviewWorkbookData));
                workbookDataPass2 = serializer.Deserialize(reader) as CrossviewWorkbookData;
            }

            Assert.NotNull(workbookDataPass2);

            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(CrossviewWorkbookData));
                serializer.Serialize(writer, workbookDataPass2);
                xmlstringpass2 = writer.ToString();
            }

            xmlstringpass1 = xmlstringpass1.Replace("\r\n", "\n");
            xmlstringpass2 = xmlstringpass2.Replace("\r\n", "\n");

            Assert.AreEqual(xmlstringpass1, xmlstringpass2);
        }
    }
}
