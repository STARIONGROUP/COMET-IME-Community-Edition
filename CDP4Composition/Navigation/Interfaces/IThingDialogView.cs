// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IThingDialogView.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Interfaces
{
    using CDP4Composition.Mvvm;

    public interface IThingDialogView : IView
    {
        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <returns>
        /// A System.Nullable{T} value of type System.Boolean that specifies whether the activity was accepted (true) or canceled (false).
        /// The return value is the value of the System.Windows.Window.DialogResult property before a window closes.
        /// </returns>
        bool? ShowDialog();
    }
}
