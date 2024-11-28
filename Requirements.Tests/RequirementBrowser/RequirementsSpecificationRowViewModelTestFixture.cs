// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.RequirementBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;

    using CDP4Dal.Events;
    using CDP4Dal.Operations;

    using CDP4Requirements.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class RequirementsSpecificationRowViewModelTestFixture : OrderHandlerServiceTestFixtureBase
    {
        private readonly PropertyInfo revision = typeof(Thing).GetProperty("RevisionNumber");

        private RequirementsBrowserViewModel requirementBrowserViewModel;
        private List<Category> categories;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IMessageBoxService> messageBoxService;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            this.categories = new List<Category>() { new Category { Name = "category1" }, new Category { Name = "category2" } };
            this.spec1.Category = this.categories;

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.messageBoxService = new Mock<IMessageBoxService>();
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.requirementBrowserViewModel = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null, this.pluginService.Object);
            this.DelayedCheck(() => this.requirementBrowserViewModel.SingleRunBackgroundWorker == null).Wait();
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);

            Assert.AreEqual("spec1", row.Name);
            Assert.AreEqual("spec1", row.ShortName);
            Assert.AreEqual("category1, category2", row.Categories);
            Assert.AreSame(this.domain, row.Owner);
            Assert.That(row.Definition, Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatGroupsCanBeAddedOrRemoved()
        {
            var row = new RequirementsSpecificationRowViewModel(this.spec2, this.session.Object, this.requirementBrowserViewModel);
            row.IsParametricConstraintDisplayed = true;
            row.IsSimpleParameterValuesDisplayed = true;

            var groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(3, groups.Count());

            var grp1Row = row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.AreEqual(5, grp1Row.ContainedRows.Count);

            var newgrp = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.grp1.Group.Add(newgrp);

            this.revision.SetValue(this.grp1, 2);
            this.revision.SetValue(this.spec2, 2);
            this.messageBus.SendObjectChangeEvent(this.grp1, EventKind.Updated);
            this.messageBus.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            Assert.AreEqual(6, grp1Row.ContainedRows.Count);

            this.spec2.Group.Remove(this.grp2);
            this.revision.SetValue(this.spec2, 3);
            this.messageBus.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(2, groups.Count());
        }

        [Test]
        public void VerifyThatRequirementCanBeAddedOrRemoved()
        {
            var spec2Row = new RequirementsSpecificationRowViewModel(this.spec2, this.session.Object, this.requirementBrowserViewModel);

            var reqRows = spec2Row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(0, reqRows.Count);

            this.req21.Group = null;
            this.revision.SetValue(this.req21, 2);

            this.messageBus.SendObjectChangeEvent(this.req21, EventKind.Updated);
            reqRows = spec2Row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(1, reqRows.Count);

            var grp1Row = spec2Row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.IsFalse(grp1Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));
            this.req21.Group = this.grp4;
            this.revision.SetValue(this.req21, 3);

            this.messageBus.SendObjectChangeEvent(this.req21, EventKind.Updated);
            Assert.IsFalse(grp1Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));

            var grp11Row = grp1Row.ContainedRows.Single(x => x.Thing.Iid == this.grp4.Iid);
            Assert.IsTrue(grp11Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));
            this.req21.Group = null;
            this.revision.SetValue(this.req21, 4);

            this.messageBus.SendObjectChangeEvent(this.req21, EventKind.Updated);
            Assert.IsFalse(grp11Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));
            Assert.IsTrue(spec2Row.ContainedRows.Any(x => x.Thing.Iid == this.req21.Iid));

            var newreq = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.spec2.Requirement.Add(newreq);
            this.revision.SetValue(this.spec2, 5);

            this.messageBus.SendObjectChangeEvent(this.spec2, EventKind.Updated);
            reqRows = spec2Row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(2, reqRows.Count);

            this.spec2.Requirement.Remove(newreq);
            this.revision.SetValue(this.spec2, 6);

            this.messageBus.SendObjectChangeEvent(this.spec2, EventKind.Updated);
            reqRows = spec2Row.ContainedRows.Where(x => x.Thing is Requirement).ToList();
            Assert.AreEqual(1, reqRows.Count);
        }

        [Test]
        public void VerifyThatAddingRequirementGroupUpdatesRequirementsSpecificationContainedRows()
        {
            var row = new RequirementsSpecificationRowViewModel(this.spec2, this.session.Object, this.requirementBrowserViewModel);
            row.IsParametricConstraintDisplayed = true;
            row.IsSimpleParameterValuesDisplayed = true;

            var groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(3, groups.Count());

            var grp1Row = row.ContainedRows.Single(x => x.Thing.Iid == this.grp1.Iid);
            Assert.AreEqual(5, grp1Row.ContainedRows.Count);

            var newgrp = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.grp1.Group.Add(newgrp);

            this.revision.SetValue(this.grp1, 2);
            this.revision.SetValue(this.spec2, 2);
            this.messageBus.SendObjectChangeEvent(this.grp1, EventKind.Updated);
            this.messageBus.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            Assert.AreEqual(6, grp1Row.ContainedRows.Count);

            this.spec2.Group.Remove(this.grp2);
            this.revision.SetValue(this.spec2, 3);
            this.messageBus.SendObjectChangeEvent(this.spec2, EventKind.Updated);

            groups = row.ContainedRows.Where(x => x.Thing is RequirementsGroup);
            Assert.AreEqual(2, groups.Count());
        }

        [Test]
        public void VerifyDragOver()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            dropinfo.Setup(x => x.Payload).Returns(this.req1);

            row.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.Move, dropinfo.Object.Effects);

            dropinfo.Setup(x => x.Payload).Returns(this.grp1);
            row.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.Move, dropinfo.Object.Effects);

            dropinfo.Setup(x => x.Payload).Returns(this.iteration);
            row.DragOver(dropinfo.Object);
            dropinfo.VerifySet(x => x.Effects = DragDropEffects.None);
        }

        [Test]
        public void VerifyRequirementDrop()
        {
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.req1);
            dropInfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            this.spec1.Requirement.Clear();
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);
            Assert.AreEqual(0, this.spec1.Requirement.Count);

            row.Drop(dropInfo.Object);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }

        [Test]
        public void VerifyRequirementGroupDropFails()
        {
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.grp1);
            dropInfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            dropInfo.Setup(x => x.KeyStates).Returns(DragDropKeyStates.LeftMouseButton);

            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);
            row.IsParametricConstraintDisplayed = true;
            row.IsSimpleParameterValuesDisplayed = true;

            Assert.AreEqual(0, this.spec1.Group.Count);
            Assert.AreEqual(5, row.ContainedRows.Count);

            row.Drop(dropInfo.Object);

            // Drop group on another RequirementsSpecification is NOT allowed anumore since 10.0.3-rc6
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(0));
        }

        [Test]
        public void VerifyRequirementGroupDrop()
        {
            //Drop grp4 on another RequirementsSpecification
            var dropInfo = new Mock<IDropInfo>();
            dropInfo.Setup(x => x.Payload).Returns(this.grp4);
            dropInfo.Setup(x => x.Effects).Returns(DragDropEffects.Move);
            dropInfo.Setup(x => x.KeyStates).Returns(DragDropKeyStates.LeftMouseButton);

            this.spec1.Group.Add(this.grp1);
            this.spec1.Group.Add(this.grp2);
            this.spec1.Group.Add(this.grp3);
            this.spec1.Group.Add(this.grp4);
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);

            row.Drop(dropInfo.Object);

            // Drop group on another RequirementsSpecification is NOT allowed anumore since 10.0.3-rc6
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Exactly(1));
        }


        [Test]
        public void VerifyThatWhenSpecIsDeprecatedTheRowIsDeprecatedAsWell()
        {
            this.spec1.IsDeprecated = true;
            var row = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, this.requirementBrowserViewModel);
            Assert.IsTrue(row.IsDeprecated);
        }

        [Test]
        public void VerifyThatDragOverParameterWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, null);
            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);
            dropinfo.SetupProperty(x => x.Effects);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.RequirementsContainerParameterValue, It.IsAny<RequirementsSpecification>())).Returns(false);

            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.RequirementsContainerParameterValue, It.IsAny<RequirementsSpecification>())).Returns(true);
            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.Copy, dropinfo.Object.Effects);

            this.spec1.ParameterValue.Add(new RequirementsContainerParameterValue { ParameterType = param });
            containerRow.DragOver(dropinfo.Object);
            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDropParameterTypeWorks()
        {
            var containerRow = new RequirementsSpecificationRowViewModel(this.spec1, this.session.Object, null);

            var param = new BooleanParameterType();
            var tuple = new Tuple<ParameterType, MeasurementScale>(param, null);
            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(tuple);

            containerRow.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        /// <summary>
        /// Checks for <see cref="BackgroundWorker"/>s to finish
        /// </summary>
        /// <param name="check">Action to check</param>
        /// <param name="maxNumberOfChecks">Number of checks (100ms per check)</param>
        /// <returns>an awaitable <see cref="Task"/></returns>
        private async Task DelayedCheck(Func<bool> check, int maxNumberOfChecks = 10)
        {
            // wait 1000ms for background worker to be finished
            for (var i = 0; i < maxNumberOfChecks; i++)
            {
                await Task.Delay(100);

                if (check.Invoke())
                {
                    break;
                }
            }
        }
    }
}
