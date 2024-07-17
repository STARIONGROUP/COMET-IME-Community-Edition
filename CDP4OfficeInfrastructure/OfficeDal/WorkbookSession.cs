// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSession.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using System;
    using System.Xml.Serialization;

    using CDP4Common.CommonData;
    using CDP4Common.DTO;
    using NetOffice.ExcelApi;

    /// <summary>
    /// The purpose of the <see cref="WorkbookSession"/> class is to store session data in a <see cref="Workbook"/> as a 
    /// custom XML part, and retrieve it from the workbook.
    /// </summary>
    [XmlRoot("CDP4Session", Namespace = "http://cdp4session.stariongroup.eu")]
    public class WorkbookSession : CustomOfficeData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookSession"/> class.
        /// </summary>
        public WorkbookSession()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookSession"/> class.
        /// </summary>
        /// <param name="person">
        /// The person.
        /// </param>
        /// <param name="engineeringModelSetup">
        /// The engineering model setup.
        /// </param>
        /// <param name="iterationSetup">
        /// The iteration setup.
        /// </param>
        /// <param name="domainOfExpertise">
        /// The domain Of Expertise.
        /// </param>
        public WorkbookSession(CDP4Common.SiteDirectoryData.Person person, CDP4Common.SiteDirectoryData.EngineeringModelSetup engineeringModelSetup, CDP4Common.SiteDirectoryData.IterationSetup iterationSetup, CDP4Common.SiteDirectoryData.DomainOfExpertise domainOfExpertise)
        {
            this.RebuildDateTime = DateTime.UtcNow;
            this.Person = (Person)person.ToDto();
            this.EngineeringModelSetup = (EngineeringModelSetup)engineeringModelSetup.ToDto();
            this.IterationSetup = (IterationSetup)iterationSetup.ToDto();
            this.DomainOfExpertise = (DomainOfExpertise)domainOfExpertise.ToDto();
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> at which the workbook was last rebuilt.
        /// </summary>
        [XmlElement]
        public DateTime RebuildDateTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Person"/> that rebuilt the workbook.
        /// </summary>
        [XmlElement]
        public Person Person { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="EngineeringModelSetup"/> that corresponds to the current <see cref="Workbook"/>.
        /// </summary>
        [XmlElement]
        public EngineeringModelSetup EngineeringModelSetup { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IterationSetup"/> that corresponds to the current <see cref="Workbook"/>.
        /// </summary>
        [XmlElement]
        public IterationSetup IterationSetup { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DomainOfExpertise"/> that corresponds to the current <see cref="Workbook"/>.
        /// </summary>
        [XmlElement]
        public DomainOfExpertise DomainOfExpertise { get; set; }
    }
}
