// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesigner.xaml.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for ReportDesigner.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public partial class ReportDesigner : IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        public ReportDesigner()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesigner"/> class.
        /// </summary>
        /// <param name="initializeComponent">A value indicating whether the contained Components shall be loaded</param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ReportDesigner(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }

        /// <summary>
        /// Trigger textboxes text changed event
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/></param>
        private void Focus_Me(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.ErrorTextBox.Text))
            {
                this.lgTabs.SelectTab(this.lgErrors);
            }
            else if (Equals(e.Source, this.OutputTextBox))
            {
                this.lgTabs.SelectTab(this.lgOutput);
            }
        }
    }
}
