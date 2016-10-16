#region

using System.Xml.Serialization;

#endregion

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("imposto", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Ipi
    {
        [XmlElement("cIEnq")]
        public string cIEnq { get; set; }

        [XmlElement("CST")]
        public string CST { get; set; }

        [XmlElement("vBC")]
        public string vBC { get; set; }

        [XmlElement("pIPI")]
        public string pIPI { get; set; }

        [XmlElement("qUnid")]
        public string qUnid { get; set; }

        [XmlElement("vUnid")]
        public string vUnid { get; set; }

        [XmlElement("vIPI")]
        public string vIPI { get; set; }
    }
}