// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DockOperationBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Behaviors
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using CDP4Composition;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Docking;
    using DevExpress.Xpf.Docking.Base;
    using DevExpress.Xpf.Layout.Core;

    /// <summary>
    /// Customizes the docking behaviour when a group of tabs is moved from a side pannel to the document group.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DockOperationBehavior : Behavior<DockLayoutManager>
    {
        /// <summary>
        /// Register event handlers
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.DockOperationStarting += this.AssociatedObject_DockOperationStarting;
        }

        /// <summary>
        /// Unregister event handlers
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.DockOperationStarting -= this.AssociatedObject_DockOperationStarting;
        }

        /// <summary>
        /// Overrides the dock operation to add all tabs from the side group being docked to the document group then places the side group back to its original layout position.
        /// </summary>
        /// <param name="sender">the sender</param>
        /// <param name="e">the arguments</param>
        private void AssociatedObject_DockOperationStarting(object sender, DockOperationStartingEventArgs e)
        {
            if (e.DockOperation is not DockOperation.Dock)
            {
                return;
            }

            if (e.DockTarget is not DocumentGroup documentGroup)
            {
                return;
            }

            if (e.Item is not TabbedGroup tabbedGroup)
            {
                return;
            }

            var groupName = tabbedGroup.Name;

            if (string.IsNullOrWhiteSpace(groupName))
            {
                return;
            }

            e.Cancel = true;
            var itemsToCopy = tabbedGroup.Items.ToArray();
            tabbedGroup.Items.Clear();
            documentGroup.AddRange(itemsToCopy);

            switch (groupName)
            {
                case LayoutGroupNames.LeftGroup:
                    tabbedGroup.GetDockLayoutManager().DockController.Dock(tabbedGroup, documentGroup.Parent, DockType.Left);
                    break;
                case LayoutGroupNames.RightGroup:
                    tabbedGroup.GetDockLayoutManager().DockController.Dock(tabbedGroup, documentGroup.Parent, DockType.Right);
                    break;
            }
        }
    }
}
