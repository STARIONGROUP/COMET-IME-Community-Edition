// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventingMemoryStream .cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Events
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// A MemoryStream that raises an event when new data is added
    /// </summary>
    public class EventingMemoryStream : MemoryStream
    {
        /// <summary>
        /// EventHandler that is invoked when text is written to the Stream
        /// </summary>
        public event EventHandler<string> OnNewText;

        /// <summary>Writes a block of bytes to the current stream using data read from a buffer.</summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.
        /// -or-
        /// The current position is closer than <paramref name="count" /> bytes to the end of the stream, and the capacity cannot be modified.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> are negative.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.OnNewText?.Invoke(this, Encoding.Default.GetString(buffer, offset, count));

            base.Write(buffer, offset, count);
        }

        /// <summary>Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.</summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous write operation.</exception>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            this.OnNewText?.Invoke(this, Encoding.Default.GetString(buffer, offset, count));

            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <summary>Writes a byte to the current stream at the current position.</summary>
        /// <param name="value">The byte to write.</param>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing. For additional information see <see cref="P:System.IO.Stream.CanWrite" />.
        /// -or-
        /// The current position is at the end of the stream, and the capacity cannot be modified.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The current stream is closed.</exception>
        public override void WriteByte(byte value)
        {
            this.OnNewText?.Invoke(this, value.ToString());

            base.WriteByte(value);
        }

        /// <summary>Begins an asynchronous write operation. (Consider using <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)" /> instead.)</summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An <see langword="IAsyncResult" /> that represents the asynchronous write, which could still be pending.</returns>
        /// <exception cref="T:System.IO.IOException">Attempted an asynchronous write past the end of the stream, or a disk error occurs.</exception>
        /// <exception cref="T:System.ArgumentException">One or more of the arguments is invalid.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        /// <exception cref="T:System.NotSupportedException">The current <see langword="Stream" /> implementation does not support the write operation.</exception>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.OnNewText?.Invoke(this, Encoding.Default.GetString(buffer, offset, count));

            return base.BeginWrite(buffer, offset, count, callback, state);
        }
    }
}
