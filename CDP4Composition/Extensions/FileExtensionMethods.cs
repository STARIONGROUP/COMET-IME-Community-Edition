// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileExtensionMethods.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Extensions
{
    using System;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Operations;

    /// <summary>
    /// The purpose of these <see cref="FileExtensionMethods"/> is to add functionality to <see cref="File"/> instances
    /// </summary>
    public static class FileExtensionMethods
    {
        /// <summary>
        /// Moves a <see cref="File"/> to another <see cref="Folder"/> or <see cref="FileStore"/>
        /// A <see cref="File"/>'s parent folder is its latest <see cref="FileRevision"/>'s <see cref="FileRevision.ContainingFolder"/>.
        /// When <see cref="FileRevision.ContainingFolder"/> is null, that means that the <see cref="File"/> is located 
        /// Moving a file means that a new <see cref="FileRevision"/> has to be created that has the destination parent folder set as its <see cref="FileRevision.ContainingFolder"/>.
        /// </summary>
        /// <param name="file"><The <see cref="File"/></param>
        /// <param name="destinationFolder">The destination <see cref="Folder"/></param>
        /// <param name="creator">The <see cref="Participant"/> that executes the move action</param>
        /// <param name="session">The <see cref="ISession"/></param>
        public static async Task MoveFile(this File file, Folder destinationFolder, Participant creator, ISession session)
        {
            var transactionContext = TransactionContextResolver.ResolveContext(file);
            var containerClone = file.TopContainer.Clone(false);
            var containerTransaction = new ThingTransaction(transactionContext, containerClone);

            var fileClone = file.Clone(false);

            var newFileRevision = file.CurrentFileRevision.CopyToNew(creator);
            fileClone.FileRevision.Add(newFileRevision);

            newFileRevision.ContainingFolder = destinationFolder;
            containerTransaction.CreateOrUpdate(fileClone);
            containerTransaction.CreateOrUpdate(newFileRevision);

            try
            {
                var operationContainer = containerTransaction.FinalizeTransaction();
                await session.Write(operationContainer);
            }
            catch (Exception ex)
            {
                throw new Exception($"Moving file failed: {ex.Message}");
            }
        }
    }
}