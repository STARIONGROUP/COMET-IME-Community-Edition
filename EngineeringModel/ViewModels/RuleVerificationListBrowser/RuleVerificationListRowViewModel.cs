// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    /// <summary>
    /// A row representing a <see cref="CDP4Common.EngineeringModelData.RuleVerificationList"/>
    /// </summary>
    public class RuleVerificationListRowViewModel : CDP4CommonView.RuleVerificationListRowViewModel, IDropTarget
    {
        /// <summary>
        /// The backing field for <see cref="ThingCreator"/>
        /// </summary>
        private IThingCreator thingCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleVerificationListRowViewModel"/> class.
        /// </summary>
        /// <param name="ruleVerificationList">
        /// The <see cref="CDP4Common.EngineeringModelData.RuleVerificationList"/> that is represented by the current row-view-model.
        /// </param>
        /// <param name="session">
        /// The current active <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The view-model that is the container of the current row-view-model.
        /// </param>
        public RuleVerificationListRowViewModel(RuleVerificationList ruleVerificationList, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(ruleVerificationList, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row on the update of the current <see cref="Thing"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.PopulateRuleVerification();

            if (this.Owner != null)
            {
                this.OwnerName = this.Owner.Name;
                this.OwnerShortName = this.Owner.ShortName;
            }
        }

        /// <summary>
        /// Populate the <see cref="CDP4Common.EngineeringModelData.RuleVerification"/> row list
        /// </summary>
        private void PopulateRuleVerification()
        {
            var currentRuleVerifications = this.ContainedRows.Select(x => (RuleVerification)x.Thing).ToList();
            var updatedRuleVerifications = this.Thing.RuleVerification.ToList();

            var newRuleVerifications = updatedRuleVerifications.Except(currentRuleVerifications).ToList();
            var oldRuleVerifications = currentRuleVerifications.Except(updatedRuleVerifications).ToList();

            foreach (var newRuleVerification in newRuleVerifications)
            {
                if (newRuleVerification is BuiltInRuleVerification builtInRuleVerification)
                {
                    var row = new BuiltInRuleVerificationRowViewModel(builtInRuleVerification, this.Session, this);
                    this.ContainedRows.Add(row);
                }

                if (newRuleVerification is UserRuleVerification userRuleVerification)
                {
                    var row = new UserRuleVerificationRowViewModel(userRuleVerification, this.Session, this);
                    this.ContainedRows.Add(row);
                }
            }

            foreach (var ruleVerification in oldRuleVerifications)
            {
                var row = this.ContainedRows.SingleOrDefault(x => x.Thing == ruleVerification);

                if (row != null)
                {
                    this.ContainedRows.RemoveAndDispose(row);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IThingCreator"/> that is used to create different <see cref="Things"/>.
        /// </summary>
        public IThingCreator ThingCreator
        {
            get => this.thingCreator = this.thingCreator ?? ServiceLocator.Current.GetInstance<IThingCreator>();
            set => this.thingCreator = value;
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            //TODO: Add Permission handling

            if (dropInfo.Payload is Rule rule)
            {
                var rdl = (ReferenceDataLibrary)rule.Container;

                var model = (EngineeringModel)this.Thing.TopContainer;
                var modelReferenceDataLibrary = model.EngineeringModelSetup.RequiredRdl.Single();

                var requiredRdls = modelReferenceDataLibrary.GetRequiredRdls().ToList();
                requiredRdls.Add(modelReferenceDataLibrary);

                if (!requiredRdls.Contains(rdl))
                {
                    logger.Info("Rule {0} is not in the chain of Reference Data Libraries and connot be used to create a Rule Verification.", rule.Iid);
                    dropInfo.Effects = DragDropEffects.None;
                    return;
                }

                dropInfo.Effects = DragDropEffects.Copy;
                return;
            }

            if (dropInfo.Payload is IBuiltInRuleMetaData builtInRuleMetaData)
            {
                dropInfo.Effects = DragDropEffects.Copy;
                return;
            }

            logger.Info("A {0} cannot be dropped on a Rule Verification List.", dropInfo.Payload);
            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            //TODO: Add Permission handling

            if (dropInfo.Payload is Rule rule)
            {
                try
                {
                    await this.ThingCreator.CreateUserRuleVerification(this.Thing, rule, this.Session);
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = ex.Message;
                }
            }

            if (dropInfo.Payload is IBuiltInRuleMetaData builtInRuleMetaData)
            {
                try
                {
                    await this.ThingCreator.CreateBuiltInRuleVerification(this.Thing, builtInRuleMetaData.Name, this.Session);
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = ex.Message;
                }
            }

            dropInfo.Effects = DragDropEffects.None;
        }
    }
}
