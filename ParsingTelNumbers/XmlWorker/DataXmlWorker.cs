using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using ParsingTelNumbers.Config;

namespace ParsingTelNumbers.XmlWorker
{
    internal static class DataXmlWorker
    {
        private const string XmlFilePath = @"Full.xml";

        internal static IList<string> GetTels()
        {
            var doc = XDocument.Load(XmlFilePath);
            var att =
                (IEnumerable)
                    doc.XPathSelectElements("//tels/item").Select(x => x.Value);

            return att as IList<string> ?? att.Cast<string>().ToList();
        }

        internal static void SetTels(IEnumerable<InfoHolder> values)
        {
            var doc = XDocument.Load(XmlFilePath);

            doc.XPathSelectElement("//tels")
                .Add(values
                    .Select(value => new XElement("item",
                        new XAttribute("site", value.Site),
                        new XAttribute("direction", value.Direction),
                        new XAttribute("city", value.City),
                        new XAttribute("name", value.Name),
                        value.Phone)));

            doc.Save(XmlFilePath);
        }
    }
}