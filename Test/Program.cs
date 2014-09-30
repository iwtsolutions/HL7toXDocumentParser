using HL7toXDocumentParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            var hl7Parser = new Parser();

            var hl7 = @"FHS|^~\&|OAZIS||||20140318114932||ADT||9568180
MSH|^~\&|OAZIS||||20140318114932||ADT^A28|09568180|P|2.3||||||ASCII
EVN|A28|20140318114932||||19900101
PID|1||3612304001||Caltleau^Monu^^^Mevrouw||19361230|F|||Residence Charles 477^^ORCUE^^7101^BE^H^^^Y||||NL^^^NL|0||||||||||BE||||N
PD1||||||||||||N  
PV1|1|N|||||||||||||||||||||||||||||||||||||N
FTS|0|9568180";

            var doc = hl7Parser.Parse(hl7);

            hl7 = @"MSH|^~\&|OAZIS||BIZTALK||20140318115439||ADT^A25|09568240|P|2.3||||||ASCII
EVN|A25|20140318115439||||201403182330
PID|1||3410131027|34101315545^^^^NN|Vanhubulcke^Victor^^^Meneer|Vermeersch^Marten|19341015|M|||Pereboomstraat 15^^STADEN^^8880^BE^H||01/56 65 99^^PH~0494/71 66 14ei^^CP||NL|M||80398500^^^^VN|0000000000|34101315545||||||BE||||N
PD1||||003809^MATTELIN^GUY||||||||N
PV1|1|O|5551^001^01^WIL|NULL|||003809^MATTELIN^GUY||000573^Bourgeois^Karel|1851|||||||000573^Bourgeois^Karel|0|80398500^^^^VN|3^20140318||||||||||||||||1|1||D|||||201403180830|201403181140";

            // Test odd subcomponent combinations.
            doc = hl7Parser.Parse(hl7);
            hl7 = getSampleHL7("sub_components.hl7");
            var xml = hl7Parser.Parse(hl7).ToXmlDocument();

            xml.testValue("//OBR.1.1", "Data");
            xml.testValue("//OBR.1.2", "here");
            xml.testValue("//OBR.2.1", "other");
            xml.testValue("//OBR.2.2.1", "data");
            xml.testValue("//OBR.3.1.2", "subbed");
            xml.testValue("//OBR.3.1.3", "here");
            xml.testValue("//OBR.3.3", "theresmore");
            xml.testValue("//OBR.4.4", "88282");
            xml.testValue("//OBR.6/OBR.6.1/OBR.6.1.1", "087234");
            xml.testValue("//OBR.6/OBR.6.1/OBR.6.1.3", "uu181");
            xml.testValue("//OBR.6/OBR.6.3", "777");
            xml.testValue("//OBR.7", "another");

            Console.ReadKey();
        }

        #region IWTSolutionsTesting
        static void testValue(this XmlDocument xml, string xPath, string expectedValue, int index = 0)
        {
            // Some legacy crap from Kinexions.
            XmlNodeList NodeList = default(XmlNodeList);
            XmlNode node = default(XmlNode);
            string Value = null;

            if (string.IsNullOrWhiteSpace(xPath))
                assert(xPath, string.Empty, expectedValue);
            else if (xPath.IndexOf("static", System.StringComparison.OrdinalIgnoreCase) > -1)
                assert(xPath, xPath.Substring(xPath.LastIndexOf("/") + 1), expectedValue);

            NodeList = xml.SelectNodes(xPath);
            node = NodeList[index];

            Value = getValueFromNode(node);

            if (string.IsNullOrEmpty(Value))
            {
                node = NodeList[0];
                Value = getValueFromNode(node);
            }

            assert(xPath, Value, expectedValue);
        }

        private static string getValueFromNode(XmlNode node)
        {
            string value = string.Empty;

            if ((node != null))
            {
                if (node.HasChildNodes)
                {
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        value += getValueFromNode(childNode);

                        if (childNode.Name.Contains("SC"))
                        {
                            value += "&";
                        }
                        else
                        {
                            value += "^";
                        }
                    }
                    value = value.TrimEnd('&', '^');
                }
                else
                {
                    value = node.InnerText.Replace("\\.br\\", "<br>");
                }
            }
            return value;
        }

        static void assert(string xPath, string first, string second)
        {
            Console.WriteLine("{0}: {1}", xPath, first == second);
        }

        static string getSampleHL7(string name)
        {
            return System.IO.File.ReadAllText(@"..\Examples\" + name);
        }
                
        static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using(var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }
#endregion

    }
}
