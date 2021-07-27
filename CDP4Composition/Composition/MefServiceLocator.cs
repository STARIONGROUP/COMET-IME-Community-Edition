// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4IMEBootstrapper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software{colon} you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation{colon} either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY{colon} without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Composition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.Linq;
    using Microsoft.Practices.ServiceLocation;

    class MefServiceLocator : IServiceLocator
    {
        private readonly CompositionContainer container;

        public MefServiceLocator(CompositionContainer container)
        {
            this.container = container;
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetExports(serviceType, null, null);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return container.GetExportedValues<TService>();
        }

        public object GetInstance(Type serviceType)
        {
            return container.GetExports(serviceType, null, null).FirstOrDefault();
        }

        public object GetInstance(Type serviceType, string key)
        {
            return container.GetExports(serviceType, null, key).FirstOrDefault();
        }

        public TService GetInstance<TService>()
        {
            return container.GetExportedValue<TService>();
        }

        public TService GetInstance<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
