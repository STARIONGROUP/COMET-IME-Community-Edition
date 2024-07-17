// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MefServiceLocator.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Composition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;

    using CommonServiceLocator;

    /// <summary>
    /// Provides service location utilizing the Managed Extensibility Framework container.
    /// </summary>
    public class MefServiceLocator : ServiceLocatorImplBase
    {
        /// <summary>
        /// The MEF <see cref="CompositionContainer"/>
        /// </summary>
        private readonly CompositionContainer compositionContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MefServiceLocator"/> class.
        /// </summary>
        /// <param name="compositionContainer">The MEF composition container.</param>
        public MefServiceLocator(CompositionContainer compositionContainer)
        {
            this.compositionContainer = compositionContainer;
        }

        /// <summary>
        /// Resolves the instance of the requested service.
        /// </summary>
        /// <param name="serviceType">Type of instance requested.</param>
        /// <returns>The requested service instance.</returns>
        protected override object DoGetInstance(Type serviceType, string key)
        {
            var exports = this.compositionContainer.GetExports(serviceType, null, key);
            if (exports is not null && exports.Any())
            {
                // If there is more than one value, this will throw an InvalidOperationException, 
                // which will be wrapped by the base class as an ActivationException.
                return exports.Single().Value;
            }

            throw new ActivationException(
                this.FormatActivationExceptionMessage(new CompositionException("Export not found"), serviceType, key));
        }

        /// <summary>
        /// Resolves all the instances of the requested service.
        /// </summary>
        /// <param name="serviceType">Type of service requested.</param>
        /// <param name="key">Name of registered service you want. May be null.</param>
        /// <returns>Sequence of service instance objects.</returns>
        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            var instances = new List<object>();

            var exports = this.compositionContainer.GetExports(serviceType, null, null);
            if (exports is not null)
            {
                instances.AddRange(exports.Select(export => export.Value));
            }

            return instances;
        }
    }
}
