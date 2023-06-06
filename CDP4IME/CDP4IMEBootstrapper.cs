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

namespace CDP4IME
{
    using System.ComponentModel.Composition.Hosting;
    using System.Windows;

    using CDP4Composition.Composition;

    using CDP4IME.Settings;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// Bootstrapper implementation for the IME
    /// </summary>
    public class CDP4IMEBootstrapper : COMETBootstrapper<ImeAppSettings>
    {
        /// <summary>
        /// Adds the bootstrapper assembly to the catalogues
        /// </summary>
        /// <param name="catalog">The <see cref="AggregateCatalog"/></param>
        protected override void AddCustomCatalogs(AggregateCatalog catalog)
        {
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(CDP4IMEBootstrapper).Assembly));
        }

        /// <summary>
        /// Override to set the initial <see cref="Shell"/> window after composition
        /// </summary>
        /// <param name="container">The <see cref="AggregateCatalog"/></param>
        protected override void OnComposed(CompositionContainer container)
        {
            this.UpdateBootstrapperStatus("Starting CDP4-COMET IME");
            Application.Current.MainWindow = container.GetExportedValue<Shell>();
        }

        /// <summary>
        /// Overrides the status update to provide status updates to the <see cref="DXSplashScreen"/>
        /// </summary>
        /// <param name="message">The status message</param>
        protected override void UpdateBootstrapperStatus(string message)
        {
            base.UpdateBootstrapperStatus(message);
        }

        /// <summary>
        /// Show meesage in splash screen
        /// </summary>
        /// <param name="message">The message</param>
        protected override void ShowStatusMessage(string message)
        {
            DXSplashScreen.SetState(message);
        }

        /// <summary>
        /// Resets any progress bars to a determinate or indeterminate state and applies max value 0
        /// </summary>
        protected override void ResetStatusProgress()
        {
            DXSplashScreen.Progress(0, 0);
        }

        /// <summary>
        /// Sets the progress bar indicator to value
        /// </summary>
        /// <param name="value">The value to set the progress bar to</param>
        /// <param name="maxValue">The max value to set the progress bar to</param>
        protected override void SetStatusProgress(int value, int maxValue = 100)
        {
            DXSplashScreen.Progress(value, maxValue);
        }
    }
}
