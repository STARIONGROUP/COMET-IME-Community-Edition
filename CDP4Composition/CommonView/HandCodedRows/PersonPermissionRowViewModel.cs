// ------------------------------------------------------------------------------------------------
// <copyright file="PersonPermissionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using CDP4Common.CommonData;
    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="PersonPermissionRowViewModel"/>
    /// </summary>
    public partial class PersonPermissionRowViewModel
    {
        /// <summary>
        /// The possible <see cref="PersonAccessRightKind"/>
        /// </summary>
        private readonly ReactiveList<PersonAccessRightKind> possibleRightKinds = new ReactiveList<PersonAccessRightKind>
        {
            PersonAccessRightKind.NONE,
            PersonAccessRightKind.MODIFY,
            PersonAccessRightKind.MODIFY_IF_PARTICIPANT,
            PersonAccessRightKind.MODIFY_OWN_PERSON,
            PersonAccessRightKind.READ,
            PersonAccessRightKind.READ_IF_PARTICIPANT
        };

        /// <summary>
        /// Gets the Possible right kinds
        /// </summary>
        public ReactiveList<PersonAccessRightKind> PossibleRightKinds
        {
            get { return this.possibleRightKinds; }
        }
    }
}