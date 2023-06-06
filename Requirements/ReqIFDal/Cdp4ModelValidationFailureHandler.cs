// -------------------------------------------------------------------------------------------------
// <copyright file="Cdp4ModelValidationFailureHandler.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ReqIFDal
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    using CDP4Common.Exceptions;

    using CDP4Composition.Services;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Handles Cdp4ModelValidation errors during the ReqIf creation process
    /// </summary>
    public class Cdp4ModelValidationFailureHandler
    {
        /// <summary>
        /// The injected <see cref="IMessageBoxService"/>
        /// </summary>
        private IMessageBoxService messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

        /// <summary>
        /// Indicates that CDP4-COMET model failures that resulted in a <see cref="Cdp4ModelValidationException"/>s were ignored by the user.
        /// </summary>
        private bool cdp4ModelValidationFailuresIgnored;

        /// <summary>
        /// A <see cref="HashSet{T}"/> of type <see cref="string"/> that contains all Model validation messages.
        /// </summary>
        private HashSet<string> cdp4ModelValidationFailures;

        /// <summary>
        /// (Re)Starts the <see cref="Cdp4ModelValidationFailureHandler"/>
        /// </summary>
        public void ReStartHandler()
        {
            this.cdp4ModelValidationFailuresIgnored = false;
            this.cdp4ModelValidationFailures = new HashSet<string>();
        }

        /// <summary>
        /// Checks if Model Validation Failures are ignored already and acts accordingly
        /// </summary>
        /// <param name="message">The ValidationService error message</param>
        /// <returns>True is Model Validation Failures are ignored, otherwise false</returns>
        public bool CheckCdp4ModelValidationMessaging(string message)
        {
            this.cdp4ModelValidationFailures.Add(message);

            if (!this.cdp4ModelValidationFailuresIgnored)
            {
                if (this.messageBoxService.Show(
                    $"CDP4-COMET Model validation failure found:\n{message}) \n\nNot all data can be added to the ReqIf.\n\nSkip data that results in CDP4-COMET Model validation failures?",
                    $"CDP4-COMET Model validation failure ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
                {
                    this.cdp4ModelValidationFailuresIgnored = true;
                }
                else
                {
                    throw new Cdp4ModelValidationException(message);
                }
            }

            return this.cdp4ModelValidationFailuresIgnored;
        }

        /// <summary>
        /// Report the found Model Validation errors to the user
        /// </summary>
        public void ReportCdp4ModelValidations()
        {
            if (this.cdp4ModelValidationFailuresIgnored && this.cdp4ModelValidationFailures.Any())
            {
                var missingParameterizedCategoryRules = string.Join("\n\n", this.cdp4ModelValidationFailures.Distinct());

                this.messageBoxService.Show(
                    $"The following CDP4-COMET Model validation failures were ignored: \n\n {missingParameterizedCategoryRules}", 
                    "CDP4-COMET Model validation failures",
                    MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }
    }
}
