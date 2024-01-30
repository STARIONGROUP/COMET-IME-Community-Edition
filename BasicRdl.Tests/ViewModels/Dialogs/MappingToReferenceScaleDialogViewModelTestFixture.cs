// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MappingToReferenceScaleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class MappingToReferenceScaleDialogViewModelTestFixture
    {
        private MappingToReferenceScaleDialogViewModel viewmodel;
        private MappingToReferenceScale mappingToReferenceScale;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private RatioScale testRatioScale;
        private RatioScale clone;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.navigation = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var person = new Person(Guid.NewGuid(), this.cache, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { Name = "testRDL", ShortName = "test" };
            this.mappingToReferenceScale = new MappingToReferenceScale(Guid.NewGuid(), null, null);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);
            this.testRatioScale = new RatioScale(Guid.NewGuid(), this.cache, null) { Name = "ratioScale", ShortName = "dqk" };
            var testRatioScale1 = new RatioScale(Guid.NewGuid(), this.cache, null) { Name = "ratioScale1", ShortName = "dqk1" };
            var svd1 = new ScaleValueDefinition(Guid.NewGuid(), this.cache, null) { Name = "ReferenceSVD", ShortName = "RSVD" };
            var svd2 = new ScaleValueDefinition(Guid.NewGuid(), this.cache, null) { Name = "DependentSVD", ShortName = "DSVD" };
            this.testRatioScale.ValueDefinition.Add(svd1);
            testRatioScale1.ValueDefinition.Add(svd2);
            rdl.Scale.Add(this.testRatioScale);
            rdl.Scale.Add(testRatioScale1);

            this.cache.TryAdd(new CacheKey(this.testRatioScale.Iid, null), new Lazy<Thing>(() => this.testRatioScale));
            this.clone = this.testRatioScale.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new MappingToReferenceScaleDialogViewModel(this.mappingToReferenceScale, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.clone);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(1, this.viewmodel.PossibleReferenceScaleValue.Count);
            Assert.AreEqual(1, this.viewmodel.PossibleDependentScaleValue.Count);
            Assert.IsNotNull(this.viewmodel.SelectedReferenceScaleValue);
            Assert.IsNotNull(this.viewmodel.SelectedDependentScaleValue);
        }

        [Test]
        public void VerifyOkCanExecute()
        {
            Assert.IsNotNull(this.viewmodel.SelectedReferenceScaleValue);
            Assert.IsNotNull(this.viewmodel.SelectedDependentScaleValue);
            Assert.IsTrue(this.viewmodel.OkCanExecute);

            this.viewmodel.SelectedReferenceScaleValue = null;
            Assert.IsFalse(this.viewmodel.OkCanExecute);
            this.viewmodel.SelectedReferenceScaleValue = this.viewmodel.PossibleReferenceScaleValue.First();

            this.viewmodel.SelectedReferenceScaleValue = null;
            Assert.IsFalse(this.viewmodel.OkCanExecute);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new MappingToReferenceScaleDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }

        [Test]
        public async Task VerifyThatInspectCommandsOpenDialogs()
        {
            Assert.IsTrue(((ICommand)this.viewmodel.InspectSelectedDependentScaleValueCommand).CanExecute(null));
            await this.viewmodel.InspectSelectedDependentScaleValueCommand.Execute();
            this.navigation.Verify(x => x.Navigate(It.IsAny<ScaleValueDefinition>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));

            Assert.IsTrue(((ICommand)this.viewmodel.InspectSelectedReferenceScaleValueCommand).CanExecute(null));
            await this.viewmodel.InspectSelectedReferenceScaleValueCommand.Execute();
            this.navigation.Verify(x => x.Navigate(It.IsAny<ScaleValueDefinition>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }
    }
}
