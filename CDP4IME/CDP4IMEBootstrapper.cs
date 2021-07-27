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

namespace CDP4IME
{
    using System.ComponentModel.Composition.Hosting;
    using System.Windows;
    using CDP4Composition.Composition;
    using CDP4IME.Settings;
    using DevExpress.Xpf.Core;

    public class CDP4IMEBootstrapper : CDP4Bootstrapper<ImeAppSettings>
    {
        protected override void AddCustomCatalogs(AggregateCatalog catalog)
        {
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(CDP4IMEBootstrapper).Assembly));
        }

        protected override void OnComposed(CompositionContainer container)
        {
            this.UpdateBootstrapperStatus("Creating the Shell");
            Application.Current.MainWindow = container.GetExportedValue<Shell>();
        }

        protected override void UpdateBootstrapperStatus(string message)
        {
            base.UpdateBootstrapperStatus(message);
            DXSplashScreen.SetState(message);
        }
    }
}
