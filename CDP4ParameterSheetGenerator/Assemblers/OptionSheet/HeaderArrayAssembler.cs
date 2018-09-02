// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HeaderArrayAssembler.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.OptionSheet
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4ParameterSheetGenerator.Assemblers;

    /// <summary>
    /// The purpose of the <see cref="HeaderArrayAssembler"/> is to create and populate arrays to
    /// write as header information to the Option Sheet
    /// </summary>
    public class HeaderArrayAssembler : AbstractHeaderArrayAssembler
    {
        /// <summary>
        /// The <see cref="Option"/> for which the option sheet is generated.
        /// </summary>
        private readonly Option option;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderArrayAssembler"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the parameter sheet is generated.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the option sheet is generated.
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> for which the option sheet is generated.
        /// </param>
        /// <param name="option">
        /// The <see cref="Option"/> for which the option sheet is generated.
        /// </param>
        public HeaderArrayAssembler(ISession session, Iteration iteration, Participant participant, Option option)
            : base(session, iteration, participant)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option), "the option may not be null");
            }

            this.option = option;

            this.PopulateHeaderArray();
            this.PopulateHeaderLockArray();
            this.PopulateHeaderFormatArray();
        }

        /// <summary>
        /// Initialize the arrays that will contain data that is to be written to the Parameter sheet 
        /// </summary>
        internal override void InitializeArrays()
        {
            this.HeaderArray = new object[7, 9];
            this.LockArray = new object[7, 9];
            this.FormatArray = new object[7, 9];
        }

        /// <summary>
        /// Populates the format array with values
        /// </summary>
        internal override void PopulateHeaderFormatArray()
        {
            base.PopulateHeaderFormatArray();

            this.FormatArray[6, 1] = "yyyy-mm-dd hh:mm:ss";
        }

        /// <summary>
        /// Populates the content of the header array
        /// </summary>
        internal override void PopulateHeaderArray()
        {
            this.HeaderArray[0, 0] = "Engineering Model:";
            this.HeaderArray[1, 0] = "Iteration number:";
            this.HeaderArray[2, 0] = "Option:";
            this.HeaderArray[3, 0] = "Study Phase:";
            this.HeaderArray[4, 0] = "Domain:";
            this.HeaderArray[5, 0] = "User:";
            this.HeaderArray[6, 0] = "Rebuild Date:";

            this.HeaderArray[0, 1] = this.EngineeringModel.EngineeringModelSetup.Name;
            this.HeaderArray[1, 1] = this.Iteration.IterationSetup.IterationNumber;
            this.HeaderArray[2, 1] = this.option.Name;
            this.HeaderArray[3, 1] = this.EngineeringModel.EngineeringModelSetup.StudyPhase.ToString();
            
            var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Iteration);

            this.HeaderArray[4, 1] = selectedDomainOfExpertise == null ? "-" : selectedDomainOfExpertise.Name;

            this.HeaderArray[5, 1] = $"{this.Participant.Person.GivenName} {this.Participant.Person.Surname}";
            this.HeaderArray[6, 1] = DateTime.Now;
        }
    }
}