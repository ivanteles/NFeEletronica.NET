using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("prod", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Prod
    {
        public Prod()
        {
            //valores padrão
            indTot = "1";
        }

        [XmlElement("cProd")]
        public String cProd { get; set; }

        [XmlElement("cEAN")]
        public String cEAN { get; set; }

        [XmlElement("xProd")]
        public String xProd { get; set; }

        [XmlElement("NCM")]
        public String NCM { get; set; }

        [XmlElement("CFOP")]
        public String CFOP { get; set; }

        [XmlElement("uCom")]
        public String uCom { get; set; }

        [XmlElement("qCom")]
        public String qCom { get; set; }

        [XmlElement("vUnCom")]
        public String vUnCom { get; set; }

        [XmlElement("vProd")]
        public String vProd { get; set; }

        [XmlElement("cEANTrib")]
        public String cEANTrib { get; set; }

        [XmlElement("uTrib")]
        public String uTrib { get; set; }

        [XmlElement("qTrib")]
        public String qTrib { get; set; }

        [XmlElement("vUnTrib")]
        public String vUnTrib { get; set; }

        [XmlElement("vFrete")]
        public String vFrete { get; set; }

        [XmlElement("vDesc")]
        public String vDesc { get; set; }

        [XmlElement("indTot")]
        public String indTot { get; set; }

        [XmlElement("vTotTrib")]
        public String vTotTrib { get; set; }
    }
}
