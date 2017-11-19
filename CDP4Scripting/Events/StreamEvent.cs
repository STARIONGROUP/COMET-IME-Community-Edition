// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Events
{
    using System;
    using System.IO;

    /// <summary>
    /// The arguments to pass to the event handler when the <see cref="MemoryStream"/> changes. 
    /// </summary>
    /// <remarks>
    /// source : http://stackoverflow.com/questions/3055002/how-can-i-redirect-the-stdout-of-ironpython-in-c
    /// </remarks>
    public class StreamEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The value we want to pass in argument.
        /// </summary>
        public T Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamEventArgs{T}"/> class.
        /// </summary>
        /// <param name="value">The value we want to pass in argument.</param>
        public StreamEventArgs(T value)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// StreamWriter that rises events when something is written on the stream.
    /// </summary>
    /// <remarks>
    /// This class is used to print the output of a Python script during the execution.
    /// </remarks>
    public class EventRaisingStreamWriter : StreamWriter
    {
        /// <summary>
        /// Event handler when something is written on the stream.
        /// </summary>
        public event EventHandler<StreamEventArgs<string>> StringWritten;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventRaisingStreamWriter"/> class.
        /// </summary>
        /// <param name="s">The stream to write to.</param>
        public EventRaisingStreamWriter(Stream s) : base(s)
        {
        }

        /// <summary>
        /// The event handler is configured to attribute this object to the sender parameter, and the text which has been written in the memory stream in the arguments parameter.
        /// </summary>
        /// <param name="txtWritten">The text written in the memory stream.</param>
        private void LaunchEvent(string txtWritten)
        {
            this.StringWritten?.Invoke(this, new StreamEventArgs<string>(txtWritten));
        }

        /// <summary>
        /// Overrides the <see cref="StreamWriter.Write(string)"/> to write the value passed in parameter to a <see cref="Stream"/> and launch the event. 
        /// </summary>
        /// <param name="value">The string to write to the stream.</param>
        public override void Write(string value)
        {
            base.Write(value);
            this.LaunchEvent(value);
        }

        /// <summary>
        /// Overrides the <see cref="StreamWriter.Write(bool)"/> to write the value passed in parameter to a <see cref="Stream"/> and launch the event. 
        /// </summary>
        /// <param name="value">The boolean to write to the stream.</param>
        public override void Write(bool value)
        {
            base.Write(value);
            LaunchEvent(value.ToString());
        }
    }
}