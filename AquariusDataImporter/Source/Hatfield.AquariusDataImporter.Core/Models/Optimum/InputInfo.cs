using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Hatfield.AquariusDataImporter.Core.Models.Optimum
{
    internal class InputInfo
    {
        public int Id { get; set; }
        public int DataLoggerId { get; set; }
        public string Label { get; set; }
        public string Unit { get; set; }

        public InputInfo(int id, int dataloggerid, string label, string unit)
        {
            this.Id = id;
            this.DataLoggerId = dataloggerid;
            this.Label = label;
            this.Unit = unit;
        }

        public static InputInfo[] ParseFromListAllInputsXml(string xml)
        {
            List<InputInfo> ret = new List<InputInfo>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNodeList inputNodes = doc.SelectNodes("/inputs/input");
            foreach (XmlNode inputNode in inputNodes)
            {
                int id = Int32.MinValue;
                Int32.TryParse(inputNode.SelectSingleNode("child::input-id").InnerText, out id);
                int dataloggerid = Int32.MinValue;
                Int32.TryParse(inputNode.SelectSingleNode("child::datalogger-id").InnerText, out dataloggerid);
                string label = inputNode.SelectSingleNode("child::label").InnerText.Trim();
                string unit = inputNode.SelectSingleNode("child::unit").InnerText.Trim();
                ret.Add(new InputInfo(id, dataloggerid, label, unit));
            } // foreach
            return ret.ToArray();
        }

    }
}
