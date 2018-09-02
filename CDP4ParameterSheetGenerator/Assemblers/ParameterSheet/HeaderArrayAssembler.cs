// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeaderArrayAssembler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4ParameterSheetGenerator.Assemblers;

    /// <summary>
    /// The purpose of the <see cref="HeaderArrayAssembler"/> is to create and populate arrays to
    /// write as header information to the Parameter Sheet
    /// </summary>
    public class HeaderArrayAssembler : AbstractHeaderArrayAssembler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderArrayAssembler"/> class.
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
        public HeaderArrayAssembler(ISession session, Iteration iteration, Participant participant)
            : base(session, iteration, participant)
        {
            this.PopulateHeaderArray();
            this.PopulateHeaderLockArray();
            this.PopulateHeaderFormatArray();
        }

        /// <summary>
        /// Initialize the arrays that will contain data that is to be written to the Parameter sheet 
        /// </summary>
        internal override void InitializeArrays()
        {
            this.HeaderArray = new object[6, 13];
            this.LockArray = new object[6, 13];
            this.FormatArray = new object[6, 13];
        }

        /// <summary>
        /// Populates the format array with values
        /// </summary>
        internal override void PopulateHeaderFormatArray()
        {
            base.PopulateHeaderFormatArray();

            this.FormatArray[5, 2] = "yyyy-mm-dd hh:mm:ss";
        }

        /// <summary>
        /// Populates the content of the header array
        /// </summary>
        internal override void PopulateHeaderArray()
        {
            this.HeaderArray[0, 0] = "Engineering Model:";
            this.HeaderArray[1, 0] = "Iteration number:";
            this.HeaderArray[2, 0] = "Study Phase:";
            this.HeaderArray[3, 0] = "Domain:";
            this.HeaderArray[4, 0] = "User:";
            this.HeaderArray[5, 0] = "Rebuild Date:";
            
            this.HeaderArray[0, 2] = this.EngineeringModel.EngineeringModelSetup.Name;
            this.HeaderArray[1, 2] = this.Iteration.IterationSetup.IterationNumber;
            this.HeaderArray[2, 2] = this.EngineeringModel.EngineeringModelSetup.StudyPhase.ToString();
            
            var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Iteration);

            this.HeaderArray[3, 2] = selectedDomainOfExpertise == null ? "-" : selectedDomainOfExpertise.Name;
            this.HeaderArray[4, 2] = $"{this.Participant.Person.GivenName} {this.Participant.Person.Surname}";
            this.HeaderArray[5, 2] = DateTime.Now;
        }
    }
}