using System;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("transp", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Transp
    {
        [XmlElement("modFrete")]
        public String modFrete { get; set; }

        [XmlElement("CNPJ")]
        public String CNPJ { get; set; }

        [XmlElement("xNome")]
        public String xNome { get; set; }

        [XmlElement("IE")]
        public String IE { get; set; }

        [XmlElement("xEnder")]
        public String xEnder { get; set; }

        [XmlElement("xMun")]
        public String xMun { get; set; }

        [XmlElement("UF")]
        public String UF { get; set; }

        [XmlElement("veic_placa")]
        public String veic_placa { get; set; }

        [XmlElement("veic_UF")]
        public String veic_UF { get; set; }

        [XmlElement("Vol")]
        public Vol Vol { get; set; }

    }
}