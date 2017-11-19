// -------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRoleDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="ParticipantRoleDialogViewModel"/> is to allow an <see cref="ParticipantRole"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="ParticipantRole"/> will result in an <see cref="ParticipantRole"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.ParticipantRole)]
    public class ParticipantRoleDialogViewModel : CDP4CommonView.ParticipantRoleDialogViewModel, IThingDialogViewModel
    {
        #region Fields

        /// <summary>
        /// The <see cref="MetaDataProvider"/> used to get the version of the classes used to populate the <see cref="ParticipantPermission"/>s
        /// </summary>
        private readonly IMetaDataProvider metadataProvider = StaticMetadataProvider.GetMetaDataProvider;

        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRoleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ParticipantRoleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRoleDialogViewModel"/> class.
        /// </summary>
        /// <param name="participantRole">
        /// The participant Role.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="ParticipantRoleDialogViewModel"/> is the root of all dialogs
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="ParticipantRoleDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="container">
        /// The container <see cref="Thing"/> for the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public ParticipantRoleDialogViewModel(ParticipantRole participantRole, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(participantRole, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }
        #endregion

        /// <summary>
        /// Gets a value indicating whether the person field should be readonly
        /// </summary>
        public bool IsPersonEditable
        {
            get { return this.dialogKind == ThingDialogKind.Create; }
        }

        /// <summary>
        /// Populates the <see cref="ParticipantPermission"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected override void PopulateParticipantPermission()
        {
            this.ParticipantPermission.Clear();
            var participantRoleWithAllPermissions = new ParticipantRole { Container = this.Container };
            foreach (var participantPermission in participantRoleWithAllPermissions.ParticipantPermission.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var permissionObjectClassVersion = new Version(this.metadataProvider.GetClassVersion(participantPermission.ObjectClass.ToString()));
                if (!this.Session.IsVersionSupported(permissionObjectClassVersion))
                {
                    continue;
                }

                var permissionToAdd = this.Thing.ParticipantPermission.SingleOrDefault(x => x.ObjectClass == participantPermission.ObjectClass) ?? participantPermission;
                var row = new CDP4CommonView.ParticipantPermissionRowViewModel(permissionToAdd, this.Session, this);
                this.ParticipantPermission.Add(row);
            }
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            // creation and update of contained Things outside the "normal" process
            foreach (var participantPermission in this.ParticipantPermission)
            {
                if (this.dialogKind == ThingDialogKind.Create)
                {
                    var permission = participantPermission.Thing;
                    permission.AccessRight = participantPermission.AccessRight;

                    this.transaction.Create(permission);
                }
                else if (this.dialogKind == ThingDialogKind.Update)
                {
                    var updatedPermission = participantPermission.Thing.Clone(false);
                    var index = this.Thing.ParticipantPermission.FindIndex(x => x.Iid == updatedPermission.Iid);
                    if (index != -1 && updatedPermission.AccessRight == participantPermission.AccessRight)
                    {
                        continue;
                    }

                    updatedPermission.AccessRight = participantPermission.AccessRight;
                    if (index != -1)
                    {
                        this.Thing.ParticipantPermission[index] = updatedPermission;
                    }
                    else
                    {
                        this.Thing.ParticipantPermission.Add(updatedPermission);
                    }

                    this.transaction.CreateOrUpdate(updatedPermission);
                }
            }
        }
    }
}