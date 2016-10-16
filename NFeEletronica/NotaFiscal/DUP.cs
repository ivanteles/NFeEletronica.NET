using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("dup", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class DUP
    {
        [XmlElement("nDup")]
        public String nDup { get; set; }

        [XmlElement("dVenc")]
        public String dVenc { get; set; }

        [XmlElement("vDup")]
        public String vDup { get; set; }
    }
}
