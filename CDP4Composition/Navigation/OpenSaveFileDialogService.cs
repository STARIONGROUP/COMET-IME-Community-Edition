// -------------------------------------------------------------------------------------------------
// <copyright file="OpenSaveFileDialogService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System.ComponentModel.Composition;
    using System.IO;
    using Microsoft.Win32;

    /// <summary>
    /// The service that handle open and save file requests
    /// </summary>
    [Export(typeof(IOpenSaveFileDialogService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OpenSaveFileDialogService : IOpenSaveFileDialogService
    {
        /// <summary>
        /// Gets the list of file paths to open.
        /// </summary>
        /// <param name="checkFileExists">if true, Check whether the file exists</param>
        /// <param name="checkPathExists">if true check whether the path exists</param>
        /// <param name="multiSelect">if true, multiple files may be selected</param>
        /// <param name="filter">the file filter</param>
        /// <param name="extension">the default extension of the file(s) to open</param>
        /// <param name="initialPath">the initial path</param>
        /// <param name="filterIndex">the index of the filter currently selected</param>
        /// <returns>
        /// An array of file paths to open or null if the operation of the dialog was cancelled.
        /// </returns>
        public string[] GetOpenFileDialog(bool checkFileExists, bool checkPathExists, bool multiSelect, string filter, string extension, string initialPath, int filterIndex)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = checkFileExists,
                CheckPathExists = checkPathExists,
                Filter = filter,
                DefaultExt = extension,
                AddExtension = true,
                Multiselect = multiSelect,
                FilterIndex = filterIndex
            };

            // setting the default directory if already been chosen
            if (!string.IsNullOrEmpty(initialPath))
            {
                var fileinfo = new FileInfo(initialPath);
                if (fileinfo.Directory != null)
                {
                    dialog.InitialDirectory = fileinfo.DirectoryName;
                }
            }

            var showdialog = dialog.ShowDialog();
            var result = showdialog.HasValue && showdialog.Value;

            return (result) ? dialog.FileNames : null;
        }

        /// <summary>
        /// Gets the location of the file to be saved
        /// </summary>
        /// <param name="defaultFilename">the default filename</param>
        /// <param name="extension">the extension of the file to create</param>
        /// <param name="filter">the filter for the dialog</param>
        /// <param name="initialPath">the initial path for the dialog</param>
        /// <param name="filterIndex">the index of the filter currently selected</param>
        /// <returns>the path of the file to create or null if the operation was cancelled.</returns>
        public string GetSaveFileDialog(string defaultFilename, string extension, string filter, string initialPath, int filterIndex)
        {
            // Configure save file dialog box
            var dlg = new SaveFileDialog
            {
                FileName = defaultFilename,
                DefaultExt = extension,
                Filter = filter
            };

            if (!string.IsNullOrEmpty(dlg.Filter))
            {
                dlg.FilterIndex = filterIndex;
            }

            // setting the default directory if already been chosen
            if (!string.IsNullOrEmpty(initialPath))
            {
                var fileinfo = new FileInfo(initialPath);
                if (fileinfo.Directory != null)
                {
                    dlg.InitialDirectory = fileinfo.DirectoryName;
                }
            }

            // Show save file dialog box
            var showDialog = dlg.ShowDialog();
            var result = showDialog != null && (bool)showDialog;

            // Process save file dialog box results
            return (result) ? dlg.FileName : null;
        }
    }
}