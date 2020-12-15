// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedThingRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Adrian Chivu, Cozmin Velciu, Alex Vorobiev
//
//    This file is part of CDP4-Server-Administration-Tool.
//    The CDP4-Server-Administration-Tool is an ECSS-E-TM-10-25 Compliant tool
//    for advanced server administration.
//
//    The CDP4-Server-Administration-Tool is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as
//    published by the Free Software Foundation; either version 3 of the
//    License, or (at your option) any later version.
//
//    The CDP4-Server-Administration-Tool is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.RowModels
{
    using CDP4Common.CommonData;
    using ReactiveUI;
    using System;

    /// <summary>
    /// Row class representing a <see cref="DefinedThing"/> as a plain object
    /// </summary>
    public abstract class DefinedThingRowViewModel<T> : ReactiveObject where T : DefinedThing
    {
        /// <summary>
        /// Backing field for <see cref="Thing"/>
        /// </summary>
        private Thing thing;

        /// <summary>
        /// Gets or sets the thing
        /// </summary>
        public Thing Thing
        {
            get => this.thing;
            set => this.RaiseAndSetIfChanged(ref this.thing, value);
        }

        /// <summary>
        /// Backing field for <see cref="Iid"/>
        /// </summary>
        private Guid iid;

        /// <summary>
        /// Gets or sets the iid
        /// </summary>
        public Guid Iid
        {
            get => this.iid;
            set => this.RaiseAndSetIfChanged(ref this.iid, value);
        }

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Gets or sets the shortName
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedThingRowViewModel{T}"/> class
        /// </summary>
        /// <param name="thing">
        /// The <see cref="DefinedThing"/> associated with this row
        /// </param>
        protected DefinedThingRowViewModel(T thing)
        {
            this.Thing = thing;
            this.Iid = thing.Iid;
            this.Name = thing.Name;
            this.ShortName = thing.ShortName;
        }
    }
}
