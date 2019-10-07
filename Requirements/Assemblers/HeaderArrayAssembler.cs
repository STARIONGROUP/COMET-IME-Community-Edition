// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeaderArrayAssembler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Assemblers
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4OfficeInfrastructure.Assemblers;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="HeaderArrayAssembler"/> is to create and populate arrays to
    /// write as header information to the Parameter Sheet
    /// </summary>
    public class HeaderArrayAssembler : AbstractHeaderArrayAssembler
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

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
        public override void InitializeArrays()
        {
            logger.Debug("Initializing Arrays");
            this.HeaderArray = new object[5, 13];
            this.LockArray = new object[5, 13];
            this.FormatArray = new object[5, 13];
        }

        /// <summary>
        /// Populates the format array with values
        /// </summary>
        public override void PopulateHeaderFormatArray()
        {
            logger.Debug("Populating format array");
            base.PopulateHeaderFormatArray();
            this.FormatArray[4, 2] = "yyyy-mm-dd hh:mm:ss";
        }

        /// <summary>
        /// Populates the content of the header array
        /// </summary>
        public override void PopulateHeaderArray()
        {
            logger.Debug("Populating header array with content");

            this.HeaderArray[0, 0] = "Engineering Model:";
            this.HeaderArray[1, 0] = "Iteration number:";
            this.HeaderArray[2, 0] = "Domain:";
            this.HeaderArray[3, 0] = "User:";
            this.HeaderArray[4, 0] = "Generation Date:";

            this.HeaderArray[0, 2] = $"{this.EngineeringModel.EngineeringModelSetup.Name} [{this.EngineeringModel.EngineeringModelSetup.ShortName}]";
            this.HeaderArray[1, 2] = this.Iteration.IterationSetup.IterationNumber;
            var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Iteration);
            this.HeaderArray[2, 2] = selectedDomainOfExpertise == null ? "-" : $"{selectedDomainOfExpertise.Name} [{selectedDomainOfExpertise.ShortName}]";
            this.HeaderArray[3, 2] = $"{this.Participant.Person.GivenName} {this.Participant.Person.Surname}";
            this.HeaderArray[4, 2] = DateTime.Now;
        }
    }
}