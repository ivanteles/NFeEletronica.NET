using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("ISSQN", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Issqn
    {
        [XmlElement("vBC")]
        public string vBC { get; set; }

        [XmlElement("vAliq")]
        public string vAliq { get; set; }

        [XmlElement("vISSQN")]
        public string vISSQN { get; set; }

        [XmlElement("cMunFG")]
        public string cMunFG { get; set; }

        [XmlElement("cListServ")]
        public string cListServ { get; set; }

        [XmlElement("indISS")]
        public string indISS { get; set; }

        [XmlElement("indIncentivo")]
        public string indIncentivo { get; set; }
    }
}
