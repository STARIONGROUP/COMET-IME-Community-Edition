// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookDataDalTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.OfficeDal
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using CDP4Common.MetaInfo;
    using CDP4OfficeInfrastructure.OfficeDal;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NetOffice.ExcelApi;
    using NUnit.Framework;
    using File = System.IO.File;

    /// <summary>
    /// Suite of tests for the <see cref="WorkbookData"/> class
    /// </summary>
    [TestFixture]
    public class WorkbookDataDalTestFixture
    {
        private string testfilepath;

        private WorkbookData workbookData;
        private Mock<IServiceLocator> serviceLocator;

        private CDP4Common.EngineeringModelData.Iteration iteration;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => this.serviceLocator.Object));
            this.serviceLocator.Setup(x => x.GetInstance<IMetaDataProvider>()).Returns(new MetaDataProvider());
            var fileinfo = new FileInfo("TestData\\test.xlsx");
            var tempfile = fileinfo.CopyTo("TestData\\temporarytestfile.xlsx", true);
            this.testfilepath = tempfile.FullName;

            this.InstantiateThings();  
          
            this.workbookData = new WorkbookData(this.iteration);
        }

        private void InstantiateThings()
        {
            var siteDirectory = new CDP4Common.SiteDirectoryData.SiteDirectory(Guid.NewGuid(), null, null);

            var domain1 = new CDP4Common.SiteDirectoryData.DomainOfExpertise(Guid.NewGuid(), null, null);
            siteDirectory.Domain.Add(domain1);
            var domain2 = new CDP4Common.SiteDirectoryData.DomainOfExpertise(Guid.NewGuid(), null, null);
            siteDirectory.Domain.Add(domain2);

            var alias = new CDP4Common.CommonData.Alias(Guid.NewGuid(), null, null);
            domain1.Alias.Add(alias);

            var engineeringModelSetup = new CDP4Common.SiteDirectoryData.EngineeringModelSetup(Guid.NewGuid(), null, null);
            siteDirectory.Model.Add(engineeringModelSetup);
            var iterationSetup = new CDP4Common.SiteDirectoryData.IterationSetup(Guid.NewGuid(), null, null);
            engineeringModelSetup.IterationSetup.Add(iterationSetup);

            var participantRole1 = new CDP4Common.SiteDirectoryData.ParticipantRole(Guid.NewGuid(), null, null);
            siteDirectory.ParticipantRole.Add(participantRole1);
            var participantRole2 = new CDP4Common.SiteDirectoryData.ParticipantRole(Guid.NewGuid(), null, null);
            siteDirectory.ParticipantRole.Add(participantRole2);

            var person1 = new CDP4Common.SiteDirectoryData.Person(Guid.NewGuid(), null, null);
            siteDirectory.Person.Add(person1);

            var personRole1 = new CDP4Common.SiteDirectoryData.PersonRole(Guid.NewGuid(), null, null);
            siteDirectory.PersonRole.Add(personRole1);

            var siteReferenceDataLibrary1 = new CDP4Common.SiteDirectoryData.SiteReferenceDataLibrary(Guid.NewGuid(), null, null);
            siteDirectory.SiteReferenceDataLibrary.Add(siteReferenceDataLibrary1);

            var engineeringModel = new CDP4Common.EngineeringModelData.EngineeringModel(Guid.NewGuid(), null, null);
            engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            engineeringModelSetup.EngineeringModelIid = engineeringModel.Iid;

            this.iteration = new CDP4Common.EngineeringModelData.Iteration(Guid.NewGuid(), null, null);
            this.iteration.IterationSetup = iterationSetup;
            iterationSetup.IterationIid = this.iteration.Iid;

            engineeringModel.Iteration.Add(this.iteration);

            var modelReferenceDataLibrary = new CDP4Common.SiteDirectoryData.ModelReferenceDataLibrary(Guid.NewGuid(), null, null);
            modelReferenceDataLibrary.RequiredRdl = siteReferenceDataLibrary1;

            engineeringModelSetup.RequiredRdl.Add(modelReferenceDataLibrary);

            var participant = new CDP4Common.SiteDirectoryData.Participant(Guid.NewGuid(), null, null);
            participant.Person = person1;
            participant.Domain.Add(domain1);
            participant.Role = participantRole1;

            engineeringModelSetup.Participant.Add(participant);

            var elementDefinition1 = new CDP4Common.EngineeringModelData.ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(elementDefinition1);
            var elementDefinition2 = new CDP4Common.EngineeringModelData.ElementDefinition(Guid.NewGuid(), null, null);
            this.iteration.Element.Add(elementDefinition2);

            var elementUsage = new CDP4Common.EngineeringModelData.ElementUsage(Guid.NewGuid(), null, null);
            elementDefinition1.ContainedElement.Add(elementUsage);
            elementUsage.ElementDefinition = elementDefinition2;
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(this.testfilepath);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyThatArgementNotNullExceptionIsThrown()
        {
            var workbookDataDal = new WorkbookDataDal(null);
        }

        [Test]
        [Category("OfficeDependent")]
        public void VerifyThatIfCustomXmlPartDoesNotExistsTheWorkbookSessionIsNull()
        {
            NetOffice.ExcelApi.Application application = null;
            Workbook workbook = null;

            try
            {
                application = new Application();
                workbook = application.Workbooks.Open(this.testfilepath, false, false);

                var workbookDataDal = new WorkbookDataDal(workbook);
                var retrievedWbSession = workbookDataDal.Read();
                Assert.IsNull(retrievedWbSession);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                if (workbook != null)
                {
                    Console.WriteLine("Closing workbook {0}", this.testfilepath);
                    workbook.Close();
                    workbook.Dispose();
                }

                if (application != null)
                {
                    Console.WriteLine("Closing Excel Application");
                    application.Quit();
                    application.Dispose();
                }
            }
        }

        [Test]
        [Category("OfficeDependent")]
        public void VerifyThatTheSessionDataIsWrittenToAWorkbookandCanBeRetrieved()
        {
            var sw = new Stopwatch();

            NetOffice.ExcelApi.Application application = null;
            Workbook workbook = null;

            try
            {
                sw.Start();
                application = new Application();
                Console.WriteLine("Excel application started in " + sw.ElapsedMilliseconds); 

                workbook = application.Workbooks.Open(this.testfilepath, false, false);

                var workbookDataDal = new WorkbookDataDal(workbook);
                workbookDataDal.Write(this.workbookData);
                workbook.Save();

                var retrievedWbData = workbookDataDal.Read();

                Assert.NotNull(retrievedWbData);

                Assert.IsNotEmpty(retrievedWbData.SitedirectoryData.Value);

                var things = retrievedWbData.SiteDirectoryThings;

                var iterationDto = things.SingleOrDefault(x => x.Iid == this.iteration.Iid);
                Assert.IsNotNull(iterationDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                if (workbook != null)
                {
                    Console.WriteLine("Closing workbook {0}", this.testfilepath);
                    workbook.Close();
                    workbook.Dispose();
                }

                if (application != null)
                {
                    Console.WriteLine("Closing Excel Application");
                    application.Quit();
                    application.Dispose();
                }
            }
        }
    }
}
