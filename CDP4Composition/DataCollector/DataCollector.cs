// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollector.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4Composition.DataCollector
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    public abstract class DataCollector : IDataCollector
    {
        /// <summary>
        /// Gets or sets the <see cref="Iteration"/>
        /// </summary>
        public Iteration Iteration { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise"/>
        /// </summary>
        public DomainOfExpertise DomainOfExpertise { get; private set; }

        /// <summary>
        /// Gets the list of available <see cref="Option"/>s
        /// </summary>
        public IEnumerable<Option> Options => this.Iteration?.Option as IEnumerable<Option> ?? new List<Option>();

        /// <summary>
        /// Initializes this DataCollector 
        /// </summary>
        /// <param name="iteration"></param>
        /// <param name="session"></param>
        public void Initialize(Iteration iteration, ISession session)
        {
            this.Iteration = iteration;
            this.Session = session;
            this.DomainOfExpertise = session.QueryCurrentDomainOfExpertise();
        }

        /// <summary>
        /// Finds <see cref="NestedParameter"/>s by their <see cref="NestedParameter.Path"/>s in the <see cref="Option"/>'s <see cref="NestedParameter"/>
        /// and returns its <see cref="NestedParameter.ActualValue"/> "converted" to the generic <typeparamref name="T"></typeparamref>'s .
        /// </summary>
        /// <typeparam name="T">The generic type to which the <see cref="NestedParameter.ActualValue"/> needs to be "converted".</typeparam>
        /// <param name="option">The <see cref="Option"/> in which to find the <see cref="NestedParameter"/>s.</param>
        /// <param name="path">The path to search for in all this <see cref="Option"/>'s <see cref="NestedParameter.Path"/> properties.</param>
        /// <param name="index">
        /// Index/position of the wanted value from the result array of <see cref="Option.GetNestedParameterValuesByPath{T}"/>
        /// 0-based indexing is used for this.
        /// </param>
        /// <returns>A single <see cref="NestedParameter"/> if the path was found and its <see cref="NestedParameter.ActualValue"/>
        /// could be converted to the requested generic <typeparamref name="T"></typeparamref>, otherwise null.</returns>
        public T GetNestedParameterValueByPath<T>(Option option, string path, int index = 0)
        {
            return (T)Convert.ChangeType(option.GetNestedParameterValuesByPath<object>(path, this.DomainOfExpertise).ToArray()[index], typeof(T));
        }

        /// <summary>
        /// Creates a new data collection instance.
        /// </summary>
        /// <returns>
        /// An object instance.
        /// </returns>
        public abstract object CreateDataObject();
    }
}
