// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowserHeader.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for <see cref="BrowserHeader"/> XAML
    /// </summary>
    public partial class BrowserHeader : UserControl
    {
        /// <summary>
        /// The dependency property whether the header sould display the option title and its value 
        /// </summary>
        public static readonly DependencyProperty IsOptionDependantProperty = DependencyProperty.Register("IsOptionDependant", typeof(bool), typeof(BrowserHeader), new PropertyMetadata(false));
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserHeader"/> class.
        /// </summary>
        public BrowserHeader()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the <see cref="IsOptionDependant"/> dependency property.
        /// </summary>
        public bool IsOptionDependant
        {
            get => (bool)this.GetValue(IsOptionDependantProperty);
            set => this.SetValue(IsOptionDependantProperty, value);
        }
    }
}
