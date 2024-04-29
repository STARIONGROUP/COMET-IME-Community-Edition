// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingSelectorDialogService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Navigation
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Windows;

    using CDP4Common.CommonData;

    using CDP4Composition.ViewModels;
    using CDP4Composition.Views;

    /// <summary>
    /// The service that handles the selection of a <see cref="Thing"/>
    /// </summary>
    [Export(typeof(IThingSelectorDialogService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThingSelectorDialogService : IThingSelectorDialogService
    {
        /// <summary>
        /// Select a single <see cref="Thing"/> from the list of <param name="things"></param>, using the <see cref="ThingSelectorDialog"/> dialog window. />
        /// </summary>
        /// <typeparam name="T">Generic type reference that should be of type <see cref="Thing"/></typeparam>
        /// <param name="things"><see cref="IEnumerable{T}"/> where the user can choose from</param>
        /// <param name="fieldNames"><see cref="IEnumerable{String}"/> that contains fieldnames of <see cref="Thing"/> data to be shown</param>
        /// <returns>
        /// A <see cref="Thing"/> of the specified generic type or null if the operation of the dialog was cancelled.
        /// </returns>
        public T SelectThing<T>(IEnumerable<T> things, IEnumerable<string> fieldNames) where T : Thing
        {
            var viewModel = new ThingSelectorDialogViewModel<T>(things, fieldNames);
            var view = new ThingSelectorDialog { DataContext = viewModel, WindowStartupLocation = WindowStartupLocation.CenterScreen };

            var result = view.ShowDialog();

            return result == true ? viewModel.SelectedThing : null;
        }
    }
}
