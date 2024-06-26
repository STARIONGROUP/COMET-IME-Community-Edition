// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrozenIterationExceptionHandler.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.ExceptionHandlers
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows;

    using CDP4Common.ExceptionHandlerService;

    using CDP4Composition.Services;

    using CDP4Dal.Exceptions;

    /// <summary>
    /// The purpose of the <see cref="FrozenIterationExceptionHandler" /> is to check exceptions for and start an IME process
    /// </summary>
    [Export(typeof(IExceptionHandler))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FrozenIterationExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// The injected <see cref="IMessageBoxService"/>
        /// </summary>
        private IMessageBoxService messageboxService = CommonServiceLocator.ServiceLocator.Current.GetInstance<IMessageBoxService>();

        /// <summary>
        /// Handles a specific <see cref="Exception"/> and enables the IME to start a process based on the ocntent or type of the <see cref="Exception"/>
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="Exception"/></typeparam>
        /// <param name="exception">The <see cref="Exception"/></param>
        public bool HandleException(Exception exception) 
        {
            if (exception is not DalWriteException dalException)
            {
                return false;
            }

            if (dalException.Message.Contains("#FROZEN_ITERATION"))
            {
                this.messageboxService.Show("It is not allowed to write data to a frozen IterationSetup.\nPlease close the current Iteration and open the new Iteration to be able to write data to the current active Iteration.", "Iteration is Frozen", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            return false;
        }
    }
}
