// -------------------------------------------------------------------------------------------------
// <copyright file="PersonRoleDialogViewModel.cs" company="RHEA System S.A.">
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
    /// The purpose of the <see cref="PersonRoleDialogViewModel"/> is to allow an <see cref="PersonRole"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="PersonRole"/> will result in an <see cref="PersonRole"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.PersonRole)]
    public class PersonRoleDialogViewModel : CDP4CommonView.PersonRoleDialogViewModel, IThingDialogViewModel
    {
        #region Fields

        /// <summary>
        /// The <see cref="MetaDataProvider"/> used to get the version of the classes used to populate the <see cref="PersonPermission"/>s
        /// </summary>
        private readonly IMetaDataProvider metadataProvider = StaticMetadataProvider.GetMetaDataProvider;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public PersonRoleDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleDialogViewModel"/> class.
        /// </summary>
        /// <param name="personRole">
        /// The <see cref="PersonRole"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="PersonRoleDialogViewModel"/> is the root of all dialogs
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="PersonRoleDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/> that allows to navigate to <see cref="Thing"/> dialog view models
        /// </param>
        /// <param name="container">The container <see cref="Thing"/> for the created <see cref="Thing"/></param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public PersonRoleDialogViewModel(PersonRole personRole, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container, IEnumerable<Thing> chainOfContainers = null)
            : base(personRole, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        /// <summary>
        /// Populates the <see cref="PersonPermission"/> property with the content of the actual thing and the content of the transaction
        /// </summary>
        protected override void PopulatePersonPermission()
        {
            this.PersonPermission.Clear();
            var personRoleWithAllPermissions = new PersonRole { Container = this.Container };
            foreach (var personPermission in personRoleWithAllPermissions.PersonPermission.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var permissionObjectClassVersion = new Version(this.metadataProvider.GetClassVersion(personPermission.ObjectClass.ToString()));
                if (!this.Session.IsVersionSupported(permissionObjectClassVersion))
                {
                    continue;
                }

                var permissionToAdd = this.Thing.PersonPermission.SingleOrDefault(x => x.ObjectClass == personPermission.ObjectClass) ?? personPermission;
                var row = new CDP4CommonView.PersonPermissionRowViewModel(permissionToAdd, this.Session, this);
                this.PersonPermission.Add(row);
            }
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            // creation and update of contained Things outside the "normal" process
            foreach (var personPermission in this.PersonPermission)
            {
                if (this.dialogKind == ThingDialogKind.Create)
                {
                    var permission = personPermission.Thing;
                    permission.AccessRight = personPermission.AccessRight;

                    this.transaction.Create(permission);
                }
                else if (this.dialogKind == ThingDialogKind.Update)
                {
                    var updatedPermission = personPermission.Thing.Clone(false);
                    var index = this.Thing.PersonPermission.FindIndex(x => x.Iid == updatedPermission.Iid);
                    if (index != -1 && updatedPermission.AccessRight == personPermission.AccessRight)
                    {
                        continue;
                    }

                    updatedPermission.AccessRight = personPermission.AccessRight;
                    if (index != -1)
                    {
                        this.Thing.PersonPermission[index] = updatedPermission;
                    }
                    else
                    {
                        this.Thing.PersonPermission.Add(updatedPermission);
                    }

                    this.transaction.CreateOrUpdate(updatedPermission);
                }
            }
        }
    }
}