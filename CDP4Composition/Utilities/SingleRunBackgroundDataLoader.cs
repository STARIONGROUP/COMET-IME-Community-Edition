// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BackgroundBrowserDataLoader.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Jaime Bernar
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Utilities
{
    using System;
    using System.ComponentModel;

    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;

    /// <summary>
    /// The main purpose of this class is to be able to create an initial loading mechanism for browsers on a <see cref="BackgroundWorker"/>
    /// Tests have approved this to be the fastest way to load browsers that contain a lot of rows.
    /// </summary>
    /// <typeparam name="T">
    /// The generic type that corresponds with the type of the constructor parameter <see cref="BrowserViewModelBase{T}"/>
    /// The reason that we use the <see cref="BrowserViewModelBase{T}"/> here and not a higher level interface is to scopy this
    /// class only to be used with a browser.
    /// </typeparam>
    public class SingleRunBackgroundDataLoader<T> where T : Thing
    {
        /// <summary>
        /// The Actual <see cref="BackgroundWorker"/>
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// Create s new instance of the <see cref="SingleRunBackgroundDataLoader{T}"/> class
        /// </summary>
        /// <param name="owningBrowser">The owning <see cref="BrowserViewModelBase{T}"/></param>
        /// <param name="doWork">The action that will be performed to execute the initial data loading</param>
        /// <param name="onCompleted">The action that will be performed when data loading is finished</param>
        public SingleRunBackgroundDataLoader(BrowserViewModelBase<T> owningBrowser, Action<DoWorkEventArgs> doWork, Action<RunWorkerCompletedEventArgs> onCompleted)
        {
            this.worker = new BackgroundWorker();
            this.worker.DoWork += (sender, args) => doWork.Invoke(args);

            this.worker.RunWorkerCompleted += (sender, args) =>
            {
                onCompleted.Invoke(args);
                this.worker.Dispose();
                this.worker = null;

                // Forces the disposal of the SingleBackgroundWorker and also implemnted as a triggers in Unit tests that the BackgroundWorker has finished
                owningBrowser.SingleRunBackgroundWorker = null;
            };
        }

        /// <summary>
        /// Executes the <see cref="BackgroundWorker"/>
        /// </summary>
        public void RunWorkerAsync()
        {
            this.worker?.RunWorkerAsync();
        }
    }
}
