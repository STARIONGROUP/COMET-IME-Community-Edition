// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrixViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Events;
    using NUnit.Framework;
    using ViewModels;

    public class RelationshipMatrixViewModelTestFixture : ViewModelTestBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }
        
        [Test]
        public void AssertViewModelWorks()
        {
            var vm = new RelationshipMatrixViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginService.Object);

            Assert.IsFalse(vm.CanEditSource1);
            Assert.IsFalse(vm.CanEditSource2);
            Assert.IsFalse(vm.CanInspectSource1);
            Assert.IsFalse(vm.CanInspectSource2);

            Assert.AreEqual(this.participant, vm.ActiveParticipant);
            Assert.AreEqual($"{this.domain.Name} [{this.domain.ShortName}]", vm.DomainOfExpertise);

            Assert.AreEqual(this.settings.PossibleClassKinds.Count, vm.Source1Configuration.PossibleClassKinds.Count);
            Assert.AreEqual(0, vm.RelationshipConfiguration.PossibleRules.Count);

            vm.Source1Configuration.SelectedClassKind = vm.Source1Configuration.PossibleClassKinds.First(x => x == ClassKind.ElementDefinition);
            vm.Source2Configuration.SelectedClassKind = vm.Source2Configuration.PossibleClassKinds.First(x => x == ClassKind.ElementDefinition);

            Assert.AreEqual(this.srdl.DefinedCategory.Count(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition)), vm.Source1Configuration.PossibleCategories.Count);
            Assert.AreEqual(this.srdl.DefinedCategory.Count(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition)), vm.Source2Configuration.PossibleCategories.Count);

            Assert.AreEqual(this.srdl.Rule.Count, vm.RelationshipConfiguration.PossibleRules.Count);
            Assert.IsEmpty(vm.Matrix.Records);
            Assert.IsEmpty(vm.Matrix.Columns);

            vm.Source1Configuration.SelectedCategories = new List<Category>(vm.Source1Configuration.PossibleCategories.Where(x => x.Iid == this.catEd1.Iid));
            vm.Source2Configuration.SelectedCategories = new List<Category>(vm.Source2Configuration.PossibleCategories.Where(x => x.Iid == this.catEd2.Iid));

            vm.RelationshipConfiguration.SelectedRule = vm.RelationshipConfiguration.PossibleRules.Single();

            Assert.AreEqual(2, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            vm.Dispose();
        }
    }
}
