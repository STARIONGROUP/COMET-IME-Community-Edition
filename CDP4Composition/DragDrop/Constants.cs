// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Constants.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System.Windows;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    public static class Constants
    {
        /// <summary>
        /// The <see cref="DragDropKeyStates"/> to perform a copy with the <see cref="IOwnedThing"/> 
        /// set to the active <see cref="DomainOfExpertise"/> and default values for potential value set
        /// </summary>
        public const DragDropKeyStates DryCopy = DragDropKeyStates.LeftMouseButton;

        /// <summary>
        /// The <see cref="DragDropKeyStates"/> to perform a copy with the <see cref="IOwnedThing"/> 
        /// set to the active <see cref="DomainOfExpertise"/> and original values for potential value set
        /// </summary>
        public const DragDropKeyStates CtrlCopy = DragDropKeyStates.LeftMouseButton | DragDropKeyStates.ControlKey;

        /// <summary>
        /// The <see cref="DragDropKeyStates"/> to perform a copy with the <see cref="IOwnedThing"/> 
        /// set to the original <see cref="DomainOfExpertise"/> and default values for potential value set
        /// </summary>
        public const DragDropKeyStates ShiftCopy = DragDropKeyStates.LeftMouseButton | DragDropKeyStates.ShiftKey;

        /// <summary>
        /// The <see cref="DragDropKeyStates"/> to perform a copy with the <see cref="IOwnedThing"/> 
        /// set to the original <see cref="DomainOfExpertise"/> and original values for potential value set
        /// </summary>
        public const DragDropKeyStates CtlrShiftCopy = DragDropKeyStates.LeftMouseButton | DragDropKeyStates.ShiftKey | DragDropKeyStates.ControlKey;
    }
}
