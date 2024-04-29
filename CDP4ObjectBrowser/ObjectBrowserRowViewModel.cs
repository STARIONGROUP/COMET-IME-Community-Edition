// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowserRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4ObjectBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The Base view-model class for Object Browser rows
    /// </summary>
    /// <typeparam name="T">The <see cref="Thing"/> represented by the row</typeparam>
    public abstract class ObjectBrowserRowViewModel<T> : RowViewModelBase<T> where T: Thing
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectBrowserRowViewModel{T}"/> class
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected ObjectBrowserRowViewModel(T thing, ISession session, IViewModelBase<Thing> containerViewModel) : base(thing, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets or set a string representing the <see cref="name"/> property
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or set a string representing the <see cref="shortName"/> property
        /// </summary>
        public string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or set a string representing the <see cref="description"/> property
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateColumnValues();
        }

        /// <summary>
        /// Updates the values of columns
        /// </summary>
        protected virtual void UpdateColumnValues()
        {
        }

        /// <summary>
        /// Compute the rows to remove and to add from a <see cref="IEnumerable{TThing}"/> in the <paramref name="folderRow"/> list
        /// </summary>
        /// <typeparam name="TThing">The type of <see cref="Thing"/> represented by the rows</typeparam>
        /// <param name="currentThings">The current <see cref="IEnumerable{TThing}"/> that shall be represented</param>
        /// <param name="folderRow">The <see cref="FolderRowViewModel"/> that contains the rows representing the <see cref="TThing"/></param>
        /// <param name="createRowMethod">The method that instantiates and adds the rows to the <see cref="ContainedRows"/> list</param>
        protected void ComputeRows<TThing>(IEnumerable<TThing> currentThings, CDP4Composition.FolderRowViewModel folderRow, Func<TThing, IRowViewModelBase<TThing>> createRowMethod) where TThing : Thing
        {
            var current = currentThings.ToList();

            var existingRowThing = folderRow.ContainedRows.Where(x => x.Thing is TThing).Select(x => (TThing)x.Thing).ToList();
            var newThing = current.Except(existingRowThing).ToList();
            var oldThing = existingRowThing.Except(current).ToList();

            foreach (var thing in oldThing)
            {
                var row = folderRow.ContainedRows.SingleOrDefault(rowViewModel => rowViewModel.Thing == thing);
                if (row != null)
                {
                    folderRow.ContainedRows.RemoveAndDispose(row);
                }
            }

            foreach (var thing in newThing)
            {
                var row = createRowMethod(thing);
                folderRow.ContainedRows.Add(row);
            }
        }
    }
}
