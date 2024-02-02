// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttachmentExtensionMethods.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
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
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;

    using CDP4Dal;

    using CommonServiceLocator;

    /// <summary>
    /// The purpose of these <see cref="AttachmentExtensionMethods"/> is to add functionality to <see cref="Attachment"/> instances
    /// </summary>
    public static class AttachmentExtensionMethods 
    {
        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private static readonly IOpenSaveFileDialogService FileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

        /// <summary>
        /// Download a physical file from a <see cref="Attachment"/> to the users' computer
        /// </summary>
        /// <param name="attachment">The <see cref="Attachment"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <returns>An awaitable task</returns>
        public static async Task DownloadFile(this Attachment attachment, ISession session)
        {
            var fileName = attachment.FileName;
            var extension = attachment.FileType.LastOrDefault()?.Extension;
            var filter = string.IsNullOrWhiteSpace(extension) ? "All files (*.*)|*.*" : $"{extension.Replace(".", "")} files|*{extension}";

            var destinationPath = FileDialogService.GetSaveFileDialog(fileName, extension, filter, string.Empty, 1);

            if (!string.IsNullOrWhiteSpace(destinationPath))
            {
                var fileContent = await session.ReadFile(attachment);

                if (fileContent != null)
                {
                    System.IO.File.WriteAllBytes(destinationPath, fileContent);
                }
            }
        }
    }
}
