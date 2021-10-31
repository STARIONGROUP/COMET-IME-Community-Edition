// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixConfigurationViewModelBase.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using NLog;

    using ReactiveUI;

    /// <summary>
    /// An abstract view-model class for the configuration classes
    /// </summary>
    public abstract class MatrixConfigurationViewModelBase : ReactiveObject, IDisposable
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// a value indicating whether the instance is disposed
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixConfigurationViewModelBase"/> class
        /// </summary>
        /// <param name="session">The current <see cref="ISession"/></param>
        /// <param name="iteration">The current <see cref="Iteration"/></param>
        /// <param name="onUpdateAction">The action to perform on update</param>
        /// <param name="settings">The module settings</param>
        protected MatrixConfigurationViewModelBase(ISession session, Iteration iteration, Action onUpdateAction, RelationshipMatrixPluginSettings settings)
        {
            this.Iteration = iteration;
            this.OnUpdateAction = onUpdateAction;
            this.Session = session;
            this.PluginSetting = settings;

            var rdls = new List<ReferenceDataLibrary>();

            var engineeringModelSetup = iteration.IterationSetup?.Container as EngineeringModelSetup;
            if (engineeringModelSetup == null)
            {
                logger.Error("The engineering-model setup is null.");
            }
            else
            {
                var mrdl = engineeringModelSetup.RequiredRdl.FirstOrDefault();

                if (mrdl != null)
                {
                    rdls.Add(mrdl);
                    rdls.AddRange(mrdl.GetRequiredRdls());
                }
            }

            this.ReferenceDataLibraries = rdls;
            this.Disposables = new List<IDisposable>();
        }

        /// <summary>
        /// Gets the <see cref="RelationshipMatrixPluginSettings"/>
        /// </summary>
        protected RelationshipMatrixPluginSettings PluginSetting { get; }

        /// <summary>
        /// Gets the current <see cref="ISession"/>
        /// </summary>
        protected ISession Session { get; }

        /// <summary>
        /// Gets the <see cref="ReferenceDataLibrary"/>s within the context of this <see cref="Iteration"/>
        /// </summary>
        protected IReadOnlyList<ReferenceDataLibrary> ReferenceDataLibraries { get; }

        /// <summary>
        /// Gets the action that shall be performed when an instance of <see cref="MatrixConfigurationViewModelBase"/> is updated
        /// </summary>
        protected Action OnUpdateAction { get; }

        /// <summary>
        /// Gets the current <see cref="Iteration"/>
        /// </summary>
        protected Iteration Iteration { get; }

        /// <summary>
        /// Gets the list of <see cref="IDisposable"/> objects that are referenced by this class
        /// </summary>
        protected List<IDisposable> Disposables { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // Clear all property values that maybe have been set
                    // when the class was instantiated
                    if (this.Disposables != null)
                    {
                        foreach (var disposable in this.Disposables)
                        {
                            disposable.Dispose();
                        }
                    }
                    else
                    {
                        logger.Trace("The Disposables collection of the {0} is null", this.GetType().Name);
                    }
                }

                // Indicate that the instance has been disposed.
                this.isDisposed = true;
            }
        }
    }
}
