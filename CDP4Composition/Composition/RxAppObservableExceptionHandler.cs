// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RxAppObservableExceptionHandler.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Composition
{
    using System;

    /// <summary>
    /// Default Exception handler for ReactiveCommands, otherwise every error in command execution will result in a crash of the application
    /// </summary>
    public class RxAppObservableExceptionHandler : IObserver<Exception>
    {
        /// <summary>
        /// The OnNext handler
        /// </summary>
        /// <param name="value">The <see cref="Exception"/></param>
        public void OnNext(Exception value)
        {
            //Do nothing, just swallow the error by default to stay backwards compatible
        }

        /// <summary>
        /// The OnError handler
        /// </summary>
        /// <param name="error">The <see cref="Exception"/></param>
        public void OnError(Exception error)
        {
            //Do nothing, just swallow the error by default to stay backwards compatible
        }

        /// <summary>
        /// The ConComplete handler
        /// </summary>
        public void OnCompleted()
        {
            //Do nothing, just swallow the error by default to stay backwards compatible
        }
    }
}
