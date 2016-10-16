using System;
using System.Xml.Serialization;

namespace NFeEletronica.NotaFiscal
{
    [XmlRoot("total", Namespace = "http://www.portalfiscal.inf.br/nfe", IsNullable = false)]
    public class Total
    {

        [XmlElement("ICMSTot")]
        public IcmsTotal IcmsTotal { get; set; }
    }
}