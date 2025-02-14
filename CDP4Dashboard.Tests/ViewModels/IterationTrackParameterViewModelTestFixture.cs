﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationTrackParameterViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
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

namespace CDP4Dashboard.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Utilities;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4Dashboard.ViewModels.Widget;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Tests if <see cref="IterationTrackParameterViewModel{TThing,TValueSet}"/> works set correctly
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal class IterationTrackParameterViewModelTestFixture
    {
        private Mock<ISession> session;
        private Iteration iteration;
        private readonly PropertyInfo revision = typeof(Thing).GetProperty("RevisionNumber");
        private readonly Uri uri = new Uri("https://www.stariongroup.eu");
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private DomainOfExpertise domain;
        private IterationTrackParameter iterationTrackParameterForParameter;
        private IterationTrackParameter iterationTrackParameterForParameterOverride;
        private ParameterValueSet parameterValueSet;
        private ParameterOverrideValueSet parameterOverrideValueSet;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private IterationTrackParameterViewModel<Parameter, ParameterValueSet> iterationTrackParameterViewModel;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.assembler = new Assembler(this.uri, this.messageBus);

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.cache = this.assembler.Cache;
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "test" };
            this.iteration = new Iteration(Guid.NewGuid(), null, null);

            var iterationTrackParameterTestFixture = new IterationTrackParameterTestFixture();
            iterationTrackParameterTestFixture.Setup();
            this.iterationTrackParameterForParameter = iterationTrackParameterTestFixture.iterationTrackParameterForParameter;
            this.iterationTrackParameterForParameterOverride = iterationTrackParameterTestFixture.iterationTrackParameterForParameterOverride;

            var parameterValueArray = new ValueArray<string>(new[] { "10" });

            this.parameterValueSet = new ParameterValueSet(Guid.NewGuid(), null, null)
            {
                Published = parameterValueArray,
                Manual = parameterValueArray,
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            ((Parameter)this.iterationTrackParameterForParameter.ParameterOrOverride).ValueSet.Add(this.parameterValueSet);

            var parameterOverrideValueArray = new ValueArray<string>(new[] { "20" });

            this.parameterOverrideValueSet = new ParameterOverrideValueSet(Guid.NewGuid(), null, null)
            {
                Published = parameterOverrideValueArray,
                Manual = parameterOverrideValueArray,
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            ((ParameterOverride)this.iterationTrackParameterForParameterOverride.ParameterOrOverride).ValueSet.Add(this.parameterOverrideValueSet);

            this.cache.AddOrUpdate(new CacheKey(this.parameterValueSet.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameterValueSet), (key, value) => value);
            this.cache.AddOrUpdate(new CacheKey(this.parameterOverrideValueSet.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameterOverrideValueSet), (key, value) => value);

            this.cache.AddOrUpdate(new CacheKey(this.iterationTrackParameterForParameter.ParameterOrOverride.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.iterationTrackParameterForParameter.ParameterOrOverride), (key, value) => value);
            this.cache.AddOrUpdate(new CacheKey(this.iterationTrackParameterForParameterOverride.ParameterOrOverride.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.iterationTrackParameterForParameterOverride.ParameterOrOverride), (key, value) => value);

            this.iterationTrackParameterViewModel = new IterationTrackParameterViewModel<Parameter, ParameterValueSet>(this.session.Object, this.iteration, this.iterationTrackParameterForParameter);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void verify_that_OnRefreshCommand_works()
        {
            //OnRefresh is executed in the constructor
            Assert.IsNotNull(this.iterationTrackParameterViewModel.OnRefreshCommand);

            Assert.AreEqual(1, this.iterationTrackParameterViewModel.LineSeriesCollection.Count);
            Assert.AreEqual(1, this.iterationTrackParameterViewModel.LineSeriesCollection.First().Lines.Count());

            Assert.AreEqual(this.iterationTrackParameterViewModel.StatusIndicatorColor.ToString(), CDP4Color.Inconclusive.GetBrush().ToString());
            Assert.AreEqual("10 [SC]", this.iterationTrackParameterViewModel.CurrentValue);
            Assert.AreEqual("- [SC]", this.iterationTrackParameterViewModel.PreviousValue);

            Assert.AreEqual(IterationTrackParameterViewModel<Parameter, ParameterValueSet>.StatusIndicatorUnknown, this.iterationTrackParameterViewModel.StatusIndicator);
            Assert.IsTrue(this.iterationTrackParameterViewModel.PercentageChange.Contains(IterationTrackParameterViewModel<Parameter, ParameterValueSet>.StatusIndicatorUnknown));

            Assert.AreEqual(this.iterationTrackParameterViewModel.ModelCode, ((Parameter)this.iterationTrackParameterForParameter.ParameterOrOverride).ModelCode());
        }

        [Test]
        public void verify_that_OnCopyDataCommand_works()
        {
            var text = "no_text_please";
            Clipboard.SetData(DataFormats.UnicodeText, text);
            ((ICommand)this.iterationTrackParameterViewModel.OnCopyDataCommand).Execute(default);

            Assert.AreNotEqual(text, Clipboard.GetData(DataFormats.UnicodeText));
        }

        [Test]
        public void verify_that_subscriptions_and_revision_selection_work()
        {
            var oldParameterValueSet = this.parameterValueSet.Clone(false);
            this.parameterValueSet.Manual = new ValueArray<string>(new[] { "15" });
            this.parameterValueSet.RevisionNumber = 2;
            this.parameterValueSet.Revisions.Add(oldParameterValueSet.RevisionNumber, oldParameterValueSet);

            this.messageBus.SendObjectChangeEvent(this.parameterValueSet, EventKind.Updated);

            Assert.AreEqual(1, this.iterationTrackParameterViewModel.LineSeriesCollection.Count);
            Assert.AreEqual(1, this.iterationTrackParameterViewModel.LineSeriesCollection.First().Lines.Count());

            var oldParameterValueSet2 = this.parameterValueSet.Clone(false);
            this.parameterValueSet.Published = new ValueArray<string>(new[] { "15" });
            this.parameterValueSet.RevisionNumber = 3;
            this.parameterValueSet.Revisions.Add(oldParameterValueSet2.RevisionNumber, oldParameterValueSet2);

            this.messageBus.SendObjectChangeEvent(this.parameterValueSet, EventKind.Updated);

            Assert.AreEqual(1, this.iterationTrackParameterViewModel.LineSeriesCollection.Count);

            //only after publication a value is selected to be used in a Widget
            Assert.AreEqual(2, this.iterationTrackParameterViewModel.LineSeriesCollection.First().Lines.Count());
        }
    }
}
