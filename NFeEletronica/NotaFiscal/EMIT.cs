using System;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("emit", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Emit
    {
        public Emit()
        {
            //valores padrão
            IE = "ISENTO";
        }

        [XmlElement("CPF")]
        public String CPF { get; set; }

        [XmlElement("CNPJ")]
        public String CNPJ { get; set; }

        [XmlElement("xNome")]
        public String xNome { get; set; }

        [XmlElement("xFant")]
        public String xFant { get; set; }

        [XmlElement("enderEmit")]
        public EnderEmit EnderEmit { get; set; }

        [XmlElement("IE")]
        public String IE { get; set; }

        [XmlElement("IM")]
        public String IM { get; set; }

        [XmlElement("CNAE")]
        public String CNAE { get; set; }

        [XmlElement("CRT")]
        public String CRT { get; set; }
    }
}