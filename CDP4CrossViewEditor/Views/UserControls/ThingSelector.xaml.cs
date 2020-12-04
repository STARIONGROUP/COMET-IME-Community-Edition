// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingSelector.xaml.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.Views.UserControls
{
    using System;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4CrossViewEditor.ViewModels;

    /// <summary>
    /// Interaction logic for ThingSelector.xaml
    /// </summary>
    public partial class ThingSelector : ThingUserControl
    {
        /// <summary>
        /// Instance handler which will handle any changes that occur to a particular instance.
        /// </summary>
        /// <param name="e">The dependency object changed event args <see cref="DependencyPropertyChangedEventArgs"/></param>
        protected override void ThingSelectionChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!(e.NewValue is ClassKind))
            {
                return;
            }

            switch (e.NewValue)
            {
                case ClassKind.ElementBase:
                    this.GroupBoxThing.Header = "Element";
                    this.Mode = ClassKind.ElementBase;
                    break;
                case ClassKind.ParameterBase:
                    this.GroupBoxThing.Header = "Parameter";
                    this.Mode = ClassKind.ParameterBase;
                    break;
                default:
                    throw new ArgumentException("Invalid mode");
            }
        }

        public ThingSelector()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Bind view model when context is available
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThingUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataContext is ThingSelectorViewModel viewModel)
            {
                viewModel.BindData();
            }
        }
    }
}
