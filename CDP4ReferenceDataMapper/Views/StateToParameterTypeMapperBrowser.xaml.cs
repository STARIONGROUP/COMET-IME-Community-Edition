﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateToParameterTypeMapperBrowser.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ReferenceDataMapper.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for OptionBrowser.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class StateToParameterTypeMapperBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StateToParameterTypeMapperBrowser"/> class.
        /// </summary>
        public StateToParameterTypeMapperBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateToParameterTypeMapperBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public StateToParameterTypeMapperBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}