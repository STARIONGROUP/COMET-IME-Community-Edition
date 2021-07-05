// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewPMeanArrayAssemblerTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4CrossViewEditor.Tests.Assemblers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CrossViewEditor.Assemblers;
    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CrossviewArrayAssembler"/> class
    /// </summary>
    [TestFixture]
    public class CrossviewPMeanArrayAssemblerTestFixture
    {
        private List<IExcelRow<Thing>> excelRows;

        private SiteDirectory siteDirectory;
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private DomainOfExpertise domain;
        private Person person;
        private Participant participant;

        private EngineeringModel engineeringModel;
        private Iteration iteration;

        private ElementDefinition elementDefinition;

        private Parameter pDutyCycleParameter;
        private Parameter pDutyCycleParameterWithMinusOneValue;
        private Parameter pDutyCycleParameterWithInvalidValue;
        private Parameter pRedundancyActiveInternalParameter;
        private Parameter pRedundancyActiveExternalParameter;
        private Parameter pStandByParameter;
        private Parameter pOnParameter;
        private Parameter pMeanParameter;

        private readonly Credentials credentials = new Credentials(
            "John",
            "Doe",
            new Uri("http://www.rheagroup.com/"));

        private Assembler assembler;
        private Mock<ISession> session;

        [SetUp]
        public void SetUp()
        {
            this.assembler = new Assembler(this.credentials.Uri);

            this.SetUpThings();
            this.SetUpMethods();
            this.SetUpRows();
        }

        private void SetUpThings()
        {
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri);
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(
                Guid.NewGuid(),
                this.assembler.Cache,
                this.credentials.Uri)
            {
                Name = "testRDL",
                ShortName = "test"
            };

            this.siteDirectory.SiteReferenceDataLibrary.Add(this.siteReferenceDataLibrary);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "Domain"
            };

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                GivenName = "John",
                Surname = "Doe"
            };

            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Person = this.person,
                Domain = { this.domain }
            };

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "Test Model",
                    ShortName = "TestModel",
                    StudyPhase = StudyPhaseKind.DESIGN_SESSION_PHASE,
                    Kind = EngineeringModelKind.STUDY_MODEL,
                    Participant = { this.participant }
                }
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Container = this.engineeringModel,
                IterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    IterationNumber = 1
                }
            };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "ElementDefinition_1",
                ShortName = "ED_1",
                Container = this.iteration,
                Owner = this.domain
            };

            var possibleFiniteState = new PossibleFiniteState(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "PossibleState_1",
                ShortName = "PS_1"
            };

            var possibleList = new PossibleFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "PossibleFiniteStateList_1",
                ShortName = "PFSL_1",
                PossibleState = { possibleFiniteState }
            };

            var actualFiniteState = new ActualFiniteState(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                PossibleState = { possibleFiniteState },
                Kind = ActualFiniteStateKind.MANDATORY
            };

            var actualList = new ActualFiniteStateList(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                PossibleFiniteStateList = { possibleList },
                ActualState = { actualFiniteState }
            };

            var pRedundancy = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
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

            this.pRedundancyActiveInternalParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pRedundancy,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "Active", "Internal", "2", "3" }),
                        Published = new ValueArray<string>(new List<string> { "Active", "Internal", "2", "3" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            this.pRedundancyActiveExternalParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pRedundancy,
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "Active", "External", "2", "3" }),
                        Published = new ValueArray<string>(new List<string> { "Active", "External", "2", "3" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var pOn = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "p_on",
                ShortName = "p_on"
            };

            this.pOnParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pOn,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "11" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var pStby = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "p_stby",
                ShortName = "p_stby"
            };

            this.pStandByParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pStby,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "10" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL
                    }
                }
            };

            var pDutyCyc = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "p_duty_cyc",
                ShortName = "p_duty_cyc"
            };

            this.pDutyCycleParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pDutyCyc,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "0.1" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = actualFiniteState
                    }
                }
            };

            this.pDutyCycleParameterWithMinusOneValue = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pDutyCyc,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "-1" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = actualFiniteState
                    }
                }
            };

            this.pDutyCycleParameterWithInvalidValue = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pDutyCyc,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "-0.5" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = actualFiniteState
                    }
                }
            };

            var pMean = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "p_mean",
                ShortName = "p_mean"
            };

            this.pMeanParameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                ParameterType = pMean,
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                {
                    Name = "W",
                    ShortName = "W"
                },
                Owner = this.domain,
                StateDependence = actualList,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
                    {
                        Manual = new ValueArray<string>(new List<string> { "-" }),
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        ActualState = actualFiniteState
                    }
                }
            };

            this.elementDefinition.Parameter.Add(this.pRedundancyActiveInternalParameter);
            this.elementDefinition.Parameter.Add(this.pOnParameter);
            this.elementDefinition.Parameter.Add(this.pStandByParameter);
            this.elementDefinition.Parameter.Add(this.pDutyCycleParameter);
            this.elementDefinition.Parameter.Add(this.pMeanParameter);

            this.iteration.Element.Add(this.elementDefinition);

            this.siteReferenceDataLibrary.ParameterType.Add(pRedundancy);
            this.siteReferenceDataLibrary.ParameterType.Add(pOn);
            this.siteReferenceDataLibrary.ParameterType.Add(pStby);
            this.siteReferenceDataLibrary.ParameterType.Add(pDutyCyc);
            this.siteReferenceDataLibrary.ParameterType.Add(pMean);
        }

        private void SetUpMethods()
        {
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Credentials).Returns(this.credentials);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.session
                .Setup(x => x.OpenIterations)
                .Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
                {
                    { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
                });

            this.session
                .Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>()))
                .Returns(this.domain);
        }

        private void SetUpRows()
        {
            this.excelRows = new List<IExcelRow<Thing>>();

            var crossviewSheetRowAssembler = new CrossviewSheetRowAssembler();
            crossviewSheetRowAssembler.Assemble(this.iteration, new[] { this.elementDefinition.Iid });
            this.excelRows.AddRange(crossviewSheetRowAssembler.ExcelRows);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfRedundancyIsMissing()
        {
            this.elementDefinition.Parameter.RemoveAll(p => p.ParameterType.ShortName == "redundancy");

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsFalse(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(9, arrayAssembler.ContentArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 8]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 8]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfPOnIsMissing()
        {
            this.elementDefinition.Parameter.RemoveAll(p => p.ParameterType.ShortName == "p_on");

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsFalse(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 11]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfPStandByIsMissing()
        {
            this.elementDefinition.Parameter.RemoveAll(p => p.ParameterType.ShortName == "p_stby");

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsFalse(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 11]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfPDutyCycIsMissing()
        {
            this.elementDefinition.Parameter.RemoveAll(p => p.ParameterType.ShortName == "p_duty_cyc");

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsFalse(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 11]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfPMeanIsMissing()
        {
            this.elementDefinition.Parameter.RemoveAll(p => p.ParameterType.ShortName == "p_mean");

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsFalse(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfPDutyCycleAndPMeanHaveDifferentOptionDependency()
        {
            var pDutyCyc = this.elementDefinition.Parameter.Single(p => p.ParameterType.ShortName == "p_duty_cyc");

            pDutyCyc.IsOptionDependent = true;

            var option1 = new Option(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri)
            {
                Name = "Option 1",
                ShortName = "OPT_1",
            };

            foreach (var parameterValueSet in pDutyCyc.ValueSet)
            {
                parameterValueSet.ActualOption = option1;
            }

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(13, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 12]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 12]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfPDutyCycleAndPeanHaveDifferentStateDependency()
        {
            this.elementDefinition.Parameter.Single(p => p.ParameterType.ShortName == "p_duty_cyc").StateDependence = null;

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(13, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 12]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 12]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfRedundancyIsNotCompound()
        {
            this.elementDefinition.Parameter.Single(p => p.ParameterType.ShortName == "redundancy")
                .ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.credentials.Uri) 
                {
                    Name = "redundancy",
                    ShortName = "redundancy"
                };

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(10, arrayAssembler.ContentArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 9]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 9]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfRedundancySchemeIsMissing()
        {
            var redundancy = this.elementDefinition.Parameter
                .Single(p => p.ParameterType.ShortName == "redundancy").ParameterType as CompoundParameterType;

            redundancy.Component.Remove(redundancy.Component.Single(c => c.ShortName == "scheme"));

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 11]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfRedundancyTypeIsMissing()
        {
            var redundancy = this.elementDefinition.Parameter
                .Single(p => p.ParameterType.ShortName == "redundancy").ParameterType as CompoundParameterType;

            redundancy.Component.Remove(redundancy.Component.Single(c => c.ShortName == "type"));

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 11]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfRedundancyKIsMissing()
        {
            var redundancy = this.elementDefinition.Parameter
                .Single(p => p.ParameterType.ShortName == "redundancy").ParameterType as CompoundParameterType;

            redundancy.Component.Remove(redundancy.Component.Single(c => c.ShortName == "k"));

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 11]);
        }

        [Test]
        public void VerifyPMeanIsNotCalculatedIfRedundancyNIsMissing()
        {
            var redundancy = this.elementDefinition.Parameter
                .Single(p => p.ParameterType.ShortName == "redundancy").ParameterType as CompoundParameterType;

            redundancy.Component.Remove(redundancy.Component.Single(c => c.ShortName == "n"));

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(12, arrayAssembler.NamesArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 11]);
        }

        [Test]
        public void VerifyThatAssemblerCalculatesPMeanIfRedundancyTypeIsInternal()
        {
            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(13, arrayAssembler.ContentArray.GetLength(1));
            Assert.AreEqual(@"Crossview_ED_1.p_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 12]);

            Assert.IsTrue(double.TryParse(arrayAssembler.ContentArray[6, 12].ToString(), out _));
        }

        [Test]
        public void VerifyThatAssemblerCalculatesPMeanIfRedundancyTypeIsExternal()
        {
            this.elementDefinition.Parameter.RemoveAll(p => p.ParameterType.ShortName == "redundancy");
            this.elementDefinition.Parameter.Add(this.pRedundancyActiveExternalParameter);

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(13, arrayAssembler.ContentArray.GetLength(1));

            // indices have to account for parameter [re]ordering
            Assert.AreEqual(@"Crossview_ED_1.p_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 7]);
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 8]);

            Assert.IsTrue(double.TryParse(arrayAssembler.ContentArray[6, 8].ToString(), out _));
        }

        [Test]
        public void VerifyThatPMeanIsZeroWhenPDutyCycleIsMinusOne()
        {
            this.elementDefinition.Parameter.RemoveAll(p => p.ParameterType.ShortName == "p_duty_cyc");
            this.elementDefinition.Parameter.Add(this.pDutyCycleParameterWithMinusOneValue);

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(13, arrayAssembler.ContentArray.GetLength(1));

            // indices have to account for parameter [re]ordering
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(@"Crossview_ED_1.p_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 12]);

            Assert.AreEqual(0, arrayAssembler.ContentArray[6, 11]);
        }

        [Test]
        public void VerifyThatPMeanIsNotComputedWhenDutyCycleHasInvalidValue()
        {
            this.elementDefinition.Parameter.RemoveAll(p => p.ParameterType.ShortName == "p_duty_cyc");
            this.elementDefinition.Parameter.Add(this.pDutyCycleParameterWithInvalidValue);

            var arrayAssembler = new CrossviewArrayAssembler(
                this.excelRows,
                this.elementDefinition.Parameter.Select(p => p.ParameterType.Iid));

            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("redundancy"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_on"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_stby"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_duty_cyc"));
            Assert.IsTrue(arrayAssembler.HeaderDictionary.ContainsKey("p_mean"));

            Assert.AreEqual(13, arrayAssembler.ContentArray.GetLength(1));

            // indices have to account for parameter [re]ordering
            Assert.AreEqual(@"Crossview_ED_1.p_mean\PS_1", arrayAssembler.NamesArray[6, 11]);
            Assert.AreEqual(@"Crossview_ED_1.p_duty_cyc\PS_1", arrayAssembler.NamesArray[6, 12]);

            Assert.AreEqual("-", arrayAssembler.ContentArray[6, 11]);
        }
    }
}
