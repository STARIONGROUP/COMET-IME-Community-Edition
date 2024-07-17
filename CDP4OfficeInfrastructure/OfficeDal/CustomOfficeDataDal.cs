// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomOfficeDataDal.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using NetOffice.ExcelApi;
    using NetOffice.OfficeApi;

    using NLog;

    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;

    /// <summary>
    /// Abstract super class that is used to derive specific data access layer classes to read and write 
    /// custom XML parts from and to a workbook.
    /// </summary>
    /// <typeparam name="T">
    /// The type of data that is read or written
    /// </typeparam>
    public abstract class CustomOfficeDataDal<T> : ICustomXmlRead<T>, ICustomXmlWrite<T> where T : CustomOfficeData 
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="Workbook"/> that is to be read from or written to.
        /// </summary>
        private readonly Workbook workbook;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomOfficeDataDal{T}"/> class.
        /// </summary>
        /// <param name="workbook">
        /// The <see cref="Workbook"/> that is to be read from or written to.
        /// </param>
        protected CustomOfficeDataDal(Workbook workbook)
        {
            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook), "The workbook may not be null");
            }

            this.workbook = workbook;
        }

        /// <summary>
        /// Reads the custom XML part from a workbook
        /// </summary>
        /// <returns>
        /// The data that is being read
        /// </returns>
        public T Read()
        {
            CustomXMLParts xmlParts;

            try
            {
                xmlParts = this.workbook.CustomXMLParts.SelectByNamespace(GetXmlNameSpace());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }

            RemoveSuperfluousXmlParts(xmlParts);

            var part = xmlParts.SingleOrDefault();
            return part == null ? null : Deserialize(part.XML);
        }

        /// <summary>
        /// Removes all but the last XML part from the workbook.
        /// </summary>
        /// <param name="xmlParts">
        /// the <see cref="CustomXMLParts"/> in the workbook.
        /// </param>
        private static void RemoveSuperfluousXmlParts(CustomXMLParts xmlParts)
        {
            while (xmlParts.Count > 1)
            {
                // NOTE indices are 1-based here
                xmlParts[1].Delete();
            }
        }

        /// <summary>
        /// Writes the custom XML part to a workbook
        /// </summary>
        /// <param name="customOfficeData">
        /// The data that is being written
        /// </param>
        public void Write(T customOfficeData)
        {
            var xmlstring = Serialize(customOfficeData);
            this.workbook.CustomXMLParts.Add(xmlstring);

            RemoveSuperfluousXmlParts(this.workbook.CustomXMLParts.SelectByNamespace(GetXmlNameSpace()));
        }

        /// <summary>
        /// Gets the XML namespace of the custom XML part
        /// </summary>
        /// <returns>
        /// a string that contains the XML namespace
        /// </returns>
        private static string GetXmlNameSpace()
        {
            var type = typeof(T);
            var attribute = (XmlRootAttribute)Attribute.GetCustomAttribute(type, typeof(XmlRootAttribute));
            return attribute.Namespace;
        }

        /// <summary>
        /// serialize to XML the <see cref="WorkbookSession"/> data
        /// </summary>
        /// <param name="t">
        /// The data that is to be serialized
        /// </param>
        /// <returns>
        /// an XML string
        /// </returns>
        private static string Serialize(T t)
        {
            string xmlstring;

            using (var writer = new StringWriter())
            {
                var serializer = new XmlSerializer(t.GetType());
                serializer.Serialize(writer, t);
                xmlstring = writer.ToString();
            }

            return xmlstring;
        }

        /// <summary>
        /// Deserialize a WorkbookSession from XML
        /// </summary>
        /// <param name="xml">
        /// the XML string to deserialize
        /// </param>
        /// <returns>
        /// an instance of <see cref="WorkbookSession"/>
        /// </returns>
        private static T Deserialize(string xml)
        {
            T t = null;

            using (TextReader reader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                t = (T)serializer.Deserialize(reader);
            }

            return t;
        }
    }
}
