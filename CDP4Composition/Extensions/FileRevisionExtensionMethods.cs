// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionExtensionMethods.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Extensions
{
    using System;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using CDP4Dal;

    using CommonServiceLocator;

    /// <summary>
    /// The purpose of these <see cref="FileRevisionExtensionMethods"/> is to add functionality to <see cref="FileRevision"/> instances
    /// </summary>
    public static class FileRevisionExtensionMethods 
    {
        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private static readonly IOpenSaveFileDialogService FileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

        /// <summary>
        /// Download a physical file from a <see cref="FileRevision"/> to the users' computer
        /// </summary>
        /// <param name="fileRevision">The <see cref="FileRevision"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <returns>An awaitable task</returns>
        public static async Task DownloadFile(this FileRevision fileRevision, ISession session)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(fileRevision.Path);
            var extension = System.IO.Path.GetExtension(fileRevision.Path);
            var filter = string.IsNullOrWhiteSpace(extension) ? "All files (*.*)|*.*" : $"{extension.Replace(".", "")} files|*{extension}";

            var destinationPath = FileDialogService.GetSaveFileDialog(fileName, extension, filter, string.Empty, 1);

            if (!string.IsNullOrWhiteSpace(destinationPath))
            {
                var fileContent = await session.ReadFile(fileRevision);

                if (fileContent != null)
                {
                    System.IO.File.WriteAllBytes(destinationPath, fileContent);
                }
            }
        }

        /// <summary>
        /// Creates a copy of this <see cref="FileRevision"/> that can be used to add as a new <see cref="FileRevision"/>
        /// </summary>
        /// <param name="fileRevision">The <see cref="FileRevision"/> to be copied</param>
        /// <param name="creator">The <see cref="Participant"/> that created this copy</param>
        /// <returns>The copy <see cref="FileRevision"/></returns>
        public static FileRevision CopyToNew(this FileRevision fileRevision, Participant creator)
        {
            var newFileRevision = new FileRevision
            {
                Name = fileRevision.Name, 
                CreatedOn = DateTime.UtcNow,
                ContainingFolder = fileRevision.ContainingFolder,
                ContentHash = fileRevision.ContentHash,
                Creator = creator
            };

            newFileRevision.FileType.AddRange(fileRevision.FileType);

            return newFileRevision;
        }
    }
}