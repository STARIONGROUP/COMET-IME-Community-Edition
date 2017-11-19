// -------------------------------------------------------------------------------------------------
// <copyright file="LogDetailsRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using ReactiveUI;
    using NLog;

    /// <summary>
    /// The row-view-model representing a combination of property/content of a <see cref="LogEventInfo"/>
    /// </summary>
    public class LogDetailsRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Property"/>
        /// </summary>
        private string property;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Gets or Sets the name of the property to display
        /// </summary>
        public string Property
        {
            get { return this.property; }
            set { this.RaiseAndSetIfChanged(ref this.property, value); }
        }

        /// <summary>
        /// Gets or Sets the content associated to this <see cref="Property"/> to display
        /// </summary>
        public string Content
        {
            get { return this.content; }
            set { this.RaiseAndSetIfChanged(ref this.content, value); }
        }
    }
}
