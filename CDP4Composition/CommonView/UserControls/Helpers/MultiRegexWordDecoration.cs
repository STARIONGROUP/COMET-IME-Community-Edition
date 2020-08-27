// -------------------------------------------------------------------------------------------------
// <copyright file="MultiRegexWordDecoration.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.UserControls
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The string decoration depencency object that handles the multiple words in a string.
    /// </summary>
    public class MultiRegexWordDecoration : StringDecoration
    {
        /// <summary>
        /// Gets or sets the list of words to be decorated.
        /// </summary>
        public List<string> Words { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the value indicating whether this <see cref="StringDecoration"/> is case sensitive or not.
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// The list of <see cref="StringRange"/> indicating which substrings are adorned.
        /// </summary>
        /// <param name="text">The string to match.</param>
        /// <returns>The list of ranges to decorate.</returns>
        public override List<StringRange> Ranges(string text)
        {
            var pairs = new List<StringRange>();

            foreach (var word in this.Words)
            {
                var rstring = @"(?i:\b" + word + @"\b)";

                if (this.IsCaseSensitive)
                {
                    rstring = @"\b" + word + @"\b)";
                }

                var rx = new Regex(rstring);
                var mc = rx.Matches(text);

                pairs.AddRange(from Match m in mc select new StringRange(m.Index, m.Length));
            }

            return pairs;
        }
    }
}
