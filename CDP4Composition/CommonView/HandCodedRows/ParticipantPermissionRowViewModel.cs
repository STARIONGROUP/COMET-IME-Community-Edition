// ------------------------------------------------------------------------------------------------
// <copyright file="ParticipantPermissionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using CDP4Common.CommonData;
    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="ParticipantPermissionRowViewModel"/>
    /// </summary>
    public partial class ParticipantPermissionRowViewModel
    {
        /// <summary>
        /// The possible <see cref="PersonAccessRightKind"/>
        /// </summary>
        private readonly ReactiveList<ParticipantAccessRightKind> possibleRightKinds = new ReactiveList<ParticipantAccessRightKind>
        {
            ParticipantAccessRightKind.NONE,
            ParticipantAccessRightKind.MODIFY,
            ParticipantAccessRightKind.MODIFY_IF_OWNER,
            ParticipantAccessRightKind.READ,
        };

        /// <summary>
        /// Gets the Possible right kinds
        /// </summary>
        public ReactiveList<ParticipantAccessRightKind> PossibleRightKinds
        {
            get { return this.possibleRightKinds; }
        }
    }
}