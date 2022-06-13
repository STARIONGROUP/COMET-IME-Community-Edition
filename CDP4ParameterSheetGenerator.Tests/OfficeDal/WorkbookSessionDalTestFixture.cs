// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSessionDalTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.OfficeDal
{
    using System;
    using System.IO;
    using CDP4Common.MetaInfo;
    using CDP4OfficeInfrastructure.OfficeDal;
    using CommonServiceLocator;
    using Moq;
    using NetOffice.ExcelApi;
    using NUnit.Framework;
    using File = System.IO.File;

    /// <summary>
    /// Suite of tests for the <see cref="WorkbookSessionDal"/> class
    /// </summary>
    [TestFixture]
    public class WorkbookSessionDalTestFixture
    {
        private string testfilepath;
        private Mock<IServiceLocator> serviceLocator;

        private WorkbookSession workbookSession;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => this.serviceLocator.Object));
            this.serviceLocator.Setup(x => x.GetInstance<IMetaDataProvider>()).Returns(new MetaDataProvider());

            var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\test.xlsx");
            var fileinfo = new FileInfo(sourcePath);

            var targetPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData\temporarytestfile.xlsx");
            var tempfile = fileinfo.CopyTo(targetPath, true);
            
            this.testfilepath = tempfile.FullName;

            var siteDirectory = new CDP4Common.SiteDirectoryData.SiteDirectory(Guid.NewGuid(), null, null);
            
            var engineeringModelSetup = new CDP4Common.SiteDirectoryData.EngineeringModelSetup(Guid.NewGuid(), null, null)
                                            {
                                                Name = "test model",
                                                ShortName = "testmodel"
                                            };
            siteDirectory.Model.Add(engineeringModelSetup);

            var iterationSetup = new CDP4Common.SiteDirectoryData.IterationSetup(Guid.NewGuid(), null, null)
                                     {
                                         IterationIid = Guid.NewGuid()
                                     };
            engineeringModelSetup.IterationSetup.Add(iterationSetup);
            
            var person = new CDP4Common.SiteDirectoryData.Person(Guid.NewGuid(), null, null)
                             {
                                 GivenName = "test",
                                 Surname = "user"
                             };
            siteDirectory.Person.Add(person);

            var domainOfExpertise = new CDP4Common.SiteDirectoryData.DomainOfExpertise(Guid.NewGuid(), null, null);
            siteDirectory.Domain.Add(domainOfExpertise);

            this.workbookSession = new WorkbookSession(person, engineeringModelSetup,iterationSetup, domainOfExpertise);
            this.workbookSession.RebuildDateTime = DateTime.Now;
        }

        [TearDown]        
        public void TearDown()
        {
            File.Delete(this.testfilepath);
        }

        [Test]
        public void VerifyThatArgementNotNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkbookSessionDal(null));
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

                var workbookSessionDal = new WorkbookSessionDal(workbook);
                var retrievedWbSession = workbookSessionDal.Read();
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
            NetOffice.ExcelApi.Application application = null;
            Workbook workbook = null;

            try
            {
                application = new Application();
                workbook = application.Workbooks.Open(this.testfilepath, false, false);

                var workbookSessionDal = new WorkbookSessionDal(workbook);
                workbookSessionDal.Write(this.workbookSession);
                workbook.Save();

                var retrievedWbSession = workbookSessionDal.Read();
                Assert.NotNull(retrievedWbSession);

                Assert.AreEqual(this.workbookSession.RebuildDateTime, retrievedWbSession.RebuildDateTime);
                Assert.AreEqual(this.workbookSession.Person.Iid, retrievedWbSession.Person.Iid);
                Assert.AreEqual(this.workbookSession.DomainOfExpertise.Iid, retrievedWbSession.DomainOfExpertise.Iid);
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