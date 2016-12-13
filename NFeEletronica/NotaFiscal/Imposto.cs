using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("imposto", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Imposto
    {
        [XmlElement("vTotTrib")]
        public String vTotTrib { get; set; }

        [XmlElement("ICMS")]
        public Icms Icms { get; set; }

        [XmlElement("PIS")]
        public Pis Pis { get; set; }

        [XmlElement("IPI")]
        public Ipi Ipi { get; set; }

        [XmlElement("COFINS")]
        public Cofins Cofins { get; set; }

        [XmlElement("ISSQN")]
        public Issqn Issqn { get; set; }
    }
}
