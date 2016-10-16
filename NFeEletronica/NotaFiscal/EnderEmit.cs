#region

using System.Xml.Serialization;

#endregion

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("enderEmit", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class EnderEmit
    {
        public EnderEmit()
        {
            //valores padrão
            cPais = "1058";
            xPais = "BRASIL";
        }

        [XmlElement("xLgr")]
        public string xLgr { get; set; }

        [XmlElement("nro")]
        public string nro { get; set; }

        [XmlElement("xBairro")]
        public string xBairro { get; set; }

        [XmlElement("cMun")]
        public string cMun { get; set; }

        [XmlElement("xMun")]
        public string xMun { get; set; }

        [XmlElement("UF")]
        public string UF { get; set; }

        [XmlElement("CEP")]
        public string CEP { get; set; }

        [XmlElement("cPais")]
        public string cPais { get; set; }

        [XmlElement("xPais")]
        public string xPais { get; set; }

        [XmlElement("cUF")]
        public string fone { get; set; }
    }
}