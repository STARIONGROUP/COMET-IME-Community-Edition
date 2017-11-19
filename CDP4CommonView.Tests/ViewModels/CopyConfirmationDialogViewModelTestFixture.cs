// -------------------------------------------------------------------------------------------------
// <copyright file="CopyConfirmationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4CommonView.ViewModels;
    using NUnit.Framework;
    using Moq;

    /// <summary>
    /// Suite of tests for the <see cref="CopyConfirmationDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class CopyConfirmationDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private Assembler assembler;
        private ElementDefinition elementDefinition;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri);

        }

        [Test]
        public void VerifyThatDialogCanBeConstructed()
        {
            var copyableThings = new List<Thing> {this.elementDefinition};

            var dialog = new CopyConfirmationDialogViewModel(copyableThings, new Dictionary<Thing, string>());

            Assert.AreEqual("Copy Confirmation", dialog.Title);

            Assert.IsTrue(dialog.ProceedCommand.CanExecute(null));
            Assert.IsTrue(dialog.CancelCommand.CanExecute(null));
            
            Assert.IsNotEmpty(dialog.CopyPermissionDetails);            
            Assert.AreEqual("A partial copy will be performed.", dialog.CopyPermissionMessage);
        }

        [Test]
        public void VerifyThatIfNoThingsToCopyTheProceedCommandIsDisabled()
        {
            var copyableThings = new List<Thing>();

            var dialog = new CopyConfirmationDialogViewModel(copyableThings, new Dictionary<Thing, string>());
            
            Assert.IsFalse(dialog.ProceedCommand.CanExecute(null));
            Assert.IsNull(dialog.CopyPermissionDetails);
            Assert.AreEqual("The copy operation cannot be performed.", dialog.CopyPermissionMessage);
        }

        [Test]
        public void VerifyThatTheDialogResultIsPositiveWhenTheProceedCommandIsExecuted()
        {
            var copyableThings = new List<Thing> { this.elementDefinition };

            var dialog = new CopyConfirmationDialogViewModel(copyableThings, new Dictionary<Thing, string>());

            dialog.ProceedCommand.Execute(null);

            Assert.IsTrue((bool)dialog.DialogResult.Result);
        }

        [Test]
        public void VerifyThatTheDialogResultIsNegativeWhenTheCancelCommandIsExecuted()
        {
            var copyableThings = new List<Thing> { this.elementDefinition };

            var dialog = new CopyConfirmationDialogViewModel(copyableThings, new Dictionary<Thing, string>());

            dialog.CancelCommand.Execute(null);

            Assert.IsFalse((bool)dialog.DialogResult.Result);
        }
    }
}
