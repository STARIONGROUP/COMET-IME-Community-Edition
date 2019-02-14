// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixConfigurationViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// An abstract view-model class for the configuration classes
    /// </summary>
    public abstract class MatrixConfigurationViewModelBase : ReactiveObject
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

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

            var modelsetup = iteration.IterationSetup?.Container as EngineeringModelSetup;
            if (modelsetup == null)
            {
                logger.Error("The engineering-model setup is null.");
            }
            else
            {
                var mrdl = modelsetup.RequiredRdl.FirstOrDefault();
                if (mrdl != null)
                {
                    rdls.Add(mrdl);
                    rdls.AddRange(mrdl.GetRequiredRdls());
                }
            }

            this.ReferenceDataLibraries = rdls;
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
    }
}
