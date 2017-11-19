// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonRoleRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;

    /// <summary>
    /// A row-view-model that represents a <see cref="PersonRole"/>
    /// </summary>
    public class PersonRoleRowViewModel : CDP4CommonView.PersonRoleRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleRowViewModel"/> class.
        /// </summary>
        /// <param name="role">The <see cref="PersonRole"/> that is represented by the current row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public PersonRoleRowViewModel(PersonRole role, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(role, session, containerViewModel)
        {
            foreach (var personPermission in this.Thing.PersonPermission.OrderBy(x => x.ObjectClass.ToString()))
            {
                var row = new PersonPermissionRowViewModel(personPermission, this.Session, this);
                this.ContainedRows.Add(row);
            }

            this.ClassKind = this.Thing.ClassKind.ToString();
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="Thing"/> that is represented by the current view-model
        /// </summary>
        public string ClassKind { get; private set; }
    }
}