// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbstractHeaderArrayAssembler.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Assemblers
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;

    /// <summary>
    /// Abstract super class from which all header array assemblers derive
    /// </summary>
    public abstract class AbstractHeaderArrayAssembler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractHeaderArrayAssembler"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the parameter sheet is generated.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the parameter sheet is generated.
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> for which the parameter sheet is generated.
        /// </param>
        protected AbstractHeaderArrayAssembler(ISession session, Iteration iteration, Participant participant)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session), "The Session may not be null");
            }

            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration), "The Iteration may not be null");
            }

            if (participant == null)
            {
                throw new ArgumentNullException(nameof(participant), "The Participant may not be null");
            }

            this.Session = session;
            this.EngineeringModel = (EngineeringModel)iteration.Container;
            this.Iteration = iteration;
            this.Participant = participant;

            this.InitializeArrays();
        }

        /// <summary>
        /// Initialize the arrays that will contain data that is to be written to the Parameter sheet 
        /// </summary>
        public abstract void InitializeArrays();

        /// <summary>
        /// Populates the content of the header array
        /// </summary>
        public abstract void PopulateHeaderArray();

        /// <summary>
        /// Populates the lock array with values
        /// </summary>
        public virtual void PopulateHeaderLockArray()
        {
            for (var i = this.LockArray.GetLowerBound(0); i <= this.LockArray.GetUpperBound(0); i++)
            {
                for (var j = this.LockArray.GetLowerBound(1); j <= this.LockArray.GetUpperBound(1); j++)
                {
                    this.LockArray[i, j] = true;
                }
            }
        }

        /// <summary>
        /// Populates the format array with values
        /// </summary>
        public virtual void PopulateHeaderFormatArray()
        {
            for (var i = this.FormatArray.GetLowerBound(0); i <= this.FormatArray.GetUpperBound(0); i++)
            {
                for (var j = this.FormatArray.GetLowerBound(1); j <= this.FormatArray.GetUpperBound(1); j++)
                {
                    this.FormatArray[i, j] = "@";
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ISession"/> for which the header is assembled
        /// </summary>
        protected ISession Session { get; private set; }

        /// <summary>
        /// Gets the <see cref="EngineeringModel"/> for which the header is assembled
        /// </summary>
        protected EngineeringModel EngineeringModel { get; private set; }

        /// <summary>
        /// Gets the <see cref="Iteration"/> for which the header is assembled
        /// </summary>
        protected Iteration Iteration { get; private set; }

        /// <summary>
        /// Gets the <see cref="Participant"/> for which the header is assembled
        /// </summary>
        protected Participant Participant { get; private set; }

        /// <summary>
        /// Gets or sets the array that contains the content of the header for the parameter sheet
        /// </summary>
        public object[,] HeaderArray { get; protected set; }

        /// <summary>
        /// Gets or sets the array that contains the lock settings of the header for the parameter sheet
        /// </summary>
        public object[,] LockArray { get; protected set; }

        /// <summary>
        /// Gets or sets the array that contains the formatting settings of the header for the parameter sheet
        /// </summary>
        public object[,] FormatArray { get; protected set; }
    }
}