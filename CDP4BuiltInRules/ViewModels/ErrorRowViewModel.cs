// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Errors.ViewModels
{
    using System;
    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using NLog;

    /// <summary>
    /// The purpose of <see cref="ErrorRowViewModel"/> is to represent rows of Errors in the error browser
    /// </summary>
    public class ErrorRowViewModel : RowViewModelBase<Thing>
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorRowViewModel"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <param name="session">The session.</param>
        /// <param name="containerViewModel">The container browser view model.</param>
        public ErrorRowViewModel(Thing thing, string content, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(thing, session, containerViewModel)
        {
            this.ContainerThingClassKind = thing.ClassKind.ToString();
            this.Content = content;

            try
            {
                var dto = thing.ToDto();
                var dtoRoute = dto.Route;
                var uriBuilder = new UriBuilder(thing.IDalUri) { Path = dtoRoute };
                this.Path = uriBuilder.ToString();
            }
            catch (ContainmentException ex)
            {
                this.Path = ex.Message;
                logger.Warn(ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="Thing"/> that contains the error.
        /// </summary>
        public string ContainerThingClassKind { get; private set; }

        /// <summary>
        /// Gets the human readable content of the Error.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the human readable content of the Error.
        /// </summary>
        public string Path { get; private set; }
    }
}
