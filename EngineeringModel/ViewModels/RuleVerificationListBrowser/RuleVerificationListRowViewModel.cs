// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleVerificationListRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using CDP4EngineeringModel.Utilities;

    /// <summary>
    /// A row representing a <see cref="CDP4Common.EngineeringModelData.RuleVerificationList"/>
    /// </summary>
    public class RuleVerificationListRowViewModel : CDP4CommonView.RuleVerificationListRowViewModel, IDropTarget
    {
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

        #region RowBase
        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        #endregion

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
                var builtInRuleVerification = newRuleVerification as BuiltInRuleVerification;
                if (builtInRuleVerification != null)
                {
                    var row = new BuiltInRuleVerificationRowViewModel(builtInRuleVerification, this.Session, this);
                    this.ContainedRows.Add(row);
                }

                var userRuleVerification = newRuleVerification as UserRuleVerification;
                if (userRuleVerification != null)
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
                    this.ContainedRows.Remove(row);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IThingCreator"/> that is used to create different <see cref="Things"/>.
        /// </summary>
        public IThingCreator ThingCreator { get; set; }

        #region IDropTarget

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

            var rule = dropInfo.Payload as Rule;
            if (rule != null)
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

            var builtInRuleMetaData = dropInfo.Payload as IBuiltInRuleMetaData;
            if (builtInRuleMetaData != null)
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

            var rule = dropInfo.Payload as Rule;
            if (rule != null)
            {
                if (this.ThingCreator == null)
                {
                    this.ThingCreator = new ThingCreator();
                }

                try
                {
                    await this.ThingCreator.CreateUserRuleVerification(this.Thing, rule, this.Session);
                }
                catch (Exception ex)
                {
                    this.ErrorMsg = ex.Message;
                }
            }

            var builtInRuleMetaData = dropInfo.Payload as IBuiltInRuleMetaData;
            if (builtInRuleMetaData != null)
            {
                if (this.ThingCreator == null)
                {
                    this.ThingCreator = new ThingCreator();
                }

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

        #endregion IDropTarget
    }
}
