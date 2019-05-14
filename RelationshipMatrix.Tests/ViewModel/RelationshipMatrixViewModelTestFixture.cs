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
    using CDP4RelationshipMatrix.Settings;
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

            Assert.IsFalse(vm.CanEditSourceY);
            Assert.IsFalse(vm.CanEditSourceX);
            Assert.IsFalse(vm.CanInspectSourceY);
            Assert.IsFalse(vm.CanInspectSourceX);

            Assert.AreEqual(this.participant, vm.ActiveParticipant);
            Assert.AreEqual($"{this.domain.Name} [{this.domain.ShortName}]", vm.DomainOfExpertise);

            Assert.AreEqual(this.settings.PossibleClassKinds.Count, vm.SourceYConfiguration.PossibleClassKinds.Count);
            Assert.AreEqual(0, vm.RelationshipConfiguration.PossibleRules.Count);

            vm.SourceYConfiguration.SelectedClassKind = vm.SourceYConfiguration.PossibleClassKinds.First(x => x == ClassKind.ElementDefinition);
            vm.SourceXConfiguration.SelectedClassKind = vm.SourceXConfiguration.PossibleClassKinds.First(x => x == ClassKind.ElementDefinition);

            Assert.AreEqual(this.srdl.DefinedCategory.Count(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition)), vm.SourceYConfiguration.PossibleCategories.Count);
            Assert.AreEqual(this.srdl.DefinedCategory.Count(x => x.PermissibleClass.Contains(ClassKind.ElementDefinition)), vm.SourceXConfiguration.PossibleCategories.Count);

            Assert.AreEqual(this.srdl.Rule.Count, vm.RelationshipConfiguration.PossibleRules.Count);
            Assert.IsEmpty(vm.Matrix.Records);
            Assert.IsEmpty(vm.Matrix.Columns);

            vm.SourceYConfiguration.SelectedCategories = new List<Category>(vm.SourceYConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd1.Iid || x.Iid == this.catEd2.Iid));
            vm.SourceXConfiguration.SelectedCategories = new List<Category>(vm.SourceXConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd2.Iid));
            vm.SourceYConfiguration.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.OR;
            vm.RelationshipConfiguration.SelectedRule = vm.RelationshipConfiguration.PossibleRules.Single();

            vm.SourceYConfiguration.IncludeSubctegories = false;

            Assert.AreEqual(3, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            vm.SourceYConfiguration.IncludeSubctegories = true;
            
            Assert.AreEqual(5, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            vm.SourceYConfiguration.SelectedCategories = new List<Category>(vm.SourceYConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd3.Iid));

            Assert.AreEqual(2, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            vm.SourceYConfiguration.SelectedCategories = new List<Category>(vm.SourceYConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd4.Iid));

            Assert.AreEqual(1, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            vm.SourceYConfiguration.SelectedCategories = new List<Category>(vm.SourceYConfiguration.PossibleCategories.Where(x => x.Iid == this.catEd1.Iid || x.Iid == this.catEd2.Iid));
            vm.SourceYConfiguration.SelectedBooleanOperatorKind = CategoryBooleanOperatorKind.AND;

            Assert.AreEqual(1, vm.Matrix.Records.Count);
            Assert.AreEqual(3, vm.Matrix.Columns.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);

            vm.SwitchAxisCommand.Execute(null);

            Assert.AreEqual(2, vm.Matrix.Records.Count);
            Assert.AreEqual(2, vm.Matrix.Columns.Count);

            vm.Dispose();
        }
    }
}
