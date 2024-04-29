// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewHeaderArrayAssembler.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CrossViewEditor.Assemblers
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Generator;

    using CDP4Dal;

    using CDP4OfficeInfrastructure.Assemblers;

    /// <summary>
    /// The purpose of the <see cref="CrossviewHeaderArrayAssembler"/> is to create and populate arrays to
    /// write as header information to the Crossview Sheet
    /// </summary>
    public sealed class CrossviewHeaderArrayAssembler : AbstractHeaderArrayAssembler
    {
        /// <summary>
        /// Total numer of columns
        /// </summary>
        private readonly int numberOfColumns;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossviewHeaderArrayAssembler"/> class.
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> for which the crossview sheet is generated.
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> for which the crossview sheet is generated.
        /// </param>
        /// <param name="participant">
        /// The <see cref="Participant"/> for which the crossview sheet is generated.
        /// </param>
        /// <param name="numberOfColumns">Total number of columns received after computation</param>
        public CrossviewHeaderArrayAssembler(ISession session, Iteration iteration, Participant participant, int numberOfColumns)
            : base(session, iteration, participant)
        {
            this.numberOfColumns = numberOfColumns;

            this.InitializeArrays();
            this.PopulateHeaderArray();
            this.PopulateHeaderLockArray();
            this.PopulateHeaderFormatArray();
        }

        /// <summary>
        /// Initialize the arrays that will contain data that is to be written to the Parameter sheet
        /// </summary>
        public override void InitializeArrays()
        {
            this.HeaderArray = new object[6, this.numberOfColumns];
            this.LockArray = new object[6, this.numberOfColumns];
            this.FormatArray = new object[6, this.numberOfColumns];
        }

        /// <summary>
        /// Populates the format array with values
        /// </summary>
        public override void PopulateHeaderFormatArray()
        {
            base.PopulateHeaderFormatArray();

            this.FormatArray[5, 2] = CrossviewSheetConstants.HeaderDateFormat;
        }

        /// <summary>
        /// Populates the content of the header array
        /// </summary>
        public override void PopulateHeaderArray()
        {
            for (var i = 0; i < CrossviewSheetConstants.HeaderColumnNames.Length; i++)
            {
                this.HeaderArray[i, 0] = CrossviewSheetConstants.HeaderColumnNames[i];
            }

            this.HeaderArray[0, 1] = this.EngineeringModel.EngineeringModelSetup.Name;
            this.HeaderArray[1, 1] = this.Iteration.IterationSetup.IterationNumber;
            this.HeaderArray[2, 1] = this.EngineeringModel.EngineeringModelSetup.StudyPhase.ToString();

            var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(this.Iteration);

            this.HeaderArray[3, 1] = selectedDomainOfExpertise == null ? "-" : selectedDomainOfExpertise.Name;
            this.HeaderArray[4, 1] = $"{this.Participant.Person.GivenName} {this.Participant.Person.Surname}";
            this.HeaderArray[5, 1] = DateTime.Now;
        }
    }
}
