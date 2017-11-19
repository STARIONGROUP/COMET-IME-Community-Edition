// -------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupListBoxItem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using CDP4Common.SiteDirectoryData;

    public class IterationSetupListBoxItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IterationSetupListBoxItem"/> class
        /// </summary>
        /// <param name="iterationSetup">The <see cref="IterationSetup"/> represented</param>
        public IterationSetupListBoxItem(IterationSetup iterationSetup)
        {
            this.IterationSetup = iterationSetup;
        }

        /// <summary>
        /// Gets the <see cref="IterationSetup"/>
        /// </summary>
        public IterationSetup IterationSetup { get; private set; }

        /// <summary>
        /// Gets the iteration number associated to this <see cref="IterationSetup"/>
        /// </summary>
        public int IterationNumber
        {
            get { return this.IterationSetup.IterationNumber; }
        }
    }
}