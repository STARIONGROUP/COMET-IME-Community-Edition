// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AboutViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using System.Reflection;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using Views;

    /// <summary>
    /// The view-model for the <see cref="About"/> view
    /// </summary>
    [DialogViewModelExport("About", "The About window")]
    public class AboutViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutViewModel"/> class.
        /// Used by MEF.
        /// </summary>
        public AboutViewModel()
        {
        }

        /// <summary>
        /// Gets the assembly version information to display.
        /// </summary>
        public string Version => $"Version: {Assembly.GetEntryAssembly().GetName().Version}";
    }
}