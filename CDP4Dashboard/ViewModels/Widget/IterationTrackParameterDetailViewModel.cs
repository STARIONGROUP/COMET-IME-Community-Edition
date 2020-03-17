// -----------------------------------------------------------------------
// <copyright file="IterationTrackParameterDetailViewModel.cs" company="RHEA">
// Copyright (c) 2020 RHEA Group. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace CDP4Dashboard.ViewModels.Widget
{
    using System;

    using CDP4Dashboard.ViewModels.Widget.Base;

    using NLog;

    /// <summary>
    /// The iteration track parameter details view model.
    /// </summary>
    public class IterationTrackParameterDetailViewModel : WidgetDetailsBase
    {
        /// <summary>
        /// The Logger.
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The iteration track parameter.
        /// </summary>
        private readonly IterationTrackParameter iterationTrackParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationTrackParameterDetailViewModel"/> class.
        /// </summary>
        /// <param name="iterationTrackParameter">
        /// The iteration track parameter
        /// </param>
        public IterationTrackParameterDetailViewModel(IterationTrackParameter iterationTrackParameter)
        {
            this.iterationTrackParameter = iterationTrackParameter;
            this.Title = iterationTrackParameter.ControlTitle;
        }

        /// <summary>
        /// Edit Object
        /// </summary>
        protected override void OnOk()
        {
            try
            {
                this.iterationTrackParameter.ControlTitle = this.Title;
                base.OnOk();
            }
            catch (Exception ex)
            {
                var msg = $"Failed to edit widget. Internal error: {ex.Message}";
                logger.Error(msg);
                throw;
            }
        }

        /// <summary>
        /// Implements the Dispose method
        /// </summary>
        public override void Dispose()
        {
        }
    }
}
