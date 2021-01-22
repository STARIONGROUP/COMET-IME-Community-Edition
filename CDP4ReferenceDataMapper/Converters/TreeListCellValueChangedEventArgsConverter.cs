// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TreeListCellValueChangedEventArgsConverter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.Converters
{
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Data;

    using DevExpress.Mvvm.UI;
    using DevExpress.Xpf.Grid.TreeList;

    /// <summary>
    /// The <see cref="IValueConverter"/> that gets a tuple that holds specific data from a <see cref="TreeListCellValueChangedEventArgs"/>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TreeListCellValueChangedEventArgsConverter : EventArgsConverterBase<TreeListCellValueChangedEventArgs>
    {
        /// <summary>
        /// Creates a tuple that holds specific data from a <see cref="TreeListCellValueChangedEventArgs"/>
        /// </summary>
        /// <param name="sender">The sender <see cref="object"/></param>
        /// <param name="args">The <see cref="TreeListCellValueChangedEventArgs"/></param>
        /// <returns></returns>
        protected override object Convert(object sender, TreeListCellValueChangedEventArgs args)
        {
            if (args == null)
            {
                return null;
            }

            return (args.Cell.Row as DataRowView, args.Cell.Property, args.Cell.Value as string);
        }
    }
}
