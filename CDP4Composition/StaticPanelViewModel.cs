// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StaticPanelViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using DevExpress.Xpf.Docking;
    using ReactiveUI;

    /// <summary>
    /// A base class from which all static docking panels derive
    /// </summary>
    /// <remarks>
    /// a static panel is a panel that is loaded at startup and exists only once
    /// </remarks>
    public abstract class StaticPanelViewModel : ReactiveObject, IMVVMDockingProperties
    {
        /// <summary>
        /// backing field for the <see cref="TargetName"/> property
        /// </summary>
        private string targetName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticPanelViewModel"/> class.
        /// </summary>
        /// <param name="targetName">
        /// The target name.
        /// </param>
        public StaticPanelViewModel(string targetName)
        {
            this.targetName = targetName;
        }

        /// <summary>
        /// Gets or sets the name of the Target that will host the Panel
        /// </summary>
        public string TargetName
        {
            get
            {
                return this.targetName;
            }

            set
            {
                this.targetName = value;
            }
        }
    }
}
