﻿// -------------------------------------------------------------------------------------------------
// <copyright file="ManageConfigurationsDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Composition.ViewModels;

    using CDP4RelationshipMatrix.Settings;
    using NUnit.Framework;
    using ViewModels;

    public class ManageConfigurationsDialogViewModelTestFixture : ViewModelTestBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }
        
        [Test]
        public async Task AssertViewModelWorks()
        {
            var savedConfig1 = new SavedConfiguration
            {
                Name = "Saved1",
                Description = "DescSaved1"
            };

            var savedConfig2 = new SavedConfiguration
            {
                Name = "Saved2",
                Description = "DescSaved2"
            };

            this.settings.SavedConfigurations.Add(savedConfig1);
            this.settings.SavedConfigurations.Add(savedConfig2);

            var vm = new ManageConfigurationsDialogViewModel<RelationshipMatrixPluginSettings>(this.pluginService.Object);

            Assert.AreEqual(this.settings.SavedConfigurations.Count, vm.SavedConfigurations.Count);

            vm.SelectedConfiguration = savedConfig1;

            await vm.DeleteSelectedCommand.Execute();

            Assert.AreEqual(1, vm.SavedConfigurations.Count);
            Assert.DoesNotThrow(() => vm.OkCommand.Execute());
        }
    }
}
