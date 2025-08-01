using System;
using System.Text.RegularExpressions;
using System.Xml;
using SAPbouiCOM;

namespace SAPUtils.Extensions {
    /// <summary>
    /// Provides extension methods for the <see cref="SAPbouiCOM.BusinessObjectInfo"/> class.
    /// </summary>
    public static class BusinessObjectInfoExtensions {
        /// <summary>
        /// Extracts the document entry (DocEntry) from the given <see cref="BusinessObjectInfo"/> instance.
        /// </summary>
        /// <param name="boInfo">
        /// An instance of <see cref="BusinessObjectInfo"/> containing the object key XML string to parse.
        /// </param>
        /// <returns>
        /// The document entry (DocEntry) as an integer if successfully extracted; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method attempts to parse the "DocEntry" node from the "ObjectKey" XML provided by
        /// <paramref name="boInfo"/>. If the node does not exist in the primary path (/DocumentParams/DocEntry),
        /// it will attempt a secondary path (/StockTransferParams/DocEntry). If parsing fails in both cases,
        /// the method returns null.
        /// </remarks>
        public static int? GetDocEntry(this BusinessObjectInfo boInfo) {
            try {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(boInfo.ObjectKey);

                // ReSharper disable PossibleNullReferenceException
                string docEntry = xml.DocumentElement.SelectSingleNode("/DocumentParams/DocEntry").InnerText;
                // ReSharper restore PossibleNullReferenceException

                if (Regex.IsMatch(docEntry, @"\d+")) {
                    return Convert.ToInt32(docEntry);
                }
            }
            catch {
                try {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(boInfo.ObjectKey);

                    string docEntry = xml.DocumentElement?.SelectSingleNode("/StockTransferParams/DocEntry")?.InnerText;

                    if (docEntry != null && Regex.IsMatch(docEntry, @"\d+")) {
                        return Convert.ToInt32(docEntry);
                    }
                }
                catch {
                    // ignored
                }
            }

            return null;

        }
        /// <summary>
        /// Extracts the document entry (DocEntry) from the given <see cref="SAPbouiCOM.BusinessObject"/> instance.
        /// </summary>
        /// <param name="boInfo">
        /// An instance of <see cref="SAPbouiCOM.BusinessObject"/> containing the object key XML string to parse.
        /// </param>
        /// <returns>
        /// The document entry (DocEntry) as an integer if successfully extracted; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method attempts to parse the "DocEntry" node from the "ObjectKey" XML provided by
        /// <paramref name="boInfo"/>. If the node does not exist in the primary path (/DocumentParams/DocEntry),
        /// it will attempt a secondary path (/StockTransferParams/DocEntry). If parsing fails in both cases,
        /// the method returns null.
        /// </remarks>
        /// <seealso cref="SAPbouiCOM.BusinessObject"/>
        public static int? GetDocEntry(this BusinessObject boInfo) {
            try {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(boInfo.Key);

                // ReSharper disable PossibleNullReferenceException
                string docEntry = xml.DocumentElement.SelectSingleNode("/DocumentParams/DocEntry").InnerText;
                // ReSharper restore PossibleNullReferenceException

                if (Regex.IsMatch(docEntry, @"\d+")) {
                    return Convert.ToInt32(docEntry);
                }
            }
            catch {
                try {
                    XmlDocument xml = new XmlDocument();
                    xml.LoadXml(boInfo.Key);

                    string docEntry = xml.DocumentElement?.SelectSingleNode("/StockTransferParams/DocEntry")?.InnerText;

                    if (docEntry != null && Regex.IsMatch(docEntry, @"\d+")) {
                        return Convert.ToInt32(docEntry);
                    }
                }
                catch {
                    // ignored
                }
            }

            return null;

        }
    }
}