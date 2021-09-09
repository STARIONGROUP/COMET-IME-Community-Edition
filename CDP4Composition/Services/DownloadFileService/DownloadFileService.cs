// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DownloadFileService.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Extensions;

    using Microsoft.Practices.ServiceLocation;

    using NLog;

    /// <summary>
    /// The purpose of the <see cref="DownloadFileService"/> is to download a file, for example from a <see cref="FileStore"/>
    /// </summary>
    [Export(typeof(IDownloadFileService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DownloadFileService : IDownloadFileService
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        protected static Logger Logger = LogManager.GetLogger(typeof(DownloadFileService).FullName);

        /// <summary>
        /// The (injected) <see cref="DevExpress.Mvvm.IMessageBoxService"/>
        /// </summary>
        private readonly IMessageBoxService messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

        /// <summary>
        /// Executes a file download for a <see cref="File"/>
        /// </summary>
        /// <param name="downloadFileViewModel">The <see cref="IDownloadFileViewModel"/></param>
        /// <param name="file">The <see cref="File"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task ExecuteDownloadFile(IDownloadFileViewModel downloadFileViewModel, File file)
        {
            await this.ExecuteDownloadFile(downloadFileViewModel, file.FileRevision.OrderByDescending(x => x.CreatedOn).FirstOrDefault());
        }

        /// <summary>
        /// Executes a file download for a <see cref="FileRevision"/>
        /// </summary>
        /// <param name="downloadFileViewModel">The <see cref="IDownloadFileViewModel"/></param>
        /// <param name="fileRevision">The <see cref="FileRevision"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task ExecuteDownloadFile(IDownloadFileViewModel downloadFileViewModel, FileRevision fileRevision)
        {
            if (fileRevision != null)
            {
                downloadFileViewModel.LoadingMessage = "Downloading";
                var cancelEnabledInterval = Observable.Interval(TimeSpan.FromMilliseconds(250));
                var subscription = cancelEnabledInterval.Subscribe(_ => { downloadFileViewModel.IsCancelButtonVisible = downloadFileViewModel.Session.CanCancel(); });

                downloadFileViewModel.IsBusy = true;

                try
                {
                    await fileRevision.DownloadFile(downloadFileViewModel.Session);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException || ex.InnerException is TaskCanceledException)
                    {
                        this.messageBoxService.Show($"Downloading {fileRevision.Name} was cancelled", "Download cancelled", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        Logger.Error(ex, $"Downloading {fileRevision.Name} caused an error");
                        this.messageBoxService.Show($"Downloading {fileRevision.Name} caused an error: {ex.Message}", "Download failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                finally
                {
                    subscription.Dispose();
                    downloadFileViewModel.LoadingMessage = "";
                    downloadFileViewModel.IsCancelButtonVisible = false;
                    downloadFileViewModel.IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Executes a file download for an <see cref="Attachment"/>
        /// </summary>
        /// <param name="downloadFileViewModel">The <see cref="IDownloadFileViewModel"/></param>
        /// <param name="attachment">The <see cref="Attachment"/></param>
        /// <returns>An awaitable <see cref="Task"/></returns>
        public async Task ExecuteDownloadFile(IDownloadFileViewModel downloadFileViewModel, Attachment attachment)
        {
            if (attachment != null)
            {
                downloadFileViewModel.LoadingMessage = "Downloading";
                var cancelEnabledInterval = Observable.Interval(TimeSpan.FromMilliseconds(250));
                var subscription = cancelEnabledInterval.Subscribe(_ => { downloadFileViewModel.IsCancelButtonVisible = downloadFileViewModel.Session.CanCancel(); });

                downloadFileViewModel.IsBusy = true;

                try
                {
                    await attachment.DownloadFile(downloadFileViewModel.Session);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException || ex.InnerException is TaskCanceledException)
                    {
                        this.messageBoxService.Show($"Downloading {attachment.FileName} was cancelled", "Download cancelled", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        Logger.Error(ex, $"Downloading {attachment.FileName} caused an error");
                        this.messageBoxService.Show($"Downloading {attachment.FileName} caused an error: {ex.Message}", "Download failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                finally
                {
                    subscription.Dispose();
                    downloadFileViewModel.LoadingMessage = "";
                    downloadFileViewModel.IsCancelButtonVisible = false;
                    downloadFileViewModel.IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Cancels a file download
        /// </summary>
        /// <param name="downloadFileViewModel">The <see cref="IDownloadFileViewModel"/></param>
        public void CancelDownloadFile(IDownloadFileViewModel downloadFileViewModel)
        {
            if (downloadFileViewModel.Session.CanCancel())
            {
                downloadFileViewModel.Session.Cancel();
            }
        }
    }
}
