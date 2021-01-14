// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewWorkbookDataDalTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Tests.OfficeDal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CDP4OfficeInfrastructure.OfficeDal;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NetOffice.ExcelApi;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossviewWorkbookData"/> class
    /// </summary>
    [TestFixture]
    public class CrossviewWorkbookDataDalTestFixture
    {
        private string excelFilePath;
        private CrossviewWorkbookData workbookData;
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

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMetaDataProvider>()).Returns(new MetaDataProvider());

            var sourcePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\TestData\test.xlsx");
            var fileinfo = new FileInfo(sourcePath);

            var targetPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\TestData\temporarytestfile.xlsx");
            var tempfile = fileinfo.CopyTo(targetPath, true);
            this.excelFilePath = tempfile.FullName;

            this.InstantiateThings();

            var elements = new List<ElementDefinition>
            {
                this.elementDefinition
            };

            var parameterTypes = new List<ParameterType>
            {
                this.parameterType
            };

            this.workbookData = new CrossviewWorkbookData(elements, parameterTypes);
        }

        private void InstantiateThings()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMetaDataProvider>()).Returns(new MetaDataProvider());

            this.assembler = new Assembler(this.credentials.Uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

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

            var parameter = new CDP4Common.EngineeringModelData.Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = this.parameterType,
                Scale = this.parameterType.DefaultScale,
                Owner = domain
            };

            this.iteration.Element.Add(this.elementDefinition);
            this.iteration.Element.FirstOrDefault()?.Parameter.Add(parameter);
        }

        [TearDown]
        public void TearDown()
        {
            System.IO.File.Delete(this.excelFilePath);
        }

        [Test]
        public void VerifyThatArgementNotNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new CrossviewWorkbookDataDal(null));
        }

        [Test]
        [Category("OfficeDependent")]
        public void VerifyThatIfCustomXmlPartDoesNotExistsTheWorkbookSessionIsNull()
        {
            var application = new Application();
            var workbook = application.Workbooks.Open(this.excelFilePath, false, false);

            Assert.NotNull(workbook);

            try
            {
                var workbookDataDal = new WorkbookDataDal(workbook);
                var retrievedSession = workbookDataDal.Read();

                Assert.IsNull(retrievedSession);
            }
            finally
            {
                workbook.Close();
                workbook.Dispose();

                application.Quit();
                application.Dispose();
            }
        }

        [Test]
        [Category("OfficeDependent")]
        public void VerifyThatTheSessionDataIsWrittenToWorkbookAndPersist()
        {
            var application = new Application();
            var workbook = application.Workbooks.Open(this.excelFilePath, false, false);

            Assert.NotNull(workbook);

            try
            {
                var workbookDataDal = new CrossviewWorkbookDataDal(workbook);
                workbookDataDal.Write(this.workbookData);
                workbook.Save();

                var retrievedSession = workbookDataDal.Read();

                Assert.NotNull(retrievedSession);

                Assert.IsNotEmpty(retrievedSession.ElementDefinitionList);
                Assert.IsNotEmpty(retrievedSession.ParameterTypeList);
            }
            finally
            {
                workbook.Close();
                workbook.Dispose();

                application.Quit();
                application.Dispose();
            }
        }
    }
}
