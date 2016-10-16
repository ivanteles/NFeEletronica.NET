#region

using System.Xml.Serialization;

#endregion

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("ICMSTot", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class IcmsTotal
    {
        [XmlElement("vBC")]
        public string vBC { get; set; }

        [XmlElement("vICMS")]
        public string vICMS { get; set; }

        [XmlElement("vBCST")]
        public string vBCST { get; set; }

        [XmlElement("vICMSDeson")]
        public string vICMSDeson { get; set; }

        [XmlElement("vST")]
        public string vST { get; set; }

        [XmlElement("vProd")]
        public string vProd { get; set; }

        [XmlElement("vFrete")]
        public string vFrete { get; set; }

        [XmlElement("vSeg")]
        public string vSeg { get; set; }

        [XmlElement("vDesc")]
        public string vDesc { get; set; }

        [XmlElement("vII")]
        public string vII { get; set; }

        [XmlElement("vIPI")]
        public string vIPI { get; set; }

        [XmlElement("vPIS")]
        public string vPIS { get; set; }

        [XmlElement("vCOFINS")]
        public string vCOFINS { get; set; }

        [XmlElement("vOutro")]
        public string vOutro { get; set; }

        [XmlElement("vNF")]
        public string vNF { get; set; }

        [XmlElement("vTotTrib")]
        public string vTotTrib { get; set; }
    }
}