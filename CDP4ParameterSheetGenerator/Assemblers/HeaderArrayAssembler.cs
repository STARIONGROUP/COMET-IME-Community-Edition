// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeaderArrayAssembler.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Assemblers
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The purpose of the <see cref="HeaderArrayAssembler"/> is to create and populate arrays to
    /// write as header information to the Parameter Sheet
    /// </summary>
    public class HeaderArrayAssembler
    {
        /// <summary>
        /// The <see cref="Iteration"/> for which the parameter sheet is generated
        /// </summary>
        private readonly Iteration iteration;

        /// <summary>
        /// The <see cref="EngineeringModel"/> for which the parameter sheet is generated
        /// </summary>
        private readonly EngineeringModel engineeringModel;

        /// <summary>
        /// The <see cref="Participant"/> for which the parameter sheet is generated.
        /// </summary>
        private readonly Participant participant;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderArrayAssembler"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the parameter sheet is generated.
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> for which the parameter sheet is generated.
        /// </param>
        public HeaderArrayAssembler(Iteration iteration, Participant participant)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException("iteration", "The Iteration may not be null");
            }

            if (participant == null)
            {
                throw new ArgumentNullException("participant", "The Participant may not be null");
            }

            this.engineeringModel = (EngineeringModel)iteration.Container;
            this.iteration = iteration;
            this.participant = participant;

            this.InitializeArrays();
            this.PopulateHeaderArray();
            this.PopulateHeaderLockArray();
            this.PopulateHeaderFormatArray();
        }

        /// <summary>
        /// Gets the array that contains the content of the header for the parameter sheet
        /// </summary>
        public object[,] HeaderArray { get; private set; }

        /// <summary>
        /// Gets the array that contains the lock settings of the header for the parameter sheet
        /// </summary>
        public object[,] LockArray { get; private set; }

        /// <summary>
        /// Gets the array that contains the formatting settings of the header for the parameter sheet
        /// </summary>
        public object[,] FormatArray { get; private set; }

        /// <summary>
        /// Initialize the arrays that will contain data that is to be written to the Parameter sheet 
        /// </summary>
        private void InitializeArrays()
        {
            this.HeaderArray = new object[6, 13];
            this.LockArray = new object[6, 13];
            this.FormatArray = new object[6, 13];
        }

        /// <summary>
        /// Populates the content of the header array
        /// </summary>
        private void PopulateHeaderArray()
        {
            this.HeaderArray[0, 0] = "Engineering Model:";
            this.HeaderArray[1, 0] = "Iteration number:";
            this.HeaderArray[2, 0] = "Study Phase:";
            this.HeaderArray[3, 0] = "Domain:";
            this.HeaderArray[4, 0] = "User:";
            this.HeaderArray[5, 0] = "Rebuild Date:";
            
            this.HeaderArray[0, 2] = this.engineeringModel.EngineeringModelSetup.Name;
            this.HeaderArray[1, 2] = this.iteration.IterationSetup.IterationNumber;
            this.HeaderArray[2, 2] = this.engineeringModel.EngineeringModelSetup.StudyPhase.ToString();
            this.HeaderArray[3, 2] = this.participant.SelectedDomain.Name;
            this.HeaderArray[4, 2] = string.Format("{0} {1}", this.participant.Person.GivenName, this.participant.Person.Surname);
            this.HeaderArray[5, 2] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// populates the lock array with values
        /// </summary>
        private void PopulateHeaderLockArray()
        {
            for (int i = this.LockArray.GetLowerBound(0); i < this.LockArray.GetUpperBound(0); i++)
            {
                for (int j = this.LockArray.GetLowerBound(1); j < this.LockArray.GetUpperBound(1); j++)
                {
                    this.LockArray[i, j] = true;   
                }
            }
        }

        /// <summary>
        /// populates the format array with values
        /// </summary>
        private void PopulateHeaderFormatArray()
        {
            for (int i = this.FormatArray.GetLowerBound(0); i < this.FormatArray.GetUpperBound(0); i++)
            {
                for (int j = this.FormatArray.GetLowerBound(1); j < this.FormatArray.GetUpperBound(1); j++)
                {
                    this.FormatArray[i, j] = "@";
                }
            }
        }
    }
}
