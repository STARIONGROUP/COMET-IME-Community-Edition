// -------------------------------------------------------------------------------------------------
// <copyright file="MemoryEventTarget.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Log
{
    using System;
    using System.ComponentModel.Composition;

    using NLog;
    using NLog.Targets;

    /// <summary>
    /// Handles Log event thoughout the CDP4 Application
    /// Code from http://dotnetsolutionsbytomi.blogspot.nl/2011/06/creating-awesome-logging-control-with.html
    /// </summary>
    public class MemoryEventTarget : Target
    {
        /// <summary>
        /// The <see cref="LogEventInfo"/> event
        /// </summary>
        public event Action<LogEventInfo> EventReceived;

        /// <summary>
        /// Notifies listeners about new event
        /// </summary>
        /// <param name="logEvent">The Log Event</param>
        protected override void Write(LogEventInfo logEvent)
        {
            if (this.EventReceived != null)
            {
                this.EventReceived(logEvent);
            }
        }
    }
}