﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShowDeprecatedRibbon.xaml.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System;
    using System.ComponentModel.Composition;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Events;
    using CDP4Composition.Ribbon;
    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using CDP4SiteDirectory.ViewModels;

    /// <summary>
    /// Interaction logic for ShowDeprecatedRibbon
    /// </summary>
    [Export(typeof(ExtendedRibbonPageGroup))]
    public partial class ShowTooltipRibbon : IView
    {
        private readonly Style disableToolTip;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowTooltipRibbon"/> class
        /// </summary>
        public ShowTooltipRibbon()
        {
            this.InitializeComponent();
            this.DataContext = new ShowTooltipRibbonViewModel();
            CDPMessageBus.Current.Listen<ToggleTooltipEvent>().Subscribe(this.ToggleTooltipEventHandler);

            this.disableToolTip = new Style(typeof(ToolTip));
            this.disableToolTip.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            this.disableToolTip.Seal();
        }

        private void ToggleTooltipEventHandler(ToggleTooltipEvent toggleTooltipEvent)
        {
            if (Application.Current == null)
            {
                //Not supported for Excel plugin
            }
            else
            {
                if (toggleTooltipEvent.ShouldShow)
                {
                    Application.Current.Resources.Remove(typeof(ToolTip));
                }
                else
                {
                    Application.Current.Resources[typeof(ToolTip)] = this.disableToolTip;
                }
            }
        }
    }
}
