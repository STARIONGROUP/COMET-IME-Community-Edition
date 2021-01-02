// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationTrackParameterDetailViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed   
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------------------------

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
