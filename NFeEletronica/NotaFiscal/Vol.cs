using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("Vol", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Vol
    {
        [XmlElement("qVol")]
        public String qVol { get; set; }

        [XmlElement("esp")]
        public String esp { get; set; }

        [XmlElement("marca")]
        public String marca { get; set; }

        [XmlElement("nVol")]
        public String nVol { get; set; }

        [XmlElement("pesoL")]
        public String pesoL { get; set; }

        [XmlElement("pesoB")]
        public String pesoB { get; set; }
    }
}
