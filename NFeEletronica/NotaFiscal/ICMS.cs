#region

using System.Xml.Serialization;

#endregion

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("CIMS", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Icms
    {
        [XmlElement("orig")]
        public string orig { get; set; }

        [XmlElement("CST")]
        public string CST { get; set; }

        [XmlElement("modBC")]
        public string modBC { get; set; }

        [XmlElement("vBC")]
        public string vBC { get; set; }

        [XmlElement("pICMS")]
        public string pICMS { get; set; }

        [XmlElement("vICMS")]
        public string vICMS { get; set; }

        [XmlElement("modBCST")]
        public string modBCST { get; set; }

        [XmlElement("pMVAST")]
        public string pMVAST { get; set; }

        [XmlElement("pRedBCST")]
        public string pRedBCST { get; set; }

        [XmlElement("vBCST")]
        public string vBCST { get; set; }

        [XmlElement("pICMSST")]
        public string pICMSST { get; set; }

        [XmlElement("vICMSST")]
        public string vICMSST { get; set; }

        [XmlElement("pRedBC")]
        public string pRedBC { get; set; }

        [XmlElement("CSOSN")]
        public string CSOSN { get; set; }

        [XmlElement("pCredSN")]
        public string pCredSN { get; set; }

        [XmlElement("vCredICMSSN")]
        public string vCredICMSSN { get; set; }

        [XmlElement("vBCSTRet")]
        public string vBCSTRet { get; set; }

        [XmlElement("vICMSSTRet")]
        public string vICMSSTRet { get; set; }
    }
}