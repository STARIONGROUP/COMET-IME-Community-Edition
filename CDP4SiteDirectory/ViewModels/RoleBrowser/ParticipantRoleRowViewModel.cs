// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParticipantRoleRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
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
    /// A row-view-model that represents a <see cref="ParticipantRole"/>
    /// </summary>
    public class ParticipantRoleRowViewModel : CDP4CommonView.ParticipantRoleRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantRoleRowViewModel"/> class.
        /// </summary>
        /// <param name="role">The <see cref="ParticipantRole"/> that is represented by the current row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParticipantRoleRowViewModel(ParticipantRole role, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(role, session, containerViewModel)
        {
            foreach (var permission in this.Thing.ParticipantPermission.OrderBy(x => x.ObjectClass.ToString()))
            {
                var row = new ParticipantPermissionRowViewModel(permission, this.Session, this);
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