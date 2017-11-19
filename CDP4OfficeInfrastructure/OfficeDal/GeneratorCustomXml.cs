// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GeneratorCustomXml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using NetOffice.ExcelApi;
    using NetOffice.Properties;

    /// <summary>
    /// The purpose of the <see cref="GeneratorCustomXml"/> is to read the custom XML part that
    /// contains meta data about the last time the generator operated on the workbook.
    /// </summary>
    /// <remarks>
    /// custom xml parts are supported from version 2007 onwards
    /// </remarks>
    public class GeneratorCustomXml
    {
        /// <summary>
        /// The <see cref="IterationSetup"/> for which the custom XML is to be written or read.
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The <see cref="Participant"/> for which the custom XML is to be written or read.
        /// </summary>
        private Participant participant;

        /// <summary>
        /// The <see cref="Workbook"/> from which the custom XML is to be read from or written to.
        /// </summary>
        private Workbook workbook;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorCustomXml"/> class.
        /// </summary>
        /// <param name="participant">
        /// The <see cref="participant"/> for which the custom XML part has to be read or written
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/>s for which the custom XML part has to be read or written
        /// </param>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> from which the custom XML part has to be read from or written to.
        /// </param>
        public GeneratorCustomXml(Participant participant, Iteration iteration, Workbook workbook)
        {
            this.participant = participant;
            this.iteration = iteration;
            this.workbook = workbook;
        }

        /// <summary>
        /// Reads the custom XML part from a workbook
        /// </summary>
        public void Read()
        {
            var customparts = this.workbook.CustomXMLParts;
            foreach (var customXmlPart in customparts)
            {
                Console.WriteLine(customXmlPart.Id);
            }
        }

        /// <summary>
        /// Writes the custom XML part to a workbook
        /// </summary>
        public void Write()
        {
            var workbookData = new WorkbookData(this.iteration);
            var xmlstring = string.Empty;

            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(WorkbookData));
                serializer.Serialize(writer, workbookData);
                xmlstring = serializer.ToString();
            }

            var customparts = this.workbook.CustomXMLParts;
            customparts.Add(xmlstring, null);
        }
    }
}
