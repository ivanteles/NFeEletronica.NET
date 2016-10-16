using System;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("dest", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Dest
    {

        [XmlElement("CPF")]
        public string CPF { get; set; }

        [XmlElement("CNPJ")]
        public string CNPJ { get; set; }

        [XmlElement("idEstrangeiro")]
        public string idEstrangeiro { get; set; }

        [XmlElement("xNome")]
        public string xNome { get; set; }

        [XmlElement("enderDest")]
        public EnderDest EnderDest { get; set; }

        [XmlElement("fone")]
        public string fone { get; set; }

        [XmlElement("IE")]
        public string IE { get; set; }

        [XmlElement("indIEDest")]
        public string indIEDest { get; set; }

        [XmlElement("email")]
        public string email { get; set; }

    }
}