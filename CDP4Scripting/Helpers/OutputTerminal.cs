// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OutputTerminal.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
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
                        CDP4 Scripting Engine					    
						
    Start using it writing 'Command.Help()' in the Input console
	
    More information can be found on the CDP4 documentation website
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