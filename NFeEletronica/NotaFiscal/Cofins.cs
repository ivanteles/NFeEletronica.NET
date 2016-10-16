using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("COFINS", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Cofins
    {
        [XmlElement("CST")]
        public String CST { get; set; }

        [XmlElement("vBC")]
        public String vBC { get; set; }

        [XmlElement("pCOFINS")]
        public String pCOFINS { get; set; }

        [XmlElement("vCOFINS")]
        public String vCOFINS { get; set; }

        [XmlElement("qBCProd")]
        public String qBCProd { get; set; }

        [XmlElement("vAliqProd")]
        public String vAliqProd { get; set; }
    }
}
