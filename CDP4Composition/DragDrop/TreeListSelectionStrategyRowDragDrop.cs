// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeListSelectionStrategyRowDragDrop.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System.Windows;
    using System.Windows.Input;

    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    /// <summary>
    /// Extended row selection strategy that override mouse down and up behaviors when using Ctrl key
    /// </summary>
    public class TreeListSelectionStrategyRowDragDrop : TreeListSelectionStrategyRow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeListSelectionStrategyRowDragDrop"/> class
        /// </summary>
        /// <param name="view">The tre view</param>
        public TreeListSelectionStrategyRowDragDrop(TreeListView view) : base(view)
        {
        }

        /// <summary>
        /// Override of the Left mouse button down behavior
        /// </summary>
        /// <param name="hitInfo">The hitinfo</param>
        protected override void OnAfterMouseLeftButtonDownCore(IDataViewHitInfo hitInfo)
        {
            if (Keyboard2.IsControlPressed)
            {
                return;
            }

            base.OnAfterMouseLeftButtonDownCore(hitInfo);
        }

        /// <summary>
        /// Override of the Left mouse button up behavior
        /// </summary>
        /// <param name="e">The mouse args</param>
        public override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (Keyboard2.IsControlPressed)
            {
                base.OnAfterMouseLeftButtonDownCore(((TreeListView)this.view).CalcHitInfo((DependencyObject)e.OriginalSource));
            }

            base.OnMouseLeftButtonUp(e);
        }
    }
}
