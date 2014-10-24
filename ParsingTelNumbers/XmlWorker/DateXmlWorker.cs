using System.Collections;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using ParsingTelNumbers.Config;

namespace ParsingTelNumbers.XmlWorker
{
    internal static class DateXmlWorker
    {
        private const string XmlFilePath = @"Full.xml";

        internal static string GetDate(SiteEnum site, DirectionEnum direction)
        {
            var doc = XDocument.Load(XmlFilePath);
            var att =
                (IEnumerable)
                    doc.XPathEvaluate(string.Format("//{0}/item[@id='{1}']/@date", site, direction));

            var firstOrDefault = att.Cast<XAttribute>().FirstOrDefault();

            return firstOrDefault != null ? firstOrDefault.Value : string.Empty;
        }

        internal static bool SetDate(SiteEnum site, DirectionEnum direction, string value)
        {
            try
            {
                var doc = XDocument.Load(XmlFilePath);
                doc.XPathSelectElement(string.Format("//{0}/item[@id='{1}']", site, direction)).Attribute("date").Value
                    = value;
                doc.Save(XmlFilePath);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}