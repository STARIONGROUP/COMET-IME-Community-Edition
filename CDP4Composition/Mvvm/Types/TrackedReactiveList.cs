// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TrackedReactiveList.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.ComponentModel;

    using DynamicData;

    /// <summary>
    /// Wrapper class for the legacy <see cref="ReactiveList{T}"/> class
    /// </summary>
    /// <typeparam name="T">The <see cref="System.Type"/></typeparam>
    public class TrackedReactiveList<T> : ReactiveList<T> where T : INotifyPropertyChanged
    {
        private IObservable<IChangeSet<T>> itemChanged;

        public IObservable<IChangeSet<T>> ItemChanged
        {
            get
            {
                if (this.itemChanged == null)
                {
                    this.itemChanged = this.SourceList.Connect();
                }

                return this.itemChanged;
            }
        }
    }
}
