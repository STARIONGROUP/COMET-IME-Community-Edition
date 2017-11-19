// -------------------------------------------------------------------------------------------------
// <copyright file="FileStoreRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------
namespace CDP4EngineeringModel.ViewModels.FileStoreBrowsers
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Interface that FileStoreRowViewModels should implement.
    /// </summary>
    public interface IFileStoreRow
    {
        /// <summary>
        /// Update the <see cref="Folder"/> rows
        /// </summary>
        void UpdateFolderRowPosition(Folder updatedFolder);
        /// <summary>
        /// Update the <see cref="File"/> rows
        /// </summary>
        void UpdateFileRowPosition(File file, FileRevision fileRevision);
    }
}
