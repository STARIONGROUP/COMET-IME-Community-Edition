// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShowTooltipRibbonViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4SiteDirectory.ViewModels
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Composition.Events;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The Team-Composition Ribbon view-model 
    /// </summary>
    public class ShowTooltipRibbonViewModel : ReactiveObject
    {
        /// <summary>
        /// The backing field for <see cref="ShowTooltip"/> property.
        /// </summary>
        private bool showTooltip = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowTooltipRibbonViewModel"/> class
        /// </summary>
        public ShowTooltipRibbonViewModel()
        {
            this.RefreshTooltipEvent();

            this.WhenAnyValue(vm => vm.ShowTooltip)
                .Subscribe(_ => this.RefreshTooltipEvent());
        }

        /// <summary>
        /// Gets or sets a value indicating whether to display tooltips.
        /// </summary>
        public bool ShowTooltip
        {
            get { return this.showTooltip; }
            set { this.RaiseAndSetIfChanged(ref this.showTooltip, value); }
        }

        /// <summary>
        /// Updates the <see cref="ToolTipService"/>
        /// </summary>
        private void RefreshTooltipEvent()
        {
            CDPMessageBus.Current.SendMessage(new ToggleTooltipEvent(this.ShowTooltip));
        }
    }
}