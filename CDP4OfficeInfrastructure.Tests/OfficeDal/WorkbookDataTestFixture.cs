// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookDataTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Tests.OfficeDal
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4OfficeInfrastructure.OfficeDal;
    using CommonServiceLocator;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="WorkbookData"/> class.
    /// </summary>
    [TestFixture]
    public class WorkbookDataTestFixture
    {
        private Iteration iteration;
        private Mock<IServiceLocator> serviceLocator;
        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => this.serviceLocator.Object));
            this.serviceLocator.Setup(x => x.GetInstance<IMetaDataProvider>()).Returns(new MetaDataProvider());
            var siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);

            var domain1 = new DomainOfExpertise(Guid.NewGuid(), null, null);
            siteDirectory.Domain.Add(domain1);
            var domain2 = new DomainOfExpertise(Guid.NewGuid(), null, null);
            siteDirectory.Domain.Add(domain2);

            var alias = new Alias(Guid.NewGuid(), null, null);
            domain1.Alias.Add(alias);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, null);
            siteDirectory.Model.Add(engineeringModelSetup);
            var iterationSetup = new IterationSetup(Guid.NewGuid(), null, null);
            engineeringModelSetup.IterationSetup.Add(iterationSetup);

            var participantRole1 = new ParticipantRole(Guid.NewGuid(), null, null);
            siteDirectory.ParticipantRole.Add(participantRole1);
            var participantRole2 = new ParticipantRole(Guid.NewGuid(), null, null);
            siteDirectory.ParticipantRole.Add(participantRole2);

            var person1 = new Person(Guid.NewGuid(), null, null);
            siteDirectory.Person.Add(person1);
            
            var personRole1 = new PersonRole(Guid.NewGuid(), null, null);
            siteDirectory.PersonRole.Add(personRole1);
            
            var siteReferenceDataLibrary1 = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            siteDirectory.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary1);
            
            var engineeringModel = new EngineeringModel(Guid.NewGuid(), null, null);
            engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            engineeringModelSetup.EngineeringModelIid = engineeringModel.Iid;

            this.iteration = new Iteration(Guid.NewGuid(), null, null);
            this.iteration.IterationSetup = iterationSetup;
            iterationSetup.IterationIid = this.iteration.Iid;

            engineeringModel.Iteration.Add(this.iteration);

            var modelReferenceDataLibrary = new ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.RequiredRdl = siteReferenceDataLibrary1;

            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);

            var participant = new Participant(Guid.NewGuid(), null, null);
            participant.Person = person1;
            participant.Domain.Add(domain1);
            participant.Role = participantRole1;

            engineeringModelSetup.Participant.Add(participant);

            var elementDefinition1 = new ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(elementDefinition1);
            var elementDefinition2 = new ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(elementDefinition2);

            var elementUsage = new ElementUsage(Guid.NewGuid(), null, null);
            elementDefinition1.ContainedElement.Add(elementUsage);
            elementUsage.ElementDefinition = elementDefinition2;
        }

        [Test]
        public void VefrifyThatWorkbookDataSerializes()
        {
            string xmlstringpass1 = string.Empty;
            string xmlstringpass2 = string.Empty;

            var settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.NewLineHandling = NewLineHandling.Entitize;

            var workbookDataPass1 = new WorkbookData(this.iteration);
            WorkbookData workbookDataPass2;

            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(WorkbookData));
                serializer.Serialize(writer, workbookDataPass1);
                xmlstringpass1 = writer.ToString();
                Console.WriteLine(xmlstringpass1);
            }

            using (var reader = new StringReader(xmlstringpass1))
            {
                var serializer = new XmlSerializer(typeof(WorkbookData));
                workbookDataPass2 = (WorkbookData)serializer.Deserialize(reader);
            }

            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(WorkbookData));
                serializer.Serialize(writer, workbookDataPass2);
                xmlstringpass2 = writer.ToString();
                Console.WriteLine(xmlstringpass2);
            }

            xmlstringpass1 = xmlstringpass1.Replace("\r\n", "\n");
            xmlstringpass2 = xmlstringpass2.Replace("\r\n", "\n");

            Assert.AreEqual(xmlstringpass1, xmlstringpass2);
        }
    }
}
