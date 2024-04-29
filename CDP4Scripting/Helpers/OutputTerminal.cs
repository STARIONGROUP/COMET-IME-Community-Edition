﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputTerminal.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4Scripting.Helpers
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    /// <summary>
    /// The output terminal of a script panel.
    /// </summary>
    public class OutputTerminal : RichTextBox
    {
        /// <summary>
        /// The message to display on the ouput console when a new panel is created. 
        /// </summary>
        private const string InitMessage = @"
    --------------------------------------------------------------------
                        CDP4-COMET Scripting Engine					    
						
    Start using it writing 'Command.Help()' in the Input console
	
    More information can be found on the CDP4-COMET documentation website
    http://cdp4docs.rheagroup.com/index.php

    --------------------------------------------------------------------
";

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputTerminal"/> class.
        /// </summary>
        public OutputTerminal()
        {
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            this.IsReadOnly = true;
            this.BorderThickness = new Thickness(0);

            // Set the paragraph margin to 0
            var paragraphStyle = new Style { TargetType = typeof(Paragraph) };
            paragraphStyle.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));
            this.Resources.Add(typeof(Paragraph), paragraphStyle);

            this.AppendText(InitMessage);
        }
    }
}
