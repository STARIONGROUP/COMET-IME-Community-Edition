// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonPermissionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Converters;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// A row-view-model that represents a <see cref="PersonPermission"/>
    /// </summary>
    public class PersonPermissionRowViewModel : CDP4CommonView.PersonPermissionRowViewModel
    {
        /// <summary>
        /// The camel case to space converter.
        /// </summary>
        private readonly CamelCaseToSpaceConverter camelCaseToSpaceConverter = new CamelCaseToSpaceConverter();
        
        /// <summary>
        /// Backing field for <see cref="IsReadOnly"/>
        /// </summary>
        private bool isReadOnly;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPermissionRowViewModel"/> class.
        /// </summary>
        /// <param name="permission">The <see cref="PersonPermission"/> that is represented by the current row-view-model</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public PersonPermissionRowViewModel(PersonPermission permission, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(permission, session, containerViewModel)
        {
            this.UpdatePermission();
            this.WhenAnyValue(x => x.AccessRight).Subscribe(_ => this.ExecuteUpdatePermission());
        }

        /// <summary>
        /// Gets the ClassKind to display it in the "Name" column of the Role browser.
        /// </summary>
        public string Name
        {
            get
            {
                return this.camelCaseToSpaceConverter.Convert(this.ObjectClass, null, null, null).ToString();
            }
        }

        /// <summary>
        /// Gets the ClassKind to display it in the "Name" column of the Role browser.
        /// </summary>
        public ClassKind ShortName
        {
            get
            {
                return this.ObjectClass;
            }
        }

        /// <summary>
        /// Gets the Access right to display it in the "AccessType" column of the Role browser.
        /// </summary>
        public string AccessType
        {
            get
            {
                return this.AccessRight.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current user may change the permission
        /// </summary>
        public bool IsReadOnly
        {
            get { return this.isReadOnly; }
            set { this.RaiseAndSetIfChanged(ref this.isReadOnly, value); }
        }

        /// <summary>
        /// Update the permission
        /// </summary>
        private void ExecuteUpdatePermission()
        {
            if (this.AccessRight == this.Thing.AccessRight)
            {
                return;
            }

            var clone = this.Thing.Clone(false);
            clone.AccessRight = this.AccessRight;

            var transactionContext = TransactionContextResolver.ResolveContext(this.Thing);
            var transaction = new ThingTransaction(transactionContext, clone);

            //TODO: add try catch to report any error back to user?
            this.DalWrite(transaction);
        }

        /// <summary>
        /// Update Permission
        /// </summary>
        private void UpdatePermission()
        {
            this.IsReadOnly = !this.Session.PermissionService.CanWrite(this.Thing);
        }
    }
}