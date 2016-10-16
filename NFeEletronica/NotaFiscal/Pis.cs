#region

using System.Xml.Serialization;

#endregion

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("PIS", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Pis
    {
        [XmlElement("CST")]
        public string CST { get; set; }

        [XmlElement("vBC")]
        public string vBC { get; set; }

        [XmlElement("pPIS")]
        public string pPIS { get; set; }

        [XmlElement("vPIS")]
        public string vPIS { get; set; }

        [XmlElement("qBCProd")]
        public string qBCProd { get; set; }

        [XmlElement("vAliqProd")]
        public string vAliqProd { get; set; }
    }
}