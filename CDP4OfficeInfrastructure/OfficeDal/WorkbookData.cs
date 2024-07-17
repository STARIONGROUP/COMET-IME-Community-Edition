// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookData.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using CDP4Common.DTO;

    /// <summary>
    /// The purpose of the <see cref="WorkbookData"/> is to serialize and deserialize <see cref="CDP4Common.EngineeringModelData.Iteration"/> to and from
    /// an custom XML part of a <see cref="Workbook"/>
    /// </summary>
    [XmlRoot("CDP4Data", Namespace = "http://cdp4data.stariongroup.eu")]
    public class WorkbookData : CustomOfficeData
    {
        /// <summary>
        /// Backing field for the <see cref="SitedirectoryData"/> property.
        /// </summary>
        [XmlIgnore]
        private string sitedirectoryData;

        /// <summary>
        /// Backing field for the <see cref="IterationData"/> property.
        /// </summary>
        [XmlIgnore]
        private string iterationData;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookData"/> class.
        /// </summary>
        public WorkbookData()
        {
            // Do nothing, required by the deserializer
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkbookData"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The iteration that needs to be stored in the <see cref="SitedirectoryData"/> property
        /// </param>
        public WorkbookData(CDP4Common.EngineeringModelData.Iteration iteration)
        {
            var factory = new WorkbookDataDtoFactory(iteration);
            factory.Process();
            
            this.SetSiteDirectoryData(factory);
            this.SetIterationData(factory);
        }

        /// <summary>
        /// Set the content of the <see cref="sitedirectoryData"/> property.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="WorkbookDataDtoFactory"/> that contains the DTO's
        /// </param>
        private void SetSiteDirectoryData(WorkbookDataDtoFactory factory)
        {
            using (var memoryStream = new MemoryStream())
            {
                this.Serializer.SerializeToStream(factory.SiteDirectoryThings, memoryStream);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    this.sitedirectoryData = reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Set the content of the <see cref="iterationData"/> property.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="WorkbookDataDtoFactory"/> that contains the DTO's
        /// </param>
        private void SetIterationData(WorkbookDataDtoFactory factory)
        {
            using (var memoryStream = new MemoryStream())
            {
                this.Serializer.SerializeToStream(factory.IterationThings, memoryStream);
                memoryStream.Position = 0;

                using (var reader = new StreamReader(memoryStream))
                {
                    this.iterationData = reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SiteDirectory"/> Data of the current <see cref="Workbook"/>
        /// </summary>
        [XmlElement(typeof(XmlCDataSection))]
        public XmlCDataSection SitedirectoryData
        {
            get
            {
                var doc = new XmlDocument();
                return doc.CreateCDataSection(this.sitedirectoryData);
            }

            set
            {
                this.sitedirectoryData = value.Value;
            }
        }

        /// <summary>
        /// Gets the <see cref="SiteDirectory"/> <see cref="Thing"/> instances.
        /// </summary>
        /// <remarks>
        /// This excludes the data that is contained in an <see cref="Iteration"/>
        /// </remarks>
        [XmlIgnore]
        public IEnumerable<Thing> SiteDirectoryThings
        {
            get
            {
                return this.Serializer.Deserialize(this.GenerateStreamFromString(this.SitedirectoryData.Value));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Iteration"/> Data of the current <see cref="Workbook"/>
        /// </summary>
        [XmlElement(typeof(XmlCDataSection))]
        public XmlCDataSection IterationData
        {
            get
            {
                var doc = new XmlDocument();
                return doc.CreateCDataSection(this.iterationData);
            }

            set
            {
                this.iterationData = value.Value;
            }
        }

        /// <summary>
        /// Gets the <see cref="Iteration"/> <see cref="Thing"/> instances.
        /// </summary>
        /// <remarks>
        /// This excludes the data that is contained in an <see cref="SiteDirectory"/>
        /// </remarks>
        [XmlIgnore]
        public IEnumerable<Thing> IterationThings
        {
            get
            {
                return this.Serializer.Deserialize(this.GenerateStreamFromString(this.IterationData.Value));
            }
        }

        /// <summary>
        /// Generate a stream from a string
        /// </summary>
        /// <param name="s">The string input</param>
        /// <returns>The stream</returns>
        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
