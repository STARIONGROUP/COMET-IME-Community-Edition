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
        /// Backing field for <see cref="Words"/> property
        /// </summary>
        private List<string> words = new List<string>();

        /// <summary>
        /// Gets or sets the list of words to be decorated.
        /// </summary>
        public List<string> Words
        {
            get { return this.words; }
            set { this.words = value; }
        }

        /// <summary>
        /// Gets or sets the value indicating whether this <see cref="StringDecoration"/> is case sensitive or not.
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// The list of <see cref="StringRange"/> indicating which substrings are adorned.
        /// </summary>
        /// <param name="Text">The string to match.</param>
        /// <returns>The list of ranges to decorate.</returns>
        public override List<StringRange> Ranges(string Text)
        {
            var pairs = new List<StringRange>();

            foreach (string word in this.words)
            {
                var rstring = @"(?i:\b" + word + @"\b)";

                if (IsCaseSensitive)
                {
                    rstring = @"\b" + word + @"\b)";
                }

                var rx = new Regex(rstring);
                var mc = rx.Matches(Text);

                pairs.AddRange(from Match m in mc select new StringRange(m.Index, m.Length));
            }
            return pairs;
        }
    }
}
