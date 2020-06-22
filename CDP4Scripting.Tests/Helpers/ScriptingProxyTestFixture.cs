// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptingProxyTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Scripting.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Documents;
    
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    
    using CDP4Dal;
    
    using CDP4Scripting.Helpers;

    using Interfaces;
    
    using Moq;

    using NUnit.Framework;
    
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ScriptingProxy"/> class
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ScriptingProxyTestFixture : DispatcherTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;

        private Assembler assembler;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private Mock<IScriptPanelViewModel> scriptViewModel;
        private ScriptingProxy scriptingProxy;
        private Mock<ISession> session;

        private OutputTerminal outputTerminal;

        private EngineeringModel engineerModel;
        private EngineeringModelSetup engineeringModelSetup;

        private IterationSetup iterationSetup;
        private Iteration iteration;

        private ElementDefinition elementDefinition;

        private Parameter parameter;
        private SimpleQuantityKind mass;
        private ParameterValueSet parameterValueSet;

        private List<EngineeringModel> myModels;
        private IEnumerable<EngineeringModel> models;

        private List<Iteration> myIterations;
        private IEnumerable<Iteration> iterations;

        private List<ElementDefinition> myElementDefinitions;
        public IEnumerable<ElementDefinition> ElementDefinitions { get; private set; }

        private List<Parameter> myParameters;
        public IEnumerable<Parameter> Parameters { get; private set; }

        private List<ParameterValueSet> myParameterValueSets;
        public IEnumerable<ParameterValueSet> ParameterValueSets { get; private set; }

        [SetUp]
        public async Task SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();

            this.assembler = new Assembler(this.uri);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.outputTerminal = new OutputTerminal();

            this.scriptViewModel = new Mock<IScriptPanelViewModel>();
            this.scriptViewModel.SetupGet(x => x.OutputTerminal).Returns(() => this.outputTerminal);
            this.scriptViewModel.SetupProperty(x => x.SelectedSession, session.Object);
            
            this.scriptingProxy = new ScriptingProxy(this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object);
            this.scriptingProxy.ScriptingPanelViewModel = this.scriptViewModel.Object;

            // Creation of the elements that can be searched using a script
            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, null)
            {
                Name = "model",
                ShortName = "Train"
            };

            this.engineerModel = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, null)
            {
                EngineeringModelSetup = engineeringModelSetup
            };
            this.engineerModel.EngineeringModelSetup = this.engineeringModelSetup;

            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, null) {IterationSetup = this.iterationSetup};
            this.iteration.IterationSetup = this.iterationSetup;

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "element",
                ShortName = "Transformator"
            };

            this.mass = new SimpleQuantityKind(Guid.NewGuid(), null, this.uri)
            {
                Name = "parameter",
                ShortName = "mass"
            };

            this.parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, null)
            {
                ParameterType = this.mass
            };

            this.parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, null)
            {
                Reference = new ValueArray<string>(new List<string> {"100"}),
                Computed = new ValueArray<string>(new List<string> {"-"}),
                Formula = new ValueArray<string>(new List<string> {"-"}),
                ValueSwitch = ParameterSwitchKind.REFERENCE
            };

            this.engineerModel.Iteration.Add(this.iteration);
            this.iteration.Element.Add(this.elementDefinition);
            this.elementDefinition.Parameter.Add(this.parameter);
            this.parameter.ValueSet.Add(this.parameterValueSet);

            // Inclusion of the engineering model in the cache 
            var testThing = new Lazy<Thing>(() => this.engineerModel);
            testThing.Value.Cache.TryAdd(new CacheKey(testThing.Value.Iid, null), testThing);
        }

        [Test]
        public void VerifyThatClearWorks()
        {
            this.scriptViewModel.Object.OutputTerminal.AppendText("Content of the output");
            this.scriptingProxy.Clear();
            var outputContent = new TextRange(this.scriptViewModel.Object.OutputTerminal.Document.ContentStart, this.scriptViewModel.Object.OutputTerminal.Document.ContentEnd);
            Assert.AreEqual(true, outputContent.IsEmpty);

            this.scriptingProxy.ScriptingPanelViewModel = null;
            Assert.DoesNotThrow(() => this.scriptingProxy.Clear());
        }

        [Test]
        public void VerifyThatHelpWorks()
        {
            this.scriptingProxy.Help();

            var content = new TextRange(this.scriptViewModel.Object.OutputTerminal.Document.ContentStart, this.scriptViewModel.Object.OutputTerminal.Document.ContentEnd);
            Assert.That(content.Text.Length, Is.GreaterThan(50));
            Assert.IsTrue(content.Text.Contains("CDP4 Commands"));
            Assert.IsTrue(content.Text.Contains("List of the commands available"));
            Assert.IsTrue(content.Text.Contains("OpenDialog(string dialogName)"));
            Assert.IsTrue(content.Text.Contains("Opens a dialog using its name."));
            Assert.IsFalse(content.Text.Contains("OpenPanelModelBrowser"));
            Assert.IsFalse(content.Text.Contains("SessionChangeEventHandler"));

            this.scriptingProxy.ScriptingPanelViewModel = null;
            Assert.DoesNotThrow(() => this.scriptingProxy.Help());
        }

        [Test]
        public void VerifyThatOpenDialogWorks()
        {
            var aboutDialogName = "About";
            Assert.DoesNotThrow(() => this.scriptingProxy.OpenDialog(aboutDialogName));
            this.dialogNavigationService.Verify(x => x.NavigateModal(aboutDialogName), Times.Once);

            var logDialogName = "LogDetails";
            Assert.DoesNotThrow(() => this.scriptingProxy.OpenDialog(logDialogName));
            this.dialogNavigationService.Verify(x => x.NavigateModal(logDialogName), Times.Once);
        }

        [Test]
        public void VerifyThatEngineeringModelShortNameReturns()
        {
            this.myModels = new List<EngineeringModel>() { this.engineerModel };
            this.models = this.myModels;
            var getEngineeringModel = this.scriptingProxy.FindEngineeringModel(this.models, "Train");
            Assert.AreEqual(this.engineerModel, getEngineeringModel);
        }

        [Test]
        public void VerifyThatIterationNumberReturns()
        {
            this.myIterations = new List<Iteration>() {this.iteration};
            this.iterations = this.myIterations;

            var getIteration = this.scriptingProxy.FindIterationNumber(this.iterations, "0");
            Assert.AreEqual(this.iteration, getIteration);
        }

        [Test]
        public void VerifyThatElementShortNameReturns()
        {
            this.myElementDefinitions = new List<ElementDefinition>() {this.elementDefinition};
            this.ElementDefinitions = this.myElementDefinitions;

            var getElement = this.scriptingProxy.FindElement(this.myElementDefinitions, "Transformator");
            Assert.AreEqual(this.elementDefinition, getElement);
        }

        [Test]
        public void VerifyThatParameterShortNameReturns()
        {
            this.myParameters = new List<Parameter>() {this.parameter};
            this.Parameters = this.myParameters;

            var getParameter = scriptingProxy.FindParameter(this.myParameters, "Transformator.mass");
            Assert.AreEqual(this.parameter, getParameter);
        }

        [Test]
        public void VerifyThatElementValueReturns()
        {
            this.myParameterValueSets = new List<ParameterValueSet>() {this.parameterValueSet};
            this.ParameterValueSets = this.myParameterValueSets;

            var getParameterValue = this.scriptingProxy.FindValue(this.myParameterValueSets, "reference");
            Assert.AreEqual("100", getParameterValue);
        }

        [Test]
        public void VerifyThatGetMethodsReturn()
        {
            var getEngineeringModel = this.scriptingProxy.GetEngineeringModel("Train");
            var getEngineeringModelIteration = this.scriptingProxy.GetEngineeringModelIteration("Train", 0);
            var getElementDefinition = this.scriptingProxy.GetElementDefinition("Train", 0, "Transformator");
            var getParameter = this.scriptingProxy.GetParameter("Train", 0, "Transformator", "mass");
            var getValue = this.scriptingProxy.GetValue("Train", 0, "Transformator", "mass", "reference");
            Assert.AreEqual(this.engineerModel, getEngineeringModel);
            Assert.AreEqual(this.iteration, getEngineeringModelIteration);
            Assert.AreEqual(this.elementDefinition, getElementDefinition);
            Assert.AreEqual(this.parameter, getParameter);
            Assert.AreEqual("100", getValue);
        }

        [Test]
        public void VerifyThatInitCommandCompletionData()
        {
            this.scriptingProxy.InitCommandCompletionData();
            var commandCompletionData = this.scriptingProxy.CommandCompletionData.Single(x => ((string) x.Content) == "GetSiteDirectory()");
            Assert.AreEqual("GetSiteDirectory()", commandCompletionData.Content);
            Assert.AreEqual("Gets the site directory associated to the selected session.", commandCompletionData.Description);

            this.scriptingProxy.InitCommandCompletionData();
            commandCompletionData = this.scriptingProxy.CommandCompletionData.Single(x => ((string)x.Content) == "NestedElementTreeGenerator");
            Assert.AreEqual("NestedElementTreeGenerator", commandCompletionData.Content);
            Assert.AreEqual("Allows the usage of the methods of this class.", commandCompletionData.Description);
        }
    }
}