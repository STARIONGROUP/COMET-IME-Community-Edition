// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class OptionDialogViewModelTestFixture
    {
        private ThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Option option;
        private Iteration iteration;
        private EngineeringModel model;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private ModelReferenceDataLibrary mrdl;
        private SiteReferenceDataLibrary srdl;
        private Category cat1;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Iteration iterationClone;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.option = new Option(Guid.NewGuid(), this.cache, null);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);
            this.iteration.Option.Add(this.option);
            this.iteration.DefaultOption = this.option;
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, null);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, null);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, null) { RequiredRdl = this.srdl };
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, null);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.cat1 = new Category(Guid.NewGuid(), this.cache, null);
            this.cat1.PermissibleClass.Add(ClassKind.Option);

            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.srdl.DefinedCategory.Add(this.cat1);

            this.model.EngineeringModelSetup = this.modelsetup;
            this.model.Iteration.Add(this.iteration);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.iterationClone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.iterationClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var vm = new OptionDialogViewModel(this.option, this.thingTransaction, this.session.Object, true,
                ThingDialogKind.Create, this.thingDialogNavigationService.Object,
                this.iterationClone);

            Assert.AreEqual(1, vm.PossibleCategory.Count);
            Assert.AreEqual(true, vm.IsDefault);
        }

        [Test]
        public async Task VerifyUpdateTransaction()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, null) { Name = "ed" };
            var keptUsage = new ElementUsage(Guid.NewGuid(), this.cache, null) { Name = "kept" };
            var droppedUsage = new ElementUsage(Guid.NewGuid(), this.cache, null) { Name = "dropped" };
            var lockedUsage = new ElementUsage(Guid.NewGuid(), this.cache, null) { Name = "locked" };
            var restoredUsage = new ElementUsage(Guid.NewGuid(), this.cache, null) { Name = "restored" };
            restoredUsage.ExcludeOption.Add(this.option);
            elementDefinition.ContainedElement.AddRange(new[] { keptUsage, droppedUsage, lockedUsage, restoredUsage });
            this.iteration.Element.Add(elementDefinition);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(lockedUsage)).Returns(false);

            OperationContainer writtenContainer = null;
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Callback<OperationContainer>(x => writtenContainer = x).Returns(Task.CompletedTask);

            var iterationClone = this.iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.iteration), iterationClone);
            var newOption = new Option(Guid.NewGuid(), this.cache, null) { Name = "new", ShortName = "new" };

            var vm = new OptionDialogViewModel(newOption, transaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, iterationClone);

            Assert.That(vm.ElementUsageRows.Select(x => x.Name), Is.EqualTo(new[] { "kept", "dropped", "locked", "restored" }));
            Assert.That(vm.ElementUsageRows.Select(x => x.IsIncluded), Is.All.True);
            Assert.That(vm.ElementUsageRows.Select(x => x.IsEditable), Is.EqualTo(new[] { true, true, false, true }));

            await vm.UncheckAllElementUsagesCommand.Execute();
            Assert.That(vm.ElementUsageRows.Select(x => x.IsIncluded), Is.EqualTo(new[] { false, false, true, false }));

            await vm.CheckAllElementUsagesCommand.Execute();
            Assert.That(vm.ElementUsageRows.Select(x => x.IsIncluded), Is.All.True);

            vm.ElementUsageRows.Single(x => x.Thing == droppedUsage).IsIncluded = false;
            await vm.OkCommand.Execute();

            var updatedUsages = writtenContainer.Operations
                .Select(x => x.ModifiedThing)
                .OfType<CDP4Common.DTO.ElementUsage>()
                .ToList();

            Assert.That(updatedUsages.Select(x => x.Iid), Is.EqualTo(new[] { droppedUsage.Iid }));
            Assert.That(updatedUsages[0].ExcludeOption, Is.EqualTo(new[] { newOption.Iid }));

            iterationClone = this.iteration.Clone(false);
            transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.iteration), iterationClone);

            vm = new OptionDialogViewModel(this.option.Clone(false), transaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, iterationClone);

            Assert.That(vm.ElementUsageRows.Select(x => x.IsIncluded), Is.EqualTo(new[] { true, true, true, false }));

            vm.ElementUsageRows.Single(x => x.Thing == restoredUsage).IsIncluded = true;
            await vm.OkCommand.Execute();

            updatedUsages = writtenContainer.Operations
                .Select(x => x.ModifiedThing)
                .OfType<CDP4Common.DTO.ElementUsage>()
                .ToList();

            Assert.That(updatedUsages.Select(x => x.Iid), Is.EqualTo(new[] { restoredUsage.Iid }));
            Assert.That(updatedUsages[0].ExcludeOption, Is.Empty);

            vm = new OptionDialogViewModel(this.option, null, this.session.Object, true, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, this.iteration);

            Assert.That(vm.ElementUsageRows.Select(x => x.IsEditable), Is.All.False);
            Assert.That(((ICommand)vm.UncheckAllElementUsagesCommand).CanExecute(null), Is.False);
        }

        [Test]
        public async Task VerifySelectedBaselineOption()
        {
            var baseline = new Option(Guid.NewGuid(), this.cache, null) { Name = "baseline" };
            this.iteration.Option.Add(baseline);

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, null) { Name = "ed" };
            var includedUsage = new ElementUsage(Guid.NewGuid(), this.cache, null) { Name = "included" };
            includedUsage.ExcludeOption.Add(this.option);
            var excludedUsage = new ElementUsage(Guid.NewGuid(), this.cache, null) { Name = "excluded" };
            excludedUsage.ExcludeOption.Add(baseline);
            var lockedUsage = new ElementUsage(Guid.NewGuid(), this.cache, null) { Name = "locked" };
            lockedUsage.ExcludeOption.Add(baseline);
            elementDefinition.ContainedElement.AddRange(new[] { includedUsage, excludedUsage, lockedUsage });
            this.iteration.Element.Add(elementDefinition);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(lockedUsage)).Returns(false);

            var clone = this.iteration.Clone(false);
            var transaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.iteration), clone);
            var vm = new OptionDialogViewModel(this.option.Clone(false), transaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, clone);

            Assert.That(vm.PossibleBaselineOption, Is.EqualTo(new[] { baseline }));
            Assert.That(vm.ElementUsageRows.Select(x => x.IsIncluded), Is.EqualTo(new[] { false, true, true }));

            vm.SelectedBaselineOption = baseline;
            Assert.That(vm.ElementUsageRows.Select(x => x.IsIncluded), Is.EqualTo(new[] { true, false, true }));

            vm.ElementUsageRows[1].IsIncluded = true;
            vm.SelectedBaselineOption = null;
            Assert.That(vm.ElementUsageRows.Select(x => x.IsIncluded), Is.EqualTo(new[] { true, true, true }));

            vm.SelectedBaselineOption = baseline;
            await vm.ResetElementUsagesCommand.Execute();
            Assert.That(vm.ElementUsageRows.Select(x => x.IsIncluded), Is.EqualTo(new[] { false, true, true }));
            Assert.That(vm.SelectedBaselineOption, Is.Null);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new OptionDialogViewModel());
        }
    }
}
